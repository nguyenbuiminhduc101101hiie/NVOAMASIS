using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services;

public static class BkavInvoiceCommandTypes
{
    public const int CreateInvoiceMT = 100;
    public const int CreateInvoiceTR = 101;
    public const int CreateInvoiceWithFormSerial = 110;
    public const int CreateInvoiceWithFormSerialNo = 111;
    public const int CreateInvoiceReplace = 120;
    public const int CreateInvoiceAdjust = 121;
    public const int UpdateInvoiceByPartnerInvoiceID = 200;
    public const int CancelInvoiceByInvoiceGUID = 201;
    public const int CancelInvoiceByPartnerInvoiceID = 202;
    public const int DeleteInvoiceByPartnerInvoiceID = 301;
    public const int DeleteInvoiceByInvoiceGUID = 303;
    public const int GetInvoiceDataWS = 800;
    public const int GetInvoiceStatusID = 801;
    public const int GetInvoiceHistory = 802;
    public const int GetInvoiceLink = 804;
    public const int GetInvoiceDataFilePDF = 808;
    public const int GetInvoiceDataFileXML = 809;
    public const int CreateAccount = 902;
    public const int GetUnitInforByTaxCode = 904;
    public const int GetDLLContent = 1001;
    public const int UpdateInvoiceByInvoiceGUID = 204;

    public static readonly int[] SupportedCreateCommandTypes =
    [
        CreateInvoiceMT,
        CreateInvoiceTR,
        CreateInvoiceWithFormSerial,
        CreateInvoiceWithFormSerialNo
    ];

    public static bool IsSupportedCreateCommandType(int commandType)
        => SupportedCreateCommandTypes.Contains(commandType);
}

public class BkavInvoiceActionInput
{
    public int CommandType { get; set; } = BkavInvoiceCommandTypes.CreateInvoiceTR;
    public string? InvoiceForm { get; set; }
    public string? InvoiceSerial { get; set; }
    public int? InvoiceNo { get; set; }
    public string? OriginalInvoiceIdentify { get; set; }
    public int PayMethodID { get; set; } = 3;
    public string? TaxCode { get; set; }
}

public class BkavInvoiceRuntimeCredentials
{
    public string? PartnerGuid { get; set; }
    public string? PartnerToken { get; set; }
}

public class BkavInvoiceGroup
{
    public string InternalInvoiceNo { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string? ElectronicInvoiceNo { get; set; }
    public string? Currency { get; set; }
    public double TotalBeforeTax { get; set; }
    public double TotalTax { get; set; }
    public double TotalAfterTax { get; set; }
    public long? BkavPartnerInvoiceID { get; set; }
    public string? BkavPartnerInvoiceStringID { get; set; }
    public string? BkavInvoiceGUID { get; set; }
    public int? BkavInvoiceNo { get; set; }
    public string? BkavInvoiceForm { get; set; }
    public string? BkavInvoiceSerial { get; set; }
    public string? BkavInvoiceLink { get; set; }
    public string? BkavPdfPath { get; set; }
    public string? BkavXmlPath { get; set; }
    public int? BkavStatusID { get; set; }
    public string? BkavLastMessage { get; set; }
    public List<M_HoaDonDauRa> Lines { get; set; } = [];
}

public class BkavInvoiceOperationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public BkavInvoiceGroup? Group { get; init; }
    public string? Link { get; init; }
    public string? FilePath { get; init; }
    public string? ObjectText { get; init; }

    public static BkavInvoiceOperationResult Ok(string message, BkavInvoiceGroup? group = null, string? link = null, string? filePath = null, string? objectText = null)
        => new() { Success = true, Message = message, Group = group, Link = link, FilePath = filePath, ObjectText = objectText };

    public static BkavInvoiceOperationResult Fail(string message, BkavInvoiceGroup? group = null, string? objectText = null)
        => new() { Success = false, Message = message, Group = group, ObjectText = objectText };
}

public class BkavInvoiceService(
    AppDbContext context,
    HttpClient httpClient,
    IWebHostEnvironment webHostEnvironment,
    IOptions<BkavInvoiceSettings> options)
{
    private const long XmlMode = 1;
    private const long ZipMode = 2;
    private const long EncryptMode = 4;
    private const long EncryptModeV1 = 8;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null,
        WriteIndented = false
    };

    private readonly BkavInvoiceSettings settings = options.Value;

    private sealed record EffectiveBkavCredentials(string PartnerGuid, string PartnerToken);

    public int DefaultCreateCommandType => settings.DefaultCreateCommandType;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(settings.ServiceUrl) &&
        !string.IsNullOrWhiteSpace(settings.PartnerGuid) &&
        !string.IsNullOrWhiteSpace(settings.PartnerToken);

    public bool HasServerCredentials =>
        !string.IsNullOrWhiteSpace(settings.PartnerGuid) &&
        !string.IsNullOrWhiteSpace(settings.PartnerToken);

    public bool IsConfiguredWith(BkavInvoiceRuntimeCredentials? credentials)
        => string.IsNullOrWhiteSpace(ValidateSettings(credentials));

    public async Task<List<BkavInvoiceGroup>> GetInvoiceGroupsAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await context.HoaDonDauRa
            .AsNoTracking()
            .Where(x => x.continued == true && x.sohoadonNoibo != null && x.sohoadonNoibo != string.Empty)
            .OrderByDescending(x => x.ngayphathanhhoadonDientu)
            .ThenByDescending(x => x.dateupdate)
            .ToListAsync(cancellationToken);

        var customerIds = invoices.Select(x => x.customerid).Distinct().ToHashSet();
        var customers = (await context.Customer
            .AsNoTracking()
            .ToListAsync(cancellationToken))
            .Where(x => customerIds.Contains(x.Customer_ID))
            .ToDictionary(x => x.Customer_ID);

        return invoices
            .GroupBy(x => new { InternalInvoiceNo = Clean(x.sohoadonNoibo), x.customerid })
            .Where(x => !string.IsNullOrWhiteSpace(x.Key.InternalInvoiceNo))
            .Select(x => BuildGroup(x.Key.InternalInvoiceNo, x.Key.customerid, x.ToList(), customers))
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.InternalInvoiceNo)
            .ToList();
    }

    public async Task<BkavInvoiceOperationResult> IssueInvoiceGroupAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceActionInput input, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var settingsError = ValidateSettings(credentials);
            if (!string.IsNullOrEmpty(settingsError))
                return BkavInvoiceOperationResult.Fail(settingsError);

            if (!BkavInvoiceCommandTypes.IsSupportedCreateCommandType(input.CommandType))
                return BkavInvoiceOperationResult.Fail($"Unsupported BKAV create command type: {input.CommandType}");

            var validation = ValidateInvoiceNumberCommand(input.CommandType, input);
            if (!string.IsNullOrEmpty(validation))
                return BkavInvoiceOperationResult.Fail(validation);

            var state = await LoadGroupStateAsync(internalInvoiceNo, customerId, cancellationToken);
            if (!string.IsNullOrEmpty(state.Error))
                return BkavInvoiceOperationResult.Fail(state.Error, state.Group);

            if (state.Lines.Any(x => !string.IsNullOrWhiteSpace(x.BkavInvoiceGUID) || !string.IsNullOrWhiteSpace(x.sohoadonDientu)))
                return BkavInvoiceOperationResult.Fail("HoaDonDauRa group already has e-invoice data.", state.Group);

            var invoiceData = BuildInvoiceData(state, input.CommandType, input);
            var commandResult = await ExecuteCommandAsync(input.CommandType, SerializeInvoiceDataList([invoiceData]), credentials, cancellationToken);
            if (!commandResult.Success)
            {
                await SetLastMessageAsync(state.Lines, commandResult.Message, cancellationToken);
                return BkavInvoiceOperationResult.Fail(commandResult.Message, state.Group, commandResult.ObjectText);
            }

            var invoiceResult = ParseFirstInvoiceResult(commandResult.ObjectText);
            if (invoiceResult == null)
                return BkavInvoiceOperationResult.Fail("BKAV did not return invoice result.", state.Group, commandResult.ObjectText);

            if (invoiceResult.Status != 0)
            {
                await SetLastMessageAsync(state.Lines, invoiceResult.MessLog, cancellationToken);
                return BkavInvoiceOperationResult.Fail(FirstNonEmpty(invoiceResult.MessLog, "BKAV returned invoice error."), state.Group, commandResult.ObjectText);
            }

            await ApplyInvoiceResultAsync(state.Lines, invoiceResult, invoiceData.PartnerInvoiceID, cancellationToken);
            return BkavInvoiceOperationResult.Ok("BKAV invoice issued successfully.", await ReloadGroupAsync(internalInvoiceNo, customerId, cancellationToken), objectText: commandResult.ObjectText);
        }
        catch (Exception ex)
        {
            return BkavInvoiceOperationResult.Fail(ex.Message);
        }
    }

    public Task<BkavInvoiceOperationResult> UpdateByPartnerInvoiceIDAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceActionInput input, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
        => SendInvoiceDataCommandAsync(internalInvoiceNo, customerId, BkavInvoiceCommandTypes.UpdateInvoiceByPartnerInvoiceID, input, false, "BKAV invoice updated by PartnerInvoiceID.", credentials, cancellationToken);

    public Task<BkavInvoiceOperationResult> UpdateByInvoiceGuidAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceActionInput input, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
        => SendInvoiceDataCommandAsync(internalInvoiceNo, customerId, BkavInvoiceCommandTypes.UpdateInvoiceByInvoiceGUID, input, true, "BKAV invoice updated by InvoiceGUID.", credentials, cancellationToken);

    public Task<BkavInvoiceOperationResult> ReplaceInvoiceAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceActionInput input, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
        => SendInvoiceDataCommandAsync(internalInvoiceNo, customerId, BkavInvoiceCommandTypes.CreateInvoiceReplace, input, false, "BKAV replacement invoice created.", credentials, cancellationToken);

    public Task<BkavInvoiceOperationResult> AdjustInvoiceAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceActionInput input, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
        => SendInvoiceDataCommandAsync(internalInvoiceNo, customerId, BkavInvoiceCommandTypes.CreateInvoiceAdjust, input, false, "BKAV adjustment invoice created.", credentials, cancellationToken);

    public Task<BkavInvoiceOperationResult> CancelByInvoiceGuidAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
        => SendLookupCommandAsync(internalInvoiceNo, customerId, BkavInvoiceCommandTypes.CancelInvoiceByInvoiceGUID, true, "BKAV invoice cancelled by InvoiceGUID.", credentials, cancellationToken);

    public Task<BkavInvoiceOperationResult> CancelByPartnerInvoiceIDAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
        => SendLookupCommandAsync(internalInvoiceNo, customerId, BkavInvoiceCommandTypes.CancelInvoiceByPartnerInvoiceID, false, "BKAV invoice cancelled by PartnerInvoiceID.", credentials, cancellationToken);

    public Task<BkavInvoiceOperationResult> DeleteByInvoiceGuidAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
        => SendLookupCommandAsync(internalInvoiceNo, customerId, BkavInvoiceCommandTypes.DeleteInvoiceByInvoiceGUID, true, "BKAV invoice deleted by InvoiceGUID.", credentials, cancellationToken);

    public Task<BkavInvoiceOperationResult> DeleteByPartnerInvoiceIDAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
        => SendLookupCommandAsync(internalInvoiceNo, customerId, BkavInvoiceCommandTypes.DeleteInvoiceByPartnerInvoiceID, false, "BKAV invoice deleted by PartnerInvoiceID.", credentials, cancellationToken);

    public async Task<BkavInvoiceOperationResult> GetInvoiceDataAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var state = await LoadGroupStateAsync(internalInvoiceNo, customerId, cancellationToken);
            if (!string.IsNullOrEmpty(state.Error))
                return BkavInvoiceOperationResult.Fail(state.Error, state.Group);

            var invoiceGuid = GetInvoiceGuid(state.Lines);
            if (string.IsNullOrWhiteSpace(invoiceGuid))
                return BkavInvoiceOperationResult.Fail("BKAV InvoiceGUID is empty.", state.Group);

            var result = await ExecuteCommandAsync(BkavInvoiceCommandTypes.GetInvoiceDataWS, invoiceGuid, credentials, cancellationToken);
            if (!result.Success)
                return BkavInvoiceOperationResult.Fail(result.Message, state.Group, result.ObjectText);

            var invoiceData = DeserializeCommandObject<BkavInvoiceDataWS>(result.ObjectText);
            if (invoiceData?.Invoice != null)
            {
                foreach (var line in state.Lines)
                {
                    line.BkavInvoiceNo = invoiceData.Invoice.InvoiceNo > 0 ? invoiceData.Invoice.InvoiceNo : line.BkavInvoiceNo;
                    line.BkavInvoiceForm = FirstNonEmpty(invoiceData.Invoice.InvoiceForm, line.BkavInvoiceForm);
                    line.BkavInvoiceSerial = FirstNonEmpty(invoiceData.Invoice.InvoiceSerial, line.BkavInvoiceSerial);
                    line.BkavLastMessage = "Invoice data refreshed.";
                    line.dateupdate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                }

                await context.SaveChangesAsync(cancellationToken);
            }

            return BkavInvoiceOperationResult.Ok("BKAV invoice data refreshed.", await ReloadGroupAsync(internalInvoiceNo, customerId, cancellationToken), objectText: result.ObjectText);
        }
        catch (Exception ex)
        {
            return BkavInvoiceOperationResult.Fail(ex.Message);
        }
    }

    public async Task<BkavInvoiceOperationResult> RefreshStatusAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var state = await LoadGroupStateAsync(internalInvoiceNo, customerId, cancellationToken);
            if (!string.IsNullOrEmpty(state.Error))
                return BkavInvoiceOperationResult.Fail(state.Error, state.Group);

            var invoiceGuid = GetInvoiceGuid(state.Lines);
            if (string.IsNullOrWhiteSpace(invoiceGuid))
                return BkavInvoiceOperationResult.Fail("BKAV InvoiceGUID is empty.", state.Group);

            var result = await ExecuteCommandAsync(BkavInvoiceCommandTypes.GetInvoiceStatusID, invoiceGuid, credentials, cancellationToken);
            if (!result.Success)
                return BkavInvoiceOperationResult.Fail(result.Message, state.Group, result.ObjectText);

            if (int.TryParse(result.ObjectText.Trim('"'), NumberStyles.Integer, CultureInfo.InvariantCulture, out var statusId))
            {
                foreach (var line in state.Lines)
                    line.BkavStatusID = statusId;
            }

            await SetLastMessageAsync(state.Lines, result.ObjectText, cancellationToken);
            return BkavInvoiceOperationResult.Ok("BKAV status refreshed.", await ReloadGroupAsync(internalInvoiceNo, customerId, cancellationToken), objectText: result.ObjectText);
        }
        catch (Exception ex)
        {
            return BkavInvoiceOperationResult.Fail(ex.Message);
        }
    }

    public async Task<BkavInvoiceOperationResult> GetHistoryAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var state = await LoadGroupStateAsync(internalInvoiceNo, customerId, cancellationToken);
            if (!string.IsNullOrEmpty(state.Error))
                return BkavInvoiceOperationResult.Fail(state.Error, state.Group);

            var invoiceGuid = GetInvoiceGuid(state.Lines);
            if (string.IsNullOrWhiteSpace(invoiceGuid))
                return BkavInvoiceOperationResult.Fail("BKAV InvoiceGUID is empty.", state.Group);

            var result = await ExecuteCommandAsync(BkavInvoiceCommandTypes.GetInvoiceHistory, invoiceGuid, credentials, cancellationToken);
            if (!result.Success)
                return BkavInvoiceOperationResult.Fail(result.Message, state.Group, result.ObjectText);

            await SetLastMessageAsync(state.Lines, result.ObjectText, cancellationToken);
            return BkavInvoiceOperationResult.Ok("BKAV history received.", await ReloadGroupAsync(internalInvoiceNo, customerId, cancellationToken), objectText: result.ObjectText);
        }
        catch (Exception ex)
        {
            return BkavInvoiceOperationResult.Fail(ex.Message);
        }
    }

    public async Task<BkavInvoiceOperationResult> GetInvoiceLinkAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var state = await LoadGroupStateAsync(internalInvoiceNo, customerId, cancellationToken);
            if (!string.IsNullOrEmpty(state.Error))
                return BkavInvoiceOperationResult.Fail(state.Error, state.Group);

            var result = await ExecuteCommandAsync(BkavInvoiceCommandTypes.GetInvoiceLink, SerializeInvoiceDataList([BuildPartnerLookup(state)]), credentials, cancellationToken);
            if (!result.Success)
                return BkavInvoiceOperationResult.Fail(result.Message, state.Group, result.ObjectText);

            var invoiceResult = ParseFirstInvoiceResult(result.ObjectText);
            if (invoiceResult == null)
                return BkavInvoiceOperationResult.Fail("BKAV did not return invoice link result.", state.Group, result.ObjectText);

            if (invoiceResult.Status != 0)
                return BkavInvoiceOperationResult.Fail(FirstNonEmpty(invoiceResult.MessLog, "BKAV returned link error."), state.Group, result.ObjectText);

            foreach (var line in state.Lines)
            {
                line.BkavInvoiceLink = invoiceResult.MessLog;
                line.BkavLastMessage = invoiceResult.MessLog;
                line.dateupdate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            }

            await context.SaveChangesAsync(cancellationToken);
            return BkavInvoiceOperationResult.Ok("BKAV invoice link received.", await ReloadGroupAsync(internalInvoiceNo, customerId, cancellationToken), invoiceResult.MessLog, objectText: result.ObjectText);
        }
        catch (Exception ex)
        {
            return BkavInvoiceOperationResult.Fail(ex.Message);
        }
    }

    public Task<BkavInvoiceOperationResult> DownloadPdfAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
        => DownloadInvoiceFileAsync(internalInvoiceNo, customerId, BkavInvoiceCommandTypes.GetInvoiceDataFilePDF, "PDF", "pdf", credentials, cancellationToken);

    public Task<BkavInvoiceOperationResult> DownloadXmlAsync(string internalInvoiceNo, Guid customerId, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
        => DownloadInvoiceFileAsync(internalInvoiceNo, customerId, BkavInvoiceCommandTypes.GetInvoiceDataFileXML, "XML", "xml", credentials, cancellationToken);

    public async Task<BkavInvoiceOperationResult> GetUnitInfoByTaxCodeAsync(string taxCode, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var settingsError = ValidateSettings(credentials);
            if (!string.IsNullOrEmpty(settingsError))
                return BkavInvoiceOperationResult.Fail(settingsError);
            if (string.IsNullOrWhiteSpace(taxCode))
                return BkavInvoiceOperationResult.Fail("Tax code is required.");

            var result = await ExecuteCommandAsync(BkavInvoiceCommandTypes.GetUnitInforByTaxCode, taxCode.Trim(), credentials, cancellationToken);
            return result.Success
                ? BkavInvoiceOperationResult.Ok("BKAV tax information received.", objectText: result.ObjectText)
                : BkavInvoiceOperationResult.Fail(result.Message, objectText: result.ObjectText);
        }
        catch (Exception ex)
        {
            return BkavInvoiceOperationResult.Fail(ex.Message);
        }
    }

    public async Task<BkavInvoiceOperationResult> CreateDemoAccountAsync(BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var settingsError = ValidateSettings(credentials);
            if (!string.IsNullOrEmpty(settingsError))
                return BkavInvoiceOperationResult.Fail(settingsError);

            var accountInfo = new BkavCreateAccountInfoFromPartner
            {
                TaxCode = DateTime.Now.ToString("ddMMyyHHmmss", CultureInfo.InvariantCulture),
                UnitName = "Cong ty BKAV",
                UnitAddress = "Toa nha HH1, P. Yen Hoa, Q. Cau Giay, Ha Noi",
                TaxDepartmentID = 1,
                UnitPersonRepresent = "Nguyen Van A",
                UnitPersonRepresentPosition = "Giam Doc",
                UnitEmail = "demo@bkav.com",
                UnitPhone = "1234567890",
                BankAccount = "2222222222",
                BankName = "BIDV",
                BrandName = "BKAV",
                DomainCheckInvoice = "tracuu.ehoadon.vn"
            };

            var result = await ExecuteCommandAsync(BkavInvoiceCommandTypes.CreateAccount, SerializeCommandObject(accountInfo), credentials, cancellationToken);
            if (!result.Success)
                return BkavInvoiceOperationResult.Fail(result.Message, objectText: result.ObjectText);

            var accountResult = DeserializeCommandObject<BkavAccountResult>(result.ObjectText);
            var message = accountResult == null ? "BKAV demo account created." : $"Account: {accountResult.Account}/{accountResult.Password}";
            return BkavInvoiceOperationResult.Ok(message, objectText: result.ObjectText);
        }
        catch (Exception ex)
        {
            return BkavInvoiceOperationResult.Fail(ex.Message);
        }
    }

    public async Task<BkavInvoiceOperationResult> DownloadDllContentAsync(string taxCode, BkavInvoiceRuntimeCredentials? credentials = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var settingsError = ValidateSettings(credentials);
            if (!string.IsNullOrEmpty(settingsError))
                return BkavInvoiceOperationResult.Fail(settingsError);
            if (string.IsNullOrWhiteSpace(taxCode))
                return BkavInvoiceOperationResult.Fail("Tax code is required.");

            var result = await ExecuteCommandAsync(BkavInvoiceCommandTypes.GetDLLContent, taxCode.Trim(), credentials, cancellationToken);
            if (!result.Success)
                return BkavInvoiceOperationResult.Fail(result.Message, objectText: result.ObjectText);

            var dllInfo = DeserializeCommandObject<BkavDllInfo>(result.ObjectText);
            if (dllInfo?.DLLContent == null || dllInfo.DLLContent.Length == 0 || string.IsNullOrWhiteSpace(dllInfo.DLLName))
                return BkavInvoiceOperationResult.Ok("BKAV DLL info received.", objectText: result.ObjectText);

            var relativePath = await SaveOutputFileAsync(dllInfo.DLLContent, $"dll/{SanitizeFileName(dllInfo.DLLName)}", cancellationToken);
            return BkavInvoiceOperationResult.Ok($"BKAV DLL saved: {relativePath}", filePath: relativePath, objectText: result.ObjectText);
        }
        catch (Exception ex)
        {
            return BkavInvoiceOperationResult.Fail(ex.Message);
        }
    }

    public static int MapTaxRateId(double? taxPercent)
    {
        var rounded = Math.Round(taxPercent.GetValueOrDefault(), 2);
        if (rounded == 0)
            return 1;
        if (rounded == 5)
            return 2;
        if (rounded == 10)
            return 3;
        if (rounded == 8)
            return 9;
        return 6;
    }

    public static long GenerateStablePartnerInvoiceId(string internalInvoiceNo, Guid customerId)
    {
        var key = $"{Clean(internalInvoiceNo).ToUpperInvariant()}|{customerId:D}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var value = BitConverter.ToInt64(hash, 0) & long.MaxValue;
        return 1_000_000_000_000_000L + value % 8_999_999_999_999_999L;
    }

    private async Task<BkavInvoiceOperationResult> SendInvoiceDataCommandAsync(
        string internalInvoiceNo,
        Guid customerId,
        int commandType,
        BkavInvoiceActionInput input,
        bool lookupByGuid,
        string successMessage,
        BkavInvoiceRuntimeCredentials? credentials,
        CancellationToken cancellationToken)
    {
        try
        {
            var settingsError = ValidateSettings(credentials);
            if (!string.IsNullOrEmpty(settingsError))
                return BkavInvoiceOperationResult.Fail(settingsError);

            var validation = ValidateOriginalIdentifyCommand(commandType, input);
            if (!string.IsNullOrEmpty(validation))
                return BkavInvoiceOperationResult.Fail(validation);

            var state = await LoadGroupStateAsync(internalInvoiceNo, customerId, cancellationToken);
            if (!string.IsNullOrEmpty(state.Error))
                return BkavInvoiceOperationResult.Fail(state.Error, state.Group);

            var invoiceData = BuildInvoiceData(state, commandType, input);
            if (lookupByGuid)
            {
                var invoiceGuid = GetInvoiceGuid(state.Lines);
                if (string.IsNullOrWhiteSpace(invoiceGuid) || !Guid.TryParse(invoiceGuid, out var parsedGuid))
                    return BkavInvoiceOperationResult.Fail("BKAV InvoiceGUID is empty or invalid.", state.Group);

                invoiceData.Invoice.InvoiceGUID = parsedGuid;
            }

            var result = await ExecuteCommandAsync(commandType, SerializeInvoiceDataList([invoiceData]), credentials, cancellationToken);
            if (!result.Success)
            {
                await SetLastMessageAsync(state.Lines, result.Message, cancellationToken);
                return BkavInvoiceOperationResult.Fail(result.Message, state.Group, result.ObjectText);
            }

            var invoiceResult = ParseFirstInvoiceResult(result.ObjectText);
            if (invoiceResult != null)
            {
                if (invoiceResult.Status != 0)
                {
                    await SetLastMessageAsync(state.Lines, invoiceResult.MessLog, cancellationToken);
                    return BkavInvoiceOperationResult.Fail(FirstNonEmpty(invoiceResult.MessLog, "BKAV returned invoice error."), state.Group, result.ObjectText);
                }

                await ApplyInvoiceResultAsync(state.Lines, invoiceResult, invoiceData.PartnerInvoiceID, cancellationToken);
            }
            else
            {
                await SetLastMessageAsync(state.Lines, successMessage, cancellationToken);
            }

            return BkavInvoiceOperationResult.Ok(successMessage, await ReloadGroupAsync(internalInvoiceNo, customerId, cancellationToken), objectText: result.ObjectText);
        }
        catch (Exception ex)
        {
            return BkavInvoiceOperationResult.Fail(ex.Message);
        }
    }

    private async Task<BkavInvoiceOperationResult> SendLookupCommandAsync(
        string internalInvoiceNo,
        Guid customerId,
        int commandType,
        bool lookupByGuid,
        string successMessage,
        BkavInvoiceRuntimeCredentials? credentials,
        CancellationToken cancellationToken)
    {
        try
        {
            var settingsError = ValidateSettings(credentials);
            if (!string.IsNullOrEmpty(settingsError))
                return BkavInvoiceOperationResult.Fail(settingsError);

            var state = await LoadGroupStateAsync(internalInvoiceNo, customerId, cancellationToken);
            if (!string.IsNullOrEmpty(state.Error))
                return BkavInvoiceOperationResult.Fail(state.Error, state.Group);

            BkavInvoiceDataWS lookup;
            if (lookupByGuid)
            {
                var invoiceGuid = GetInvoiceGuid(state.Lines);
                if (string.IsNullOrWhiteSpace(invoiceGuid) || !Guid.TryParse(invoiceGuid, out var parsedGuid))
                    return BkavInvoiceOperationResult.Fail("BKAV InvoiceGUID is empty or invalid.", state.Group);

                lookup = new BkavInvoiceDataWS { Invoice = new BkavInvoiceWS { InvoiceGUID = parsedGuid } };
            }
            else
            {
                lookup = BuildPartnerLookup(state);
            }

            var result = await ExecuteCommandAsync(commandType, SerializeInvoiceDataList([lookup]), credentials, cancellationToken);
            if (!result.Success)
                return BkavInvoiceOperationResult.Fail(result.Message, state.Group, result.ObjectText);

            var invoiceResult = ParseFirstInvoiceResult(result.ObjectText);
            var message = invoiceResult == null ? successMessage : FirstNonEmpty(invoiceResult.MessLog, successMessage);
            if (invoiceResult is { Status: not 0 })
                return BkavInvoiceOperationResult.Fail(message, state.Group, result.ObjectText);

            await SetLastMessageAsync(state.Lines, message, cancellationToken);
            return BkavInvoiceOperationResult.Ok(message, await ReloadGroupAsync(internalInvoiceNo, customerId, cancellationToken), objectText: result.ObjectText);
        }
        catch (Exception ex)
        {
            return BkavInvoiceOperationResult.Fail(ex.Message);
        }
    }

    private async Task<BkavInvoiceOperationResult> DownloadInvoiceFileAsync(
        string internalInvoiceNo,
        Guid customerId,
        int commandType,
        string objectPropertyName,
        string extension,
        BkavInvoiceRuntimeCredentials? credentials,
        CancellationToken cancellationToken)
    {
        try
        {
            var settingsError = ValidateSettings(credentials);
            if (!string.IsNullOrEmpty(settingsError))
                return BkavInvoiceOperationResult.Fail(settingsError);

            var state = await LoadGroupStateAsync(internalInvoiceNo, customerId, cancellationToken);
            if (!string.IsNullOrEmpty(state.Error))
                return BkavInvoiceOperationResult.Fail(state.Error, state.Group);

            var partnerInvoiceId = GetPartnerInvoiceId(state);
            if (string.IsNullOrWhiteSpace(partnerInvoiceId))
                return BkavInvoiceOperationResult.Fail("BKAV PartnerInvoiceID is empty.", state.Group);

            var result = await ExecuteCommandAsync(commandType, partnerInvoiceId, credentials, cancellationToken);
            if (!result.Success)
                return BkavInvoiceOperationResult.Fail(result.Message, state.Group, result.ObjectText);

            var dataFile = DeserializeCommandObject<BkavInvoiceDataFileBase64>(result.ObjectText);
            var base64 = objectPropertyName.Equals("PDF", StringComparison.OrdinalIgnoreCase) ? dataFile?.PDF : dataFile?.XML;
            if (string.IsNullOrWhiteSpace(base64))
                return BkavInvoiceOperationResult.Fail($"BKAV did not return {extension.ToUpperInvariant()} data.", state.Group, result.ObjectText);

            var relativePath = await SaveOutputFileAsync(Convert.FromBase64String(base64), $"{SanitizeFileName(partnerInvoiceId)}_{DateTime.Now:yyyyMMddHHmmss}.{extension}", cancellationToken);
            foreach (var line in state.Lines)
            {
                if (extension.Equals("pdf", StringComparison.OrdinalIgnoreCase))
                    line.BkavPdfPath = relativePath;
                else
                    line.BkavXmlPath = relativePath;

                line.BkavLastMessage = $"{extension.ToUpperInvariant()} saved: {relativePath}";
                line.dateupdate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            }

            await context.SaveChangesAsync(cancellationToken);
            return BkavInvoiceOperationResult.Ok($"BKAV {extension.ToUpperInvariant()} downloaded.", await ReloadGroupAsync(internalInvoiceNo, customerId, cancellationToken), filePath: relativePath, objectText: result.ObjectText);
        }
        catch (Exception ex)
        {
            return BkavInvoiceOperationResult.Fail(ex.Message);
        }
    }

    private async Task<GroupState> LoadGroupStateAsync(string internalInvoiceNo, Guid customerId, CancellationToken cancellationToken)
    {
        var normalizedInternalNo = Clean(internalInvoiceNo);
        if (string.IsNullOrWhiteSpace(normalizedInternalNo))
            return GroupState.Fail("Internal invoice number is required.");

        var lines = await context.HoaDonDauRa
            .Where(x => x.continued == true && x.customerid == customerId && x.sohoadonNoibo != null && x.sohoadonNoibo.Trim() == normalizedInternalNo)
            .OrderBy(x => x.soThuTu)
            .ThenBy(x => x.hoadondauraid)
            .ToListAsync(cancellationToken);

        if (lines.Count == 0)
            return GroupState.Fail("HoaDonDauRa group not found.");

        var customer = await context.Customer.AsNoTracking().FirstOrDefaultAsync(x => x.Customer_ID == customerId, cancellationToken);
        if (customer == null)
            return GroupState.Fail("Customer is required before issuing BKAV invoice.", BuildGroup(normalizedInternalNo, customerId, lines, new Dictionary<Guid, M_Customer>()));

        var chargeIds = lines.Select(x => x.itemid).Distinct().ToHashSet();
        var charges = (await context.Charge
            .AsNoTracking()
            .ToListAsync(cancellationToken))
            .Where(x => chargeIds.Contains(x.CHARGE_ID))
            .ToDictionary(x => x.CHARGE_ID);

        return new GroupState(normalizedInternalNo, customerId, lines, customer, charges, BuildGroup(normalizedInternalNo, customerId, lines, new Dictionary<Guid, M_Customer> { [customer.Customer_ID] = customer }), string.Empty);
    }

    private async Task<BkavInvoiceGroup?> ReloadGroupAsync(string internalInvoiceNo, Guid customerId, CancellationToken cancellationToken)
    {
        var groups = await GetInvoiceGroupsAsync(cancellationToken);
        return groups.FirstOrDefault(x => x.CustomerId == customerId && x.InternalInvoiceNo.Equals(Clean(internalInvoiceNo), StringComparison.OrdinalIgnoreCase));
    }

    private BkavInvoiceDataWS BuildInvoiceData(GroupState state, int commandType, BkavInvoiceActionInput input)
    {
        var first = state.Lines.First();
        var currency = state.Lines.Select(x => Clean(x.tiente)).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).SingleOrDefault() ?? "VND";
        var exchangeRate = first.tigia.GetValueOrDefault(1);
        if (exchangeRate <= 0)
            exchangeRate = 1;

        var invoiceWs = new BkavInvoiceWS
        {
            InvoiceTypeID = 1,
            InvoiceDate = first.ngayphathanhhoadonDientu ?? DateTime.Now,
            BuyerName = Clean(state.Customer.ATTN),
            BuyerTaxCode = Clean(state.Customer.TaxCode),
            BuyerUnitName = FirstNonEmpty(state.Customer.EnglishName, state.Customer.COMPANY, state.Customer.BIZName, state.Customer.Customer_Code),
            BuyerAddress = FirstNonEmpty(state.Customer.addresstiengviet, state.Customer.Address),
            BuyerBankAccount = Clean(state.Customer.sotaikhoan),
            PayMethodID = input.PayMethodID <= 0 ? 3 : input.PayMethodID,
            ReceiveTypeID = 3,
            ReceiverEmail = Clean(state.Customer.Email),
            ReceiverMobile = Clean(state.Customer.Tel),
            ReceiverAddress = FirstNonEmpty(state.Customer.addresstiengviet, state.Customer.Address),
            ReceiverName = FirstNonEmpty(state.Customer.ATTN, state.Customer.COMPANY, state.Customer.EnglishName),
            Note = BuildGroupNote(state.Lines),
            UserDefine = "{}",
            BillCode = state.InternalInvoiceNo,
            CurrencyID = currency,
            ExchangeRate = exchangeRate,
            InvoiceStatusID = 1,
            SignedDate = DateTime.Now,
            InvoiceNo = 0,
            InvoiceForm = string.Empty,
            InvoiceSerial = string.Empty,
            InvoiceCode = string.Empty,
            OriginalInvoiceIdentify = string.Empty,
            TypeCreateInvoice = 0
        };

        if (commandType is BkavInvoiceCommandTypes.CreateInvoiceWithFormSerial or BkavInvoiceCommandTypes.CreateInvoiceWithFormSerialNo)
        {
            invoiceWs.InvoiceForm = Clean(input.InvoiceForm);
            invoiceWs.InvoiceSerial = Clean(input.InvoiceSerial);
        }

        if (commandType == BkavInvoiceCommandTypes.CreateInvoiceWithFormSerialNo)
            invoiceWs.InvoiceNo = input.InvoiceNo.GetValueOrDefault();

        if (commandType == BkavInvoiceCommandTypes.CreateInvoiceReplace)
        {
            invoiceWs.TypeCreateInvoice = 1;
            invoiceWs.OriginalInvoiceIdentify = Clean(input.OriginalInvoiceIdentify);
        }

        if (commandType == BkavInvoiceCommandTypes.CreateInvoiceAdjust)
        {
            invoiceWs.TypeCreateInvoice = 2;
            invoiceWs.OriginalInvoiceIdentify = Clean(input.OriginalInvoiceIdentify);
        }

        var partnerInvoiceId = GetPartnerInvoiceId(state);
        if (string.IsNullOrWhiteSpace(partnerInvoiceId))
            partnerInvoiceId = GenerateStablePartnerInvoiceId(state.InternalInvoiceNo, state.CustomerId).ToString(CultureInfo.InvariantCulture);

        return new BkavInvoiceDataWS
        {
            Invoice = invoiceWs,
            ListInvoiceDetailsWS = state.Lines.Select(x => BuildInvoiceDetail(x, state.Charges)).ToList(),
            ListInvoiceAttachFileWS = [],
            PartnerInvoiceID = long.Parse(partnerInvoiceId, CultureInfo.InvariantCulture),
            PartnerInvoiceStringID = string.Empty
        };
    }

    private static BkavInvoiceDetailsWS BuildInvoiceDetail(M_HoaDonDauRa line, IReadOnlyDictionary<Guid, ChargeModel> charges)
    {
        charges.TryGetValue(line.itemid, out var charge);
        var quantity = line.soluong.GetValueOrDefault(1);
        if (quantity <= 0)
            quantity = 1;

        var amount = line.thanhtien ?? quantity * line.dongia.GetValueOrDefault();
        var price = line.dongia ?? (quantity == 0 ? amount : amount / quantity);
        var taxAmount = line.thanhtiensauthue.HasValue && line.thanhtien.HasValue
            ? Math.Max(0, line.thanhtiensauthue.Value - line.thanhtien.Value)
            : Math.Round(amount * line.thue.GetValueOrDefault() / 100, 0);

        return new BkavInvoiceDetailsWS
        {
            ItemTypeID = 0,
            ItemName = FirstNonEmpty(
                charge == null ? null : $"{charge.CHARGE_CODE} - {charge.CHARGE}",
                line.ghiChu,
                "Dich vu"),
            UnitName = FirstNonEmpty(charge?.DVT, charge?.unit),
            Qty = quantity,
            Price = price,
            Amount = amount,
            TaxRateID = MapTaxRateId(line.thue),
            TaxAmount = taxAmount,
            IsDiscount = false,
            UserDefineDetails = "{}"
        };
    }

    private BkavInvoiceDataWS BuildPartnerLookup(GroupState state)
    {
        var partnerInvoiceId = GetPartnerInvoiceId(state);
        if (string.IsNullOrWhiteSpace(partnerInvoiceId))
            partnerInvoiceId = GenerateStablePartnerInvoiceId(state.InternalInvoiceNo, state.CustomerId).ToString(CultureInfo.InvariantCulture);

        return new BkavInvoiceDataWS
        {
            PartnerInvoiceID = long.Parse(partnerInvoiceId, CultureInfo.InvariantCulture),
            PartnerInvoiceStringID = string.Empty
        };
    }

    private async Task ApplyInvoiceResultAsync(List<M_HoaDonDauRa> lines, BkavInvoiceResult result, long fallbackPartnerInvoiceId, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var partnerInvoiceId = result.PartnerInvoiceID > 0 ? result.PartnerInvoiceID : fallbackPartnerInvoiceId;
        var invoiceGuid = result.InvoiceGUID == Guid.Empty ? GetInvoiceGuid(lines) : result.InvoiceGUID.ToString();
        var invoiceNo = result.InvoiceNo > 0 ? result.InvoiceNo : (int?)null;
        var electronicNo = invoiceNo.HasValue ? invoiceNo.Value.ToString(CultureInfo.InvariantCulture) : lines.FirstOrDefault()?.sohoadonDientu;

        foreach (var line in lines)
        {
            line.sohoadonDientu = FirstNonEmpty(electronicNo, line.sohoadonDientu);
            line.ngayphathanhhoadonDientu ??= now;
            line.BkavPartnerInvoiceID = partnerInvoiceId;
            line.BkavPartnerInvoiceStringID = result.PartnerInvoiceStringID;
            line.BkavInvoiceGUID = invoiceGuid;
            line.BkavInvoiceNo = invoiceNo ?? line.BkavInvoiceNo;
            line.BkavInvoiceForm = FirstNonEmpty(result.InvoiceForm, line.BkavInvoiceForm);
            line.BkavInvoiceSerial = FirstNonEmpty(result.InvoiceSerial, line.BkavInvoiceSerial);
            line.BkavLastMessage = FirstNonEmpty(result.MessLog, "BKAV invoice command success.");
            line.dateupdate = now.ToString(CultureInfo.InvariantCulture);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SetLastMessageAsync(List<M_HoaDonDauRa> lines, string? message, CancellationToken cancellationToken)
    {
        foreach (var line in lines)
        {
            line.BkavLastMessage = message;
            line.dateupdate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private string ValidateSettings(BkavInvoiceRuntimeCredentials? credentials = null)
    {
        if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
            return "Missing BkavInvoice:ServiceUrl.";

        var effectiveCredentials = ResolveCredentials(credentials);
        if (string.IsNullOrWhiteSpace(effectiveCredentials.PartnerGuid))
            return "Missing BkavInvoice:PartnerGuid.";
        if (string.IsNullOrWhiteSpace(effectiveCredentials.PartnerToken))
            return "Missing BkavInvoice:PartnerToken.";
        if (!Uri.TryCreate(settings.ServiceUrl, UriKind.Absolute, out _))
            return "Invalid BkavInvoice:ServiceUrl.";

        return string.Empty;
    }

    private EffectiveBkavCredentials ResolveCredentials(BkavInvoiceRuntimeCredentials? credentials)
    {
        var hasRuntimeCredential = credentials != null &&
            (!string.IsNullOrWhiteSpace(credentials.PartnerGuid) ||
             !string.IsNullOrWhiteSpace(credentials.PartnerToken));

        return hasRuntimeCredential
            ? new EffectiveBkavCredentials(Clean(credentials!.PartnerGuid), Clean(credentials.PartnerToken))
            : new EffectiveBkavCredentials(Clean(settings.PartnerGuid), Clean(settings.PartnerToken));
    }

    private static string ValidateInvoiceNumberCommand(int commandType, BkavInvoiceActionInput input)
    {
        if (commandType is BkavInvoiceCommandTypes.CreateInvoiceWithFormSerial or BkavInvoiceCommandTypes.CreateInvoiceWithFormSerialNo)
        {
            if (string.IsNullOrWhiteSpace(input.InvoiceForm))
                return "InvoiceForm is required for BKAV command 110/111.";
            if (string.IsNullOrWhiteSpace(input.InvoiceSerial))
                return "InvoiceSerial is required for BKAV command 110/111.";
        }

        if (commandType == BkavInvoiceCommandTypes.CreateInvoiceWithFormSerialNo && input.InvoiceNo.GetValueOrDefault() <= 0)
            return "InvoiceNo must be a positive number for BKAV command 111.";

        return string.Empty;
    }

    private static string ValidateOriginalIdentifyCommand(int commandType, BkavInvoiceActionInput input)
    {
        if ((commandType == BkavInvoiceCommandTypes.CreateInvoiceReplace ||
             commandType == BkavInvoiceCommandTypes.CreateInvoiceAdjust) &&
            string.IsNullOrWhiteSpace(input.OriginalInvoiceIdentify))
            return "OriginalInvoiceIdentify is required for BKAV replace/adjust commands.";

        return ValidateInvoiceNumberCommand(commandType, input);
    }

    private async Task<BkavCommandResult> ExecuteCommandAsync(int commandType, string commandObject, BkavInvoiceRuntimeCredentials? credentials, CancellationToken cancellationToken)
    {
        var effectiveCredentials = ResolveCredentials(credentials);
        var commandData = new BkavCommandData
        {
            CommandType = commandType,
            CommandObject = commandObject,
            CmdType = IsXmlMode(settings.Mode) ? 1 : 0
        };

        var serializedCommand = SerializeCommandData(commandData);
        var encodedCommand = EncodeTransportData(serializedCommand, effectiveCredentials);
        var responseData = await CallExecuteCommandSoapAsync(encodedCommand, effectiveCredentials, cancellationToken);
        var decodedResponse = DecodeTransportData(responseData, effectiveCredentials);

        return ParseCommandResult(decodedResponse);
    }

    private string SerializeCommandData(BkavCommandData commandData)
        => IsXmlMode(settings.Mode) ? SerializeXml(commandData, "CommandData") : JsonSerializer.Serialize(commandData, JsonOptions);

    private string SerializeInvoiceDataList(List<BkavInvoiceDataWS> list)
        => IsXmlMode(settings.Mode)
            ? SerializeXml(list, "ArrayOfInvoiceDataWS")
            : JsonSerializer.Serialize(list, JsonOptions);

    private string SerializeCommandObject<T>(T value)
        => IsXmlMode(settings.Mode) ? SerializeXml(value, typeof(T).Name) : JsonSerializer.Serialize(value, JsonOptions);

    private T? DeserializeCommandObject<T>(string objectText)
    {
        if (string.IsNullOrWhiteSpace(objectText))
            return default;

        return IsXmlMode(settings.Mode)
            ? DeserializeXml<T>(objectText)
            : JsonSerializer.Deserialize<T>(objectText, JsonOptions);
    }

    private string EncodeTransportData(string plainText, EffectiveBkavCredentials credentials)
    {
        var bytes = Encoding.UTF8.GetBytes(plainText);
        if (HasMode(ZipMode))
            bytes = Gzip(bytes);

        if (HasMode(EncryptModeV1))
            bytes = EncryptAes(bytes, credentials);
        else if (HasMode(EncryptMode))
            bytes = EncryptTripleDes(bytes, credentials);

        return Convert.ToBase64String(bytes);
    }

    private string DecodeTransportData(string responseData, EffectiveBkavCredentials credentials)
    {
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(responseData);
        }
        catch (FormatException)
        {
            return responseData;
        }

        if (HasMode(EncryptModeV1))
            bytes = DecryptAes(bytes, credentials);
        else if (HasMode(EncryptMode))
            bytes = DecryptTripleDes(bytes, credentials);

        if (HasMode(ZipMode))
            bytes = Gunzip(bytes);

        return Encoding.UTF8.GetString(bytes);
    }

    private async Task<string> CallExecuteCommandSoapAsync(string encryptedCommandData, EffectiveBkavCredentials credentials, CancellationToken cancellationToken)
    {
        var body =
            $"""
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <ExecuteCommand xmlns="http://tempuri.org/">
      <PartnerGUID>{XmlEscape(credentials.PartnerGuid)}</PartnerGUID>
      <EncryptedCommandData>{XmlEscape(encryptedCommandData)}</EncryptedCommandData>
    </ExecuteCommand>
  </soap:Body>
</soap:Envelope>
""";

        using var request = new HttpRequestMessage(HttpMethod.Post, settings.ServiceUrl)
        {
            Content = new StringContent(body, Encoding.UTF8, "text/xml")
        };
        request.Headers.Add("SOAPAction", "\"http://tempuri.org/ExecuteCommand\"");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"BKAV SOAP error {(int)response.StatusCode}: {responseText}");

        var document = XDocument.Parse(responseText);
        var result = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "ExecuteCommandResult")?.Value;
        if (string.IsNullOrWhiteSpace(result))
            throw new InvalidOperationException("BKAV SOAP response does not contain ExecuteCommandResult.");

        return result;
    }

    private BkavCommandResult ParseCommandResult(string resultText)
    {
        if (IsXmlMode(settings.Mode))
        {
            var xmlResult = DeserializeXml<BkavResultEnvelopeXml>(resultText);
            if (xmlResult == null)
                return BkavCommandResult.Fail("Cannot parse BKAV XML result.");

            return xmlResult.Status == 0
                ? BkavCommandResult.Ok(xmlResult.Object ?? string.Empty, xmlResult.Message ?? string.Empty)
                : BkavCommandResult.Fail(FirstNonEmpty(xmlResult.Message, xmlResult.Object, $"BKAV returned status {xmlResult.Status}"));
        }

        using var document = JsonDocument.Parse(resultText);
        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.String)
            return BkavCommandResult.Ok(root.GetString() ?? string.Empty);

        var status = TryGetInt(root, "Status") ?? 0;
        var message = FirstNonEmpty(TryGetString(root, "Message"), TryGetString(root, "MessLog"), TryGetString(root, "Error"), TryGetString(root, "ErrorMessage"));
        var objectText = root.TryGetProperty("Object", out var objectElement) ? GetJsonElementText(objectElement) : root.GetRawText();

        return status == 0
            ? BkavCommandResult.Ok(objectText, message)
            : BkavCommandResult.Fail(FirstNonEmpty(message, objectText, $"BKAV returned status {status}"), objectText);
    }

    private BkavInvoiceResult? ParseFirstInvoiceResult(string objectText)
        => DeserializeCommandObject<List<BkavInvoiceResult>>(objectText)?.FirstOrDefault();

    private async Task<string> SaveOutputFileAsync(byte[] bytes, string fileName, CancellationToken cancellationToken)
    {
        var outputFolder = string.IsNullOrWhiteSpace(settings.OutputFolder) ? "exports/bkav" : settings.OutputFolder.Trim('/', '\\');
        var webRoot = webHostEnvironment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");

        var relativeFile = fileName.Replace('\\', '/').TrimStart('/');
        var relativeFolder = Path.GetDirectoryName(relativeFile.Replace('/', Path.DirectorySeparatorChar));
        var fullFolder = string.IsNullOrWhiteSpace(relativeFolder)
            ? Path.Combine(new[] { webRoot }.Concat(outputFolder.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)).ToArray())
            : Path.Combine(new[] { webRoot }.Concat(outputFolder.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)).Concat(relativeFolder.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)).ToArray());

        Directory.CreateDirectory(fullFolder);
        var fullPath = Path.Combine(fullFolder, Path.GetFileName(relativeFile));
        await File.WriteAllBytesAsync(fullPath, bytes, cancellationToken);

        return $"{outputFolder.Replace('\\', '/')}/{relativeFile}";
    }

    private byte[] EncryptTripleDes(byte[] bytes, EffectiveBkavCredentials credentials)
    {
        using var md5 = MD5.Create();
        using var tripleDes = TripleDES.Create();
        tripleDes.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(credentials.PartnerToken));
        tripleDes.Mode = CipherMode.ECB;
        tripleDes.Padding = PaddingMode.PKCS7;

        using var encryptor = tripleDes.CreateEncryptor();
        return encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
    }

    private byte[] DecryptTripleDes(byte[] bytes, EffectiveBkavCredentials credentials)
    {
        using var md5 = MD5.Create();
        using var tripleDes = TripleDES.Create();
        tripleDes.Key = md5.ComputeHash(Encoding.UTF8.GetBytes(credentials.PartnerToken));
        tripleDes.Mode = CipherMode.ECB;
        tripleDes.Padding = PaddingMode.PKCS7;

        using var decryptor = tripleDes.CreateDecryptor();
        return decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
    }

    private byte[] EncryptAes(byte[] bytes, EffectiveBkavCredentials credentials)
    {
        var (key, iv) = GetAesKeyAndIv(credentials);
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
    }

    private byte[] DecryptAes(byte[] bytes, EffectiveBkavCredentials credentials)
    {
        var (key, iv) = GetAesKeyAndIv(credentials);
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
    }

    private (byte[] Key, byte[] Iv) GetAesKeyAndIv(EffectiveBkavCredentials credentials)
    {
        var parts = credentials.PartnerToken.Split(':', 2);
        if (parts.Length != 2)
            throw new InvalidOperationException("AES BKAV mode requires PartnerToken in 'base64Key:base64IV' format.");

        return (Convert.FromBase64String(parts[0]), Convert.FromBase64String(parts[1]));
    }

    private static BkavInvoiceGroup BuildGroup(string internalInvoiceNo, Guid customerId, List<M_HoaDonDauRa> lines, IReadOnlyDictionary<Guid, M_Customer> customers)
    {
        customers.TryGetValue(customerId, out var customer);
        var first = lines.FirstOrDefault();
        var totalBeforeTax = lines.Sum(x => x.thanhtien.GetValueOrDefault());
        var totalAfterTax = lines.Sum(x => x.thanhtiensauthue.GetValueOrDefault());

        return new BkavInvoiceGroup
        {
            InternalInvoiceNo = internalInvoiceNo,
            CustomerId = customerId,
            CustomerName = customer == null ? string.Empty : FirstNonEmpty(customer.Customer_Code, customer.COMPANY, customer.EnglishName),
            LineCount = lines.Count,
            InvoiceDate = lines.Select(x => x.ngayphathanhhoadonDientu).FirstOrDefault(x => x.HasValue),
            ElectronicInvoiceNo = first?.sohoadonDientu,
            Currency = lines.Select(x => x.tiente).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
            TotalBeforeTax = totalBeforeTax,
            TotalTax = Math.Max(0, totalAfterTax - totalBeforeTax),
            TotalAfterTax = totalAfterTax,
            BkavPartnerInvoiceID = first?.BkavPartnerInvoiceID,
            BkavPartnerInvoiceStringID = first?.BkavPartnerInvoiceStringID,
            BkavInvoiceGUID = first?.BkavInvoiceGUID,
            BkavInvoiceNo = first?.BkavInvoiceNo,
            BkavInvoiceForm = first?.BkavInvoiceForm,
            BkavInvoiceSerial = first?.BkavInvoiceSerial,
            BkavInvoiceLink = first?.BkavInvoiceLink,
            BkavPdfPath = first?.BkavPdfPath,
            BkavXmlPath = first?.BkavXmlPath,
            BkavStatusID = first?.BkavStatusID,
            BkavLastMessage = first?.BkavLastMessage,
            Lines = lines
        };
    }

    private static string BuildGroupNote(IEnumerable<M_HoaDonDauRa> lines)
    {
        var notes = lines.Select(x => Clean(x.ghiChu)).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Take(3).ToList();
        return notes.Count == 0 ? string.Empty : string.Join("; ", notes);
    }

    private static string GetPartnerInvoiceId(GroupState state)
    {
        var partnerId = state.Lines.Select(x => x.BkavPartnerInvoiceID).FirstOrDefault(x => x.GetValueOrDefault() > 0);
        return partnerId.GetValueOrDefault() > 0 ? partnerId!.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
    }

    private static string GetInvoiceGuid(IEnumerable<M_HoaDonDauRa> lines)
        => lines.Select(x => Clean(x.BkavInvoiceGUID)).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;

    private static byte[] Gzip(byte[] bytes)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }

        return output.ToArray();
    }

    private static byte[] Gunzip(byte[] bytes)
    {
        using var input = new MemoryStream(bytes);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return output.ToArray();
    }

    private static string SerializeXml<T>(T value, string rootName)
    {
        var serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(rootName));
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        serializer.Serialize(writer, value);
        return writer.ToString();
    }

    private static T? DeserializeXml<T>(string xml)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(xml);
        return (T?)serializer.Deserialize(reader);
    }

    private bool HasMode(long mode) => (settings.Mode & mode) == mode;

    private static bool IsXmlMode(long mode) => (mode & XmlMode) == XmlMode;

    private static int? TryGetInt(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property))
            return null;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var intValue))
            return intValue;

        if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out intValue))
            return intValue;

        return null;
    }

    private static string TryGetString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property))
            return string.Empty;

        return GetJsonElementText(property);
    }

    private static string GetJsonElementText(JsonElement element)
        => element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Null => string.Empty,
            JsonValueKind.Undefined => string.Empty,
            _ => element.GetRawText()
        };

    private static string FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim() ?? string.Empty;

    private static string Clean(string? value) => value?.Trim() ?? string.Empty;

    private static string SanitizeFileName(string value)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            value = value.Replace(invalidChar, '_');

        return value.Replace('/', '_').Replace('\\', '_');
    }

    private static string XmlEscape(string value) => System.Security.SecurityElement.Escape(value) ?? string.Empty;

    private sealed record GroupState(
        string InternalInvoiceNo,
        Guid CustomerId,
        List<M_HoaDonDauRa> Lines,
        M_Customer Customer,
        Dictionary<Guid, ChargeModel> Charges,
        BkavInvoiceGroup? Group,
        string Error)
    {
        public static GroupState Fail(string error, BkavInvoiceGroup? group = null)
            => new(string.Empty, Guid.Empty, [], new M_Customer(), [], group, error);
    }

    private sealed class BkavCommandResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public string ObjectText { get; init; } = string.Empty;

        public static BkavCommandResult Ok(string objectText, string message = "")
            => new() { Success = true, ObjectText = objectText, Message = message };

        public static BkavCommandResult Fail(string message, string objectText = "")
            => new() { Success = false, Message = message, ObjectText = objectText };
    }

    [XmlRoot("CommandData")]
    public class BkavCommandData
    {
        public int CommandType { get; set; }
        public string CommandObject { get; set; } = string.Empty;
        public int CmdType { get; set; }
    }

    [XmlRoot("Result")]
    public class BkavResultEnvelopeXml
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public string? Object { get; set; }
    }

    [XmlType("InvoiceResult")]
    public class BkavInvoiceResult
    {
        public long PartnerInvoiceID { get; set; }
        public string? PartnerInvoiceStringID { get; set; }
        public Guid InvoiceGUID { get; set; }
        public string? InvoiceForm { get; set; }
        public string? InvoiceSerial { get; set; }
        public int InvoiceNo { get; set; }
        public int Status { get; set; }
        public string? MessLog { get; set; }
    }

    [XmlType("InvoiceDataFileBase64")]
    public class BkavInvoiceDataFileBase64
    {
        public string? PDF { get; set; }
        public string? XML { get; set; }
    }

    [XmlType("InvoiceDataWS")]
    public class BkavInvoiceDataWS
    {
        public BkavInvoiceWS Invoice { get; set; } = new();
        public List<BkavInvoiceDetailsWS> ListInvoiceDetailsWS { get; set; } = [];
        public List<BkavInvoiceAttachFileWS> ListInvoiceAttachFileWS { get; set; } = [];
        public long PartnerInvoiceID { get; set; }
        public string PartnerInvoiceStringID { get; set; } = string.Empty;
    }

    [XmlType("InvoiceWS")]
    public class BkavInvoiceWS
    {
        public int InvoiceTypeID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerTaxCode { get; set; } = string.Empty;
        public string BuyerUnitName { get; set; } = string.Empty;
        public string BuyerAddress { get; set; } = string.Empty;
        public string BuyerBankAccount { get; set; } = string.Empty;
        public int PayMethodID { get; set; }
        public int ReceiveTypeID { get; set; }
        public string ReceiverEmail { get; set; } = string.Empty;
        public string ReceiverMobile { get; set; } = string.Empty;
        public string ReceiverAddress { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public string UserDefine { get; set; } = string.Empty;
        public string BillCode { get; set; } = string.Empty;
        public string CurrencyID { get; set; } = "VND";
        public double ExchangeRate { get; set; } = 1;
        public Guid InvoiceGUID { get; set; }
        public int InvoiceStatusID { get; set; }
        public string InvoiceForm { get; set; } = string.Empty;
        public string InvoiceSerial { get; set; } = string.Empty;
        public int InvoiceNo { get; set; }
        public string InvoiceCode { get; set; } = string.Empty;
        public DateTime SignedDate { get; set; }
        public int TypeCreateInvoice { get; set; }
        public string OriginalInvoiceIdentify { get; set; } = string.Empty;
    }

    [XmlType("InvoiceDetailsWS")]
    public class BkavInvoiceDetailsWS
    {
        public int ItemTypeID { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public double Qty { get; set; }
        public double Price { get; set; }
        public double Amount { get; set; }
        public int TaxRateID { get; set; }
        public double TaxAmount { get; set; }
        public bool IsDiscount { get; set; }
        public string UserDefineDetails { get; set; } = string.Empty;
        public bool? IsIncrease { get; set; }
    }

    [XmlType("InvoiceAttachFileWS")]
    public class BkavInvoiceAttachFileWS
    {
        public string FileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public string FileContent { get; set; } = string.Empty;
    }

    [XmlType("CreateAccountInfoFromPartner")]
    public class BkavCreateAccountInfoFromPartner
    {
        public string UnitName { get; set; } = string.Empty;
        public string UnitAddress { get; set; } = string.Empty;
        public string UnitPersonRepresent { get; set; } = string.Empty;
        public string UnitPersonRepresentPosition { get; set; } = string.Empty;
        public string UnitEmail { get; set; } = string.Empty;
        public string UnitPhone { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string BankAccount { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public int TaxDepartmentID { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string DomainCheckInvoice { get; set; } = string.Empty;
    }

    [XmlType("AccountResult")]
    public class BkavAccountResult
    {
        public Guid AccountGUID { get; set; }
        public string? Account { get; set; }
        public string? Password { get; set; }
        public int NumberInvoice { get; set; }
        public int NumberMSG { get; set; }
        public Guid PartnerGUID { get; set; }
        public string? PartnerToken { get; set; }
    }

    [XmlType("DllInfo")]
    public class BkavDllInfo
    {
        public int RunType { get; set; }
        public string? ClassName { get; set; }
        public string? DLLName { get; set; }
        public string? Code { get; set; }
        public byte[]? DLLContent { get; set; }
    }
}
