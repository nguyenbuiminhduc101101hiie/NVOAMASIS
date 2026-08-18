using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using Stimulsoft.Report;

namespace NVOAMASIS.Services
{
    public static class BillSeaReportTemplateNames
    {
        public const string Main = "BillSea_NVOCC.mrt";
        public const string Attach = "BillSea_NVOCC_Att_1.mrt";
        public const string Air = "BillAir.mrt";
        public const string AnSea = "BillArrivalNoticeNVOCC.mrt";
        public const string AnAir = "BillArrivalNotice_Air.mrt";
        public const string Do = "BillDeliveryOrderNVOCC.mrt";
        public const string LenhDieuXe = "BillLenhDieuXe.mrt";
        public const string Trang2 = "BillTrang2.mrt";
        public const string Booking = "BookingRequestNVOCC.mrt";
        public const string Quotation = "Quotation.mrt";
        public const string Bbgn = "BienBanGiaoNhan.mrt";
    }

    public static class BillLayoutFormKindHelper
    {
        public static string GetDefaultTemplateFile(BillLayoutFormKind kind) => kind switch
        {
            BillLayoutFormKind.Air => BillSeaReportTemplateNames.Air,
            BillLayoutFormKind.AnSea => BillSeaReportTemplateNames.AnSea,
            BillLayoutFormKind.AnAir => BillSeaReportTemplateNames.AnAir,
            BillLayoutFormKind.Do => BillSeaReportTemplateNames.Do,
            BillLayoutFormKind.LenhDieuXe => BillSeaReportTemplateNames.LenhDieuXe,
            BillLayoutFormKind.Trang2 => BillSeaReportTemplateNames.Trang2,
            BillLayoutFormKind.Booking => BillSeaReportTemplateNames.Booking,
            BillLayoutFormKind.Quotation => BillSeaReportTemplateNames.Quotation,
            BillLayoutFormKind.Bbgn => BillSeaReportTemplateNames.Bbgn,
            _ => BillSeaReportTemplateNames.Main
        };

        public static BillLayoutFormKind ParseFormKind(string? value)
        {
            if (string.Equals(value, nameof(BillLayoutFormKind.Air), StringComparison.OrdinalIgnoreCase))
                return BillLayoutFormKind.Air;
            if (string.Equals(value, nameof(BillLayoutFormKind.AnSea), StringComparison.OrdinalIgnoreCase))
                return BillLayoutFormKind.AnSea;
            if (string.Equals(value, nameof(BillLayoutFormKind.AnAir), StringComparison.OrdinalIgnoreCase))
                return BillLayoutFormKind.AnAir;
            if (string.Equals(value, nameof(BillLayoutFormKind.Do), StringComparison.OrdinalIgnoreCase))
                return BillLayoutFormKind.Do;
            if (string.Equals(value, nameof(BillLayoutFormKind.LenhDieuXe), StringComparison.OrdinalIgnoreCase))
                return BillLayoutFormKind.LenhDieuXe;
            if (string.Equals(value, nameof(BillLayoutFormKind.Trang2), StringComparison.OrdinalIgnoreCase))
                return BillLayoutFormKind.Trang2;
            if (string.Equals(value, nameof(BillLayoutFormKind.Booking), StringComparison.OrdinalIgnoreCase))
                return BillLayoutFormKind.Booking;
            if (string.Equals(value, nameof(BillLayoutFormKind.Quotation), StringComparison.OrdinalIgnoreCase))
                return BillLayoutFormKind.Quotation;
            if (string.Equals(value, nameof(BillLayoutFormKind.Bbgn), StringComparison.OrdinalIgnoreCase))
                return BillLayoutFormKind.Bbgn;
            return BillLayoutFormKind.Sea;
        }

        public static string ToStorageValue(BillLayoutFormKind kind) => kind switch
        {
            BillLayoutFormKind.Air => nameof(BillLayoutFormKind.Air),
            BillLayoutFormKind.AnSea => nameof(BillLayoutFormKind.AnSea),
            BillLayoutFormKind.AnAir => nameof(BillLayoutFormKind.AnAir),
            BillLayoutFormKind.Do => nameof(BillLayoutFormKind.Do),
            BillLayoutFormKind.LenhDieuXe => nameof(BillLayoutFormKind.LenhDieuXe),
            BillLayoutFormKind.Trang2 => nameof(BillLayoutFormKind.Trang2),
            BillLayoutFormKind.Booking => nameof(BillLayoutFormKind.Booking),
            BillLayoutFormKind.Quotation => nameof(BillLayoutFormKind.Quotation),
            BillLayoutFormKind.Bbgn => nameof(BillLayoutFormKind.Bbgn),
            _ => nameof(BillLayoutFormKind.Sea)
        };

        public static string GetDisplayName(BillLayoutFormKind kind) => kind switch
        {
            BillLayoutFormKind.Air => "Air",
            BillLayoutFormKind.AnSea => "AN Sea",
            BillLayoutFormKind.AnAir => "AN Air",
            BillLayoutFormKind.Do => "DO",
            BillLayoutFormKind.LenhDieuXe => "Lệnh Điều Xe",
            BillLayoutFormKind.Trang2 => "Trang 2",
            BillLayoutFormKind.Booking => "Booking",
            BillLayoutFormKind.Quotation => "Quotation",
            BillLayoutFormKind.Bbgn => "BBGN",
            _ => "Sea"
        };

        public static string GetLongDisplayName(BillLayoutFormKind kind) => kind switch
        {
            BillLayoutFormKind.Air => "Bill Air",
            BillLayoutFormKind.AnSea => "AN Sea",
            BillLayoutFormKind.AnAir => "AN Air",
            BillLayoutFormKind.Do => "DO",
            BillLayoutFormKind.LenhDieuXe => "Lệnh Điều Xe",
            BillLayoutFormKind.Trang2 => "Trang 2",
            BillLayoutFormKind.Booking => "Booking",
            BillLayoutFormKind.Quotation => "Quotation",
            BillLayoutFormKind.Bbgn => "BBGN",
            _ => "Bill Sea"
        };

        public static bool SupportsAttach(BillLayoutFormKind kind) => kind == BillLayoutFormKind.Sea;

        public static bool SupportsFormBillAir(BillLayoutFormKind kind) => kind == BillLayoutFormKind.Air;

        /// <summary>Bill Sea / Air cho phép upload ảnh nền form (lưu FormBillAir).</summary>
        public static bool SupportsFormBackground(BillLayoutFormKind kind) =>
            kind is BillLayoutFormKind.Sea or BillLayoutFormKind.Air;

        /// <summary>Trang 2 dùng FormBillAir để lưu ảnh full-page (PageImage).</summary>
        public static bool SupportsTrang2Content(BillLayoutFormKind kind) => kind == BillLayoutFormKind.Trang2;

        public static bool UsesImage1AsLogo(BillLayoutFormKind kind) =>
            kind is BillLayoutFormKind.AnAir or BillLayoutFormKind.Quotation or BillLayoutFormKind.Booking or
            BillLayoutFormKind.Bbgn or BillLayoutFormKind.LenhDieuXe;
    }

    public sealed class BillSeaLayoutFormSummary
    {
        public Guid BillSeaLayoutFormId { get; init; }
        public string FormName { get; init; } = string.Empty;
        public BillLayoutFormKind FormKind { get; init; } = BillLayoutFormKind.Sea;
        public DateTime UpdatedAt { get; init; }
        public bool IsCustomized { get; init; }
        public bool HasAttachForm { get; init; }
    }

    public class BillSeaLayoutFormService(IDbContextFactory<AppDbContext> dbFactory, IWebHostEnvironment env)
    {
        private async Task<T> WithDbAsync<T>(
            Func<AppDbContext, CancellationToken, Task<T>> action,
            CancellationToken cancellationToken = default)
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken);
            return await action(context, cancellationToken);
        }

        private async Task WithDbAsync(
            Func<AppDbContext, CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            await using var context = await dbFactory.CreateDbContextAsync(cancellationToken);
            await action(context, cancellationToken);
        }

        public string GetDefaultTemplatePath(string reportFileName) =>
            Path.Combine(env.WebRootPath, "Reports", reportFileName);

        public async Task<byte[]> GetDefaultTemplateBytesAsync(string reportFileName = BillSeaReportTemplateNames.Main, CancellationToken cancellationToken = default)
        {
            var defaultPath = GetDefaultTemplatePath(reportFileName);
            if (!File.Exists(defaultPath))
                throw new FileNotFoundException($"Không tìm thấy template mặc định: {reportFileName}", defaultPath);

            var bytes = await File.ReadAllBytesAsync(defaultPath, cancellationToken);
            // Giữ nguyên bytes gốc (JSON hoặc XML). StiReport.Load đọc được cả hai.
            // Chỉ convert JSON→XML khi editor cần XDocument (xem LoadFormDocumentAsync).
            return bytes;
        }

        /// <summary>
        /// Bảo đảm nội dung MRT ở định dạng XML. Nếu là JSON (Stimulsoft 2023) thì load bằng
        /// StiReport rồi lưu lại dạng XML để tương thích với trình chỉnh layout (XDocument).
        /// </summary>
        public static byte[] EnsureXmlMrt(byte[] mrtBytes)
        {
            if (mrtBytes is not { Length: > 0 } || LooksLikeXml(mrtBytes))
                return mrtBytes;

            StimulsoftLicenseHelper.EnsureApplied();
            var report = new StiReport();
            using (var input = new MemoryStream(mrtBytes))
                report.Load(input); // tự nhận diện JSON

            // Stimulsoft 2023 mặc định serialize JSON; ép về XML để editor (XDocument) đọc được.
            // IsJsonReport chỉ có getter công khai nên phải set qua reflection (setter non-public).
            ForceXmlReportFormat(report);
            StimulsoftLicenseHelper.ClearTrialWatermark(report);

            using var output = new MemoryStream();
            report.Save(output); // giờ ghi ở định dạng XML (StiSerializer)
            return output.ToArray();
        }

        private static void ForceXmlReportFormat(StiReport report)
        {
            var type = report.GetType();
            // Ưu tiên setter non-public của property IsJsonReport.
            var prop = type.GetProperty("IsJsonReport",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var setter = prop?.GetSetMethod(nonPublic: true);
            if (setter != null)
            {
                setter.Invoke(report, new object[] { false });
                return;
            }

            // Dự phòng: set trực tiếp backing field nếu có.
            var field = type.GetField("<IsJsonReport>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(report, false);
        }

        private static bool LooksLikeXml(byte[] bytes)
        {
            var start = 0;
            // Bỏ qua UTF-8 BOM
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                start = 3;

            for (var i = start; i < bytes.Length; i++)
            {
                var c = (char)bytes[i];
                if (c is ' ' or '\t' or '\r' or '\n' or '\uFEFF')
                    continue;

                return c == '<';
            }

            return false;
        }

        public async Task<List<BillSeaLayoutFormSummary>> ListFormsAsync(BillLayoutFormKind? formKind = null, CancellationToken cancellationToken = default)
        {
            var forms = await WithDbAsync(async (context, ct) =>
            {
                var query = context.BillSeaLayoutForms
                    .AsNoTracking()
                    .Where(x => x.IsActive);

                if (formKind is BillLayoutFormKind kind)
                {
                    var kindValue = BillLayoutFormKindHelper.ToStorageValue(kind);
                    query = query.Where(x => x.FormKind == kindValue);
                }

                return await query
                    .OrderBy(x => x.FormName)
                    .ToListAsync(ct);
            }, cancellationToken);

            var defaultCache = new Dictionary<BillLayoutFormKind, byte[]>();
            async Task<byte[]> GetDefaultCachedAsync(BillLayoutFormKind kind)
            {
                if (defaultCache.TryGetValue(kind, out var cached))
                    return cached;

                var bytes = await GetDefaultTemplateBytesAsync(
                    BillLayoutFormKindHelper.GetDefaultTemplateFile(kind),
                    cancellationToken);
                defaultCache[kind] = bytes;
                return bytes;
            }

            var result = new List<BillSeaLayoutFormSummary>(forms.Count);
            foreach (var x in forms)
            {
                var kind = BillLayoutFormKindHelper.ParseFormKind(x.FormKind);
                var defaultBytes = await GetDefaultCachedAsync(kind);
                result.Add(new BillSeaLayoutFormSummary
                {
                    BillSeaLayoutFormId = x.BillSeaLayoutFormId,
                    FormName = x.FormName,
                    FormKind = kind,
                    UpdatedAt = x.UpdatedAt,
                    IsCustomized = !ContentEquals(x.MrtContent, defaultBytes),
                    HasAttachForm = x.AttachMrtContent is { Length: > 0 }
                });
            }

            return result;
        }

        public Task<M_BillSeaLayoutForm?> GetFormAsync(Guid formId, CancellationToken cancellationToken = default) =>
            WithDbAsync((context, ct) =>
                context.BillSeaLayoutForms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct), cancellationToken);

        public async Task<byte[]> GetFormBytesAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            if (form is null)
                throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

            // Trả bytes gốc cho Stimulsoft export (JSON/XML đều Load được).
            // Editor parse XDocument qua LoadFormDocumentAsync → EnsureXmlMrt tại đó.
            return form.MrtContent ?? Array.Empty<byte>();
        }

        public async Task<XDocument> LoadFormDocumentAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var bytes = await GetFormBytesAsync(formId, cancellationToken);
            bytes = EnsureXmlMrt(bytes);
            using var stream = new MemoryStream(bytes);
            return XDocument.Load(stream);
        }

        public async Task<bool> IsFormCustomizedAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            if (form is null)
                return false;

            var defaultBytes = await GetDefaultTemplateBytesAsync(form.SourceTemplate, cancellationToken);
            return !ContentEquals(form.MrtContent, defaultBytes);
        }

        public Task<byte[]?> GetCompanyLogoAsync(CancellationToken cancellationToken = default) =>
            WithDbAsync((context, ct) =>
                context.CompanyInfomation
                    .AsNoTracking()
                    .Select(x => x.Logo)
                    .FirstOrDefaultAsync(ct), cancellationToken);

        public Task<byte[]?> GetCompanyFormBillSeaAsync(CancellationToken cancellationToken = default) =>
            WithDbAsync((context, ct) =>
                context.CompanyInfomation
                    .AsNoTracking()
                    .Select(x => x.FormBillSea)
                    .FirstOrDefaultAsync(ct), cancellationToken);

        public async Task<byte[]?> GetFormLogoAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            return form?.Logo is { Length: > 0 } ? form.Logo : null;
        }

        public async Task<byte[]?> GetEffectiveFormLogoAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var formLogo = await GetFormLogoAsync(formId, cancellationToken);
            return formLogo ?? await GetCompanyLogoAsync(cancellationToken);
        }

        public async Task<bool> HasCustomFormLogoAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            return form?.Logo is { Length: > 0 };
        }

        public Task SaveFormLogoAsync(Guid formId, byte[]? logo, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

                form.Logo = logo is { Length: > 0 } ? logo : null;
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public Task ClearFormLogoAsync(Guid formId, CancellationToken cancellationToken = default) =>
            SaveFormLogoAsync(formId, null, cancellationToken);

        public async Task<byte[]?> GetFormBillAirAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            return form?.FormBillAir is { Length: > 0 } ? form.FormBillAir : null;
        }

        public async Task<byte[]?> GetDefaultBillAirImage1Async(CancellationToken cancellationToken = default)
        {
            var templateBytes = await GetDefaultTemplateBytesAsync(BillSeaReportTemplateNames.Air, cancellationToken);
            return BillSeaLayoutMrtHelper.ExtractImageBytes(templateBytes, "Image1");
        }

        public async Task<bool> HasCustomFormBillAirAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            return form?.FormBillAir is { Length: > 0 };
        }

        public Task SaveFormBillAirAsync(Guid formId, byte[]? formBillAir, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill.");

                form.FormBillAir = formBillAir is { Length: > 0 } ? formBillAir : null;
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public Task ClearFormBillAirAsync(Guid formId, CancellationToken cancellationToken = default) =>
            SaveFormBillAirAsync(formId, null, cancellationToken);

        public async Task<(bool ImageMode, string PageText, bool HasImage)> GetTrang2ContentAsync(
            Guid formId,
            CancellationToken cancellationToken = default)
        {
            var doc = await LoadFormDocumentAsync(formId, cancellationToken);
            var imageMode = BillSeaLayoutMrtHelper.IsTrang2ImageMode(doc);
            var pageText = BillSeaLayoutMrtHelper.GetTrang2PageText(doc);
            var hasImage = await HasCustomFormBillAirAsync(formId, cancellationToken);
            if (!imageMode && hasImage && string.IsNullOrWhiteSpace(pageText))
                imageMode = true;
            return (imageMode, pageText, hasImage);
        }

        public async Task SaveTrang2ContentAsync(
            Guid formId,
            bool imageMode,
            string? pageText,
            bool clearImageWhenText = false,
            CancellationToken cancellationToken = default)
        {
            var doc = await LoadFormDocumentAsync(formId, cancellationToken);
            BillSeaLayoutMrtHelper.ApplyTrang2Content(doc, imageMode, pageText);
            await SaveFormDocumentAsync(formId, doc, cancellationToken);

            if (!imageMode && clearImageWhenText)
                await ClearFormBillAirAsync(formId, cancellationToken);
        }

        public async Task<M_BillSeaLayoutForm> CreateFormAsync(
            string formName,
            BillLayoutFormKind formKind = BillLayoutFormKind.Sea,
            string? createdBy = null,
            CancellationToken cancellationToken = default)
        {
            var trimmedName = formName.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
                throw new ArgumentException("Tên form không được để trống.", nameof(formName));

            var kindValue = BillLayoutFormKindHelper.ToStorageValue(formKind);
            var templateFile = BillLayoutFormKindHelper.GetDefaultTemplateFile(formKind);
            // Form lưu trong DB nên chuẩn hoá XML để editor (XDocument) mở được ngay.
            var defaultBytes = EnsureXmlMrt(
                await GetDefaultTemplateBytesAsync(templateFile, cancellationToken));

            return await WithDbAsync(async (context, ct) =>
            {
                var exists = await context.BillSeaLayoutForms
                    .AnyAsync(x => x.IsActive && x.FormName == trimmedName && x.FormKind == kindValue, ct);
                if (exists)
                    throw new InvalidOperationException($"Đã tồn tại form {BillLayoutFormKindHelper.GetDisplayName(formKind)} tên '{trimmedName}'.");

                var now = DateTime.UtcNow;
                var form = new M_BillSeaLayoutForm
                {
                    BillSeaLayoutFormId = Guid.NewGuid(),
                    FormName = trimmedName,
                    FormKind = kindValue,
                    MrtContent = defaultBytes,
                    SourceTemplate = templateFile,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = createdBy,
                    IsActive = true
                };

                context.BillSeaLayoutForms.Add(form);
                await context.SaveChangesAsync(ct);
                return form;
            }, cancellationToken);
        }

        public Task SaveFormDocumentAsync(Guid formId, XDocument document, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

                form.MrtContent = DocumentToBytes(document);
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public Task ResetFormToDefaultAsync(Guid formId, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

                form.MrtContent = EnsureXmlMrt(
                    await GetDefaultTemplateBytesAsync(form.SourceTemplate, ct));
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public Task DeleteFormAsync(Guid formId, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    return;

                form.IsActive = false;
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public async Task<bool> HasAttachFormAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            return form?.AttachMrtContent is { Length: > 0 };
        }

        public async Task<byte[]> GetAttachSkeletonBytesAsync(CancellationToken cancellationToken = default) =>
            await GetDefaultTemplateBytesAsync(BillSeaAttachMrtHelper.AttachSkeletonFileName, cancellationToken);

        public async Task<byte[]> BuildDefaultAttachBytesAsync(byte[] mainFormBytes, CancellationToken cancellationToken = default)
        {
            var skeletonBytes = await GetAttachSkeletonBytesAsync(cancellationToken);
            return BillSeaAttachMrtHelper.BuildAttachMrt(mainFormBytes, skeletonBytes);
        }

        public Task<M_BillSeaLayoutForm> CreateAttachFormAsync(Guid mainFormId, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == mainFormId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

                if (!BillLayoutFormKindHelper.SupportsAttach(BillLayoutFormKindHelper.ParseFormKind(form.FormKind)))
                    throw new InvalidOperationException("Form Attach chỉ áp dụng cho Bill Sea.");

                if (form.AttachMrtContent is { Length: > 0 })
                    throw new InvalidOperationException($"Form '{form.FormName}' đã có form Attach.");

                form.AttachMrtContent = await BuildDefaultAttachBytesAsync(form.MrtContent, ct);
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
                return form;
            }, cancellationToken);

        public async Task<byte[]> GetAttachFormBytesAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            if (form is null)
                throw new InvalidOperationException("Không tìm thấy form Bill Sea.");
            if (form.AttachMrtContent is not { Length: > 0 })
                throw new InvalidOperationException("Form này chưa có form Attach. Tạo tại menu 1.17.");

            return BillSeaAttachMrtHelper.EnsureExportableAttachMrt(form.AttachMrtContent);
        }

        public async Task<XDocument> LoadAttachFormDocumentAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var bytes = await GetAttachFormBytesAsync(formId, cancellationToken);
            using var stream = new MemoryStream(bytes);
            return XDocument.Load(stream);
        }

        public async Task<bool> IsAttachFormCustomizedAsync(Guid formId, CancellationToken cancellationToken = default)
        {
            var form = await GetFormAsync(formId, cancellationToken);
            if (form?.AttachMrtContent is not { Length: > 0 })
                return false;

            var defaultAttachBytes = await BuildDefaultAttachBytesAsync(form.MrtContent, cancellationToken);
            return !ContentEquals(form.AttachMrtContent, defaultAttachBytes);
        }

        public Task SaveAttachFormDocumentAsync(Guid formId, XDocument document, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill Sea.");

                form.AttachMrtContent = DocumentToBytes(document);
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public Task ResetAttachFormToDefaultAsync(Guid formId, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    throw new InvalidOperationException("Không tìm thấy form Bill Sea.");
                if (form.AttachMrtContent is not { Length: > 0 })
                    throw new InvalidOperationException("Form này chưa có form Attach.");

                form.AttachMrtContent = await BuildDefaultAttachBytesAsync(form.MrtContent, ct);
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public Task DeleteAttachFormAsync(Guid formId, CancellationToken cancellationToken = default) =>
            WithDbAsync(async (context, ct) =>
            {
                var form = await context.BillSeaLayoutForms
                    .FirstOrDefaultAsync(x => x.BillSeaLayoutFormId == formId && x.IsActive, ct);
                if (form is null)
                    return;

                form.AttachMrtContent = null;
                form.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(ct);
            }, cancellationToken);

        public static byte[] DocumentToBytes(XDocument document)
        {
            using var stream = new MemoryStream();
            document.Save(stream);
            return stream.ToArray();
        }

        private static bool ContentEquals(byte[] left, byte[] right) =>
            left.AsSpan().SequenceEqual(right);
    }
}
