using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Org.BouncyCastle.Ocsp;
using Stimulsoft.Blockly.Model;
using Stimulsoft.Report;
using Stimulsoft.Report.Blazor;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using System.Text.Json;
using System.Text.RegularExpressions;
using NVOAMASIS.Components.Report_RFQ.Pages;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using NVOAMASIS.Services.MultiTenant;
using static NVOAMASIS.Components.Report_RFQ.Pages.Index;

namespace NVOAMASIS.Services
{
    public class QuotationService(AppDbContext _context, IJSRuntime JSRuntime, AccountService asv, HistoryLogService HistoryLogService, BillSeaLayoutFormService billSeaLayoutFormService, ITenantContext tenantContext)
    {
        public async Task<List<M_Quotation>> GetListQuotation()
        {
            try
            {
                _context.ChangeTracker.Clear();
 
                var rs = await _context.Quotation.Where(x => x.quotationID != null).OrderByDescending(x => x.quotationNo).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        

        public async Task<List<M_Quotation>> GetListQuotation_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Quos = await _context.Quotation.Where(_ => _.quotationID == id).ToListAsync();
            return Quos;
        }

        public async Task<List<string>> UpdateOrCreateQuotation(M_Quotation IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.quotationID == null || IV.quotationID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Quotation Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Quotation Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Quotation Fail", "0"];
            }
        }

        public async Task<M_Quotation> GetDetailQuotationFromID(Guid? id)
        {
            _context.ChangeTracker.Clear();

            var Quo = await _context.Quotation.Where(_ => _.quotationID == id).FirstOrDefaultAsync();
            return Quo;
        }

        public async Task<BoolandMessReponse> DeleteQuotation(Models.M_Quotation QUO)
        {
            _context.ChangeTracker.Clear();
            try
            {
                if (QUO?.quotationID == null || QUO?.quotationID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");
                // Xóa các dòng trong bảng InvoiceDetail có Invoice_No khớp với InvFobCmt_ID
                var QuoDetails = _context.Quotation
                                             .Where(detail => detail.quotationID == QUO.quotationID)
                                             .ToList();

                if (QuoDetails.Any())
                {
                    _context.Quotation.RemoveRange(QuoDetails);
                }

                // Xóa bản ghi trong bảng INV_FOB_CMT
                _context?.Quotation.Remove(QUO!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Deleted");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot delete with error code: " + ex.Message);

            }
        }

        public async Task<BoolandMessReponse> DeleteQuotation_Debit(Models.M_Quotation QUO)
        {
            _context.ChangeTracker.Clear();
            try
            {
                if (QUO?.quotationID == null || QUO?.quotationID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");
                // Xóa các dòng trong bảng InvoiceDetail có Invoice_No khớp với InvFobCmt_ID
                var debits = _context.Debit
                                             .Where(detail => detail.quotationid == QUO.quotationID)
                                             .ToList();

                if (debits.Any())
                {
                    _context.Debit.RemoveRange(debits);
                }

                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Deleted");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot delete with error code: " + ex.Message);

            }
        }

        public async Task<BoolandMessReponse> DeleteQuotation_CreDit(Models.M_Quotation QUO)
        {
            _context.ChangeTracker.Clear();
            try
            {
                if (QUO?.quotationID == null || QUO?.quotationID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");
                // Xóa các dòng trong bảng InvoiceDetail có Invoice_No khớp với InvFobCmt_ID
                var credits = _context.Credit
                                             .Where(detail => detail.quotationid == QUO.quotationID)
                                             .ToList();

                if (credits.Any())
                {
                    _context.Credit.RemoveRange(credits);
                }

                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Deleted");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot delete with error code: " + ex.Message);

            }
        }
        public async Task<BoolandMessReponse> UpdateQuotationRequest(Models.M_Quotation BT)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Quotation.Update(BT);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Quotation Successfully");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Update Quotation Fail with Error code:" + ex.Message);
            }
        }
        public async Task<int> GetQuotation_No(string currentMonth, string currentYear)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rowWithMinRefNo = await _context.GetQuotationNo
                                                .Where(r => r.approve == true
                                                && r.nam == currentYear
                                                && r.thang == currentMonth)
                                                .OrderBy(r => r.quotation_number) // Sắp xếp theo RefNo từ nhỏ đến lớn
                                                .FirstOrDefaultAsync(); // Lấy dòng đầu tiên (nhỏ nhất)

                rowWithMinRefNo.approve = false;
                rowWithMinRefNo.userget = asv.GetAuth().Result.User.Identity.Name;
                rowWithMinRefNo.timeget = System.DateTime.Now;

                await _context.SaveChangesAsync();
                return rowWithMinRefNo.quotation_number!;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<int?> GetDebitNo(string currentMonth, string currentYear)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rowWithMinRefNo = await _context.RefNo
                                                .Where(r => r.used_debit == null 
                                                && r.nam == int.Parse(currentYear)
                                                && r.thang == int.Parse(currentMonth))
                                                .OrderBy(r => r.ref_number) // Sắp xếp theo RefNo từ nhỏ đến lớn
                                                .FirstOrDefaultAsync(); // Lấy dòng đầu tiên (nhỏ nhất)

                rowWithMinRefNo.used_debit = true;
                rowWithMinRefNo.userused_debit = asv.GetAuth().Result.User.Identity.Name;


                await _context.SaveChangesAsync();
                return rowWithMinRefNo.ref_number!;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<BoolandMessReponse> UpdateQuotation_Debit(M_Debit c,M_Debit c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Debit.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Debit Quotation", "Quotation", c.debitId, c.debitno, new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update Debit Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Debit with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> UpdateQuotation_Credit(M_Credit c,M_Credit c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Credit.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Credit Quotation", "Quotation", c.creditid, "", new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update Credit Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Credit with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> CreateQuotation_Debit(M_Debit c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.debitId = Guid.NewGuid();
                _context.Debit.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Debit Quotation", "Quotation", c.debitId, c.debitno, c);
                return new BoolandMessReponse(true, "Create Debit Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Debit with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateQuotation_Credit(M_Credit c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.creditid = Guid.NewGuid();
                _context.Credit.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Credit Quotation", "Quotation", c.creditid,"", c);
                return new BoolandMessReponse(true, "Create Credit Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Credit with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteDebit(M_Debit c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.debitId == null || c?.debitId == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Debit.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Credit with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteCredit(M_Credit c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.creditid == null || c?.creditid == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Credit.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Credit with error code: " + ex.Message);
            }
        }


        public async Task<List<ChargeModel>> Getcharge()
        {
            try
            {
                _context.ChangeTracker.Clear();

                var rs = await _context.Charge.Where(x => x.CHARGE != null).OrderByDescending(x => x.CHARGE).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public async Task<List<M_Debit>> GetListDebit(Guid ID)
        {
            try { 
                var Debits = await _context.Debit
                                    .Where(inv => inv.quotationid == ID)
                                    .ToListAsync();
                return Debits;
            }
            catch (Exception ex)
            {
                return new List<M_Debit>();
            }
        }

 

        public async Task<List<M_Credit>> GetListCredit(Guid ID)
        {
            try
            {
                var Credits = await _context.Credit
                                    .Where(inv => inv.quotationid == ID)
                                    .ToListAsync();
                return Credits;
            }
            catch (Exception ex)
            {
                return new List<M_Credit>();
            }
        }
        public async Task<List<M_Debit>> GetListDebit_quotationid(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Debit.Where(x => x.quotationid == id && x.continued == true).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Debit>();
            }
        }
        public async Task<BoolandMessReponse> ExportQuotation(Guid? id, string currency, Guid? layoutFormId = null)
        {
            try
            {
                var (templateBytes, reportLogo) = await ResolveQuotationTemplateAsync(layoutFormId);

                StiBlazorHelper.Initialize(JSRuntime);
                var report = StimulsoftLicenseHelper.CreateReport();
                report.Load(new MemoryStream(templateBytes));
                ApplyQuotationReportSetup(report, reportLogo);
                report.Dictionary.Variables["ID"].Value = id.ToString();
                report.Dictionary.Variables["Currency"].Value = currency;

                report.Dictionary.Variables["Total_amount"].Value =
                    (await ComputeQuotationTotalAsync(id, currency)).ToString("#,##0.##");

                try
                {
                    StimulsoftLicenseHelper.PrepareAndRender(report);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new BoolandMessReponse(false, "Export failed!, Error code: " + ex.Message);
                }

                using (var ms = new MemoryStream())
                {
                    report.ExportDocument(StiExportFormat.Pdf, ms);
                    var pdfData = ms.ToArray();
                    await JSRuntime.InvokeVoidAsync("openReportInNewTab", pdfData);
                }

                return new BoolandMessReponse(true, "Export successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new BoolandMessReponse(false, "Export failed!, Error code: " + ex.Message);
            }
        }

        public async Task<(bool, string, byte[])> ExportQuotation_file(Guid? id, Guid? layoutFormId = null)
        {
            try
            {
                var (templateBytes, reportLogo) = await ResolveQuotationTemplateAsync(layoutFormId);

                StiBlazorHelper.Initialize(JSRuntime);
                var report = StimulsoftLicenseHelper.CreateReport();
                report.Load(new MemoryStream(templateBytes));
                ApplyQuotationReportSetup(report, reportLogo);
                report.Dictionary.Variables["ID"].Value = id.ToString();
                if (report.Dictionary.Variables.Contains("Currency")
                    && string.IsNullOrWhiteSpace(report.Dictionary.Variables["Currency"].Value))
                    report.Dictionary.Variables["Currency"].Value = "VND";
                var currency = report.Dictionary.Variables.Contains("Currency")
                    ? report.Dictionary.Variables["Currency"].Value
                    : "VND";
                report.Dictionary.Variables["Total_amount"].Value =
                    (await ComputeQuotationTotalAsync(id, currency)).ToString("#,##0.##");

                try
                {
                    StimulsoftLicenseHelper.PrepareAndRender(report);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return (false, "Export failed! Error: " + ex.Message, null);
                }

                using (var ms = new MemoryStream())
                {
                    report.ExportDocument(StiExportFormat.Pdf, ms);
                    return (true, "Export successfully!", ms.ToArray());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return (false, "Export failed! Error: " + ex.Message, null);
            }
        }

        private async Task<(byte[] TemplateBytes, byte[]? Logo)> ResolveQuotationTemplateAsync(Guid? layoutFormId)
        {
            if (layoutFormId is Guid formId && formId != Guid.Empty)
            {
                var form = await billSeaLayoutFormService.GetFormAsync(formId);
                if (form is null || !form.IsActive)
                    throw new InvalidOperationException("Không tìm thấy form Quotation.");

                if (BillLayoutFormKindHelper.ParseFormKind(form.FormKind) != BillLayoutFormKind.Quotation)
                    throw new InvalidOperationException("Form đã chọn không phải loại Quotation.");

                var templateBytes = await billSeaLayoutFormService.GetFormBytesAsync(formId);
                // Logo riêng của form nếu có, không thì CompanyInfo.Logo.
                var logo = await billSeaLayoutFormService.GetEffectiveFormLogoAsync(formId);
                return (templateBytes, logo);
            }

            var defaultBytes = await billSeaLayoutFormService.GetDefaultTemplateBytesAsync(BillSeaReportTemplateNames.Quotation);
            var companyLogo = await billSeaLayoutFormService.GetCompanyLogoAsync();
            return (defaultBytes, companyLogo);
        }

        private void ApplyQuotationReportSetup(StiReport report, byte[]? companyLogo)
        {
            ApplyReportConnectionString(report, ResolveReportConnectionString());
            AlignCompanyInfoExpressions(report);
            ApplyLogoToImageComponent(report, "Image1", companyLogo);
        }

        /// <summary>
        /// Quotation.mrt đặt data source tên <c>companyinfomation</c>, nhưng một số text
        /// (và form đã lưu) dùng <c>{CompanyInfomation.xxx}</c>. Stimulsoft compile C# phân biệt hoa/thường.
        /// </summary>
        private static void AlignCompanyInfoExpressions(StiReport report)
        {
            string? sourceName = null;
            foreach (StiDataSource ds in report.Dictionary.DataSources)
            {
                if (string.Equals(ds.Name, "companyinfomation", StringComparison.OrdinalIgnoreCase))
                {
                    sourceName = ds.Name;
                    break;
                }
            }

            if (string.IsNullOrEmpty(sourceName))
                return;

            foreach (StiComponent component in report.GetComponents())
            {
                if (component is not StiText text)
                    continue;

                var current = text.Text?.Value;
                if (string.IsNullOrEmpty(current))
                    continue;

                var updated = Regex.Replace(
                    current,
                    @"\{CompanyInfomation\.",
                    "{" + sourceName + ".",
                    RegexOptions.IgnoreCase);

                if (!string.Equals(current, updated, StringComparison.Ordinal))
                    text.Text.Value = updated;
            }
        }

        private string ResolveReportConnectionString()
        {
            tenantContext.EnsureInitializedFromHttpContext();
            if (!string.IsNullOrWhiteSpace(tenantContext.ConnectionString))
                return tenantContext.ConnectionString;

            var connectionString = _context.Database.GetConnectionString();
            if (!string.IsNullOrWhiteSpace(connectionString))
                return connectionString;

            return _context.Database.GetDbConnection().ConnectionString;
        }

        private static void ApplyReportConnectionString(StiReport report, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            if (!report.Dictionary.Variables.Contains("connectDB"))
                report.Dictionary.Variables.Add(new StiVariable("connectDB", connectionString));
            else
                report.Dictionary.Variables["connectDB"].Value = connectionString;

            foreach (StiDatabase database in report.Dictionary.Databases)
            {
                if (database is not StiSqlDatabase sqlDatabase)
                    continue;

                sqlDatabase.ConnectionString = connectionString;
            }
        }

        private async Task<double> ComputeQuotationTotalAsync(Guid? id, string? currency)
        {
            _context.ChangeTracker.Clear();
            // Khớp SQL của Quotation.mrt (không lọc continued) để TOTAL trùng các dòng trên bill.
            var listDebit = await _context.Debit.Where(x => x.quotationid == id).ToListAsync();
            double total = 0;
            var useVnd = string.Equals(currency, "VND", StringComparison.OrdinalIgnoreCase);
            foreach (var item in listDebit)
            {
                var amount = item.thanhtiensauthue ?? 0;
                var rate = item.tigiadebit is > 0 ? item.tigiadebit.Value : 1;
                if (useVnd)
                    total += item.tiente == "VND" ? amount : amount * rate;
                else
                    total += item.tiente == "USD" ? amount : amount / rate;
            }

            return total;
        }

        private static void ApplyLogoToImageComponent(StiReport report, string componentName, byte[]? logo)
        {
            if (logo is not { Length: > 0 })
                return;

            // Giữ ImageURL = resource://Logo như form gốc; chỉ thay nội dung resource.
            if (report.Dictionary.Resources.Contains("Logo"))
                report.Dictionary.Resources["Logo"].Content = logo;

            foreach (StiComponent component in report.GetComponents())
            {
                if (component is not StiImage image)
                    continue;

                if (!string.Equals(image.Name, componentName, StringComparison.OrdinalIgnoreCase))
                    continue;

                image.Enabled = true;
                image.Stretch = true;
                image.AspectRatio = true;
                if (image.ImageURL != null)
                    image.ImageURL.Value = "resource://Logo";
                if (image.Expressions is { Count: > 0 })
                {
                    for (var i = image.Expressions.Count - 1; i >= 0; i--)
                    {
                        if (string.Equals(image.Expressions[i].Name, "Enabled", StringComparison.OrdinalIgnoreCase))
                            image.Expressions.RemoveAt(i);
                    }
                }
            }
        }

        public async Task<string?> GetEmailAsync(string? user)
        {
            try
            {
                _context.ChangeTracker.Clear();

                var rs = await _context.UserList
                                       .Where(x => x.Name == user)
                                       .Select(x => x.Email) // Chỉ lấy cột Email
                                       .FirstOrDefaultAsync(); // Lấy giá trị đầu tiên hoặc null

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                return null;
            }
        }

        //---------------------------------------------------------------------------------------------------------
        public async Task<BoolandMessReponse> CreateRFQ_Detail(M_RFQ c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.RFQ_ID = Guid.NewGuid();
                _context.RFQ.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Request For Quotation List Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Request For Quotation List with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateRFQ(M_RFQ IV)
        {
            try
            {
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                _context.ChangeTracker.Clear();
                if (IV.RFQ_ID == null || IV.RFQ_ID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
               

                    return ["Create new Request For Quotation List Line Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
    
                    return ["Update Request For Quotation Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Request For Quotation Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> DeleteRFQ_Detail(M_RFQ c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.RFQ_ID == null || c?.RFQ_ID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.RFQ.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task DeleteRFQ_Log_By_RFQid(Guid? id)
        {
            _context.ChangeTracker.Clear();

            if (id == null || id == Guid.Empty)
                throw new ArgumentException("Invalid ID to delete.");

            var rfqLogs = await _context.RFQ_Log.Where(x => x.RFQID == id).ToListAsync();

            if (rfqLogs == null || !rfqLogs.Any())
                return; // Không có gì để xóa, nên return luôn

            _context.RFQ_Log.RemoveRange(rfqLogs);
            await _context.SaveChangesAsync();
        }

        public async Task<BoolandMessReponse> UpdateRFQ_Detail(M_RFQ c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.RFQ.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Request For Quotation List Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Request For Quotation List with error code: " + ex.Message);
            }
        }

        public async Task<List<M_RFQ>> GetListRFQ_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.RFQ.Where(_ => _.RFQ_ID == id).ToListAsync();
            return Invoices;
        }

        public async Task<List<M_RFQ>> GetList_RFQ()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_RFQ> rs = new();

                rs = _context.RFQ.OrderByDescending(x => x.RFQNo).ToList();
       
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_RFQ>();
            }
        }

        public async Task<List<M_Quotation>> GetList_Quotation()
        {
            try
            {
                AuthUser user = new AuthUser();
                user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();
                List<M_Quotation> rs = new();

                if (user.Department == "ADMIN")
                {
                    rs = _context.Quotation.OrderByDescending(x => x.quotationNo).ToList();
                }
                else
                {
                    rs = _context.Quotation
                        .Where(x=>x.SaleName==user.Name)
                        .OrderByDescending(x => x.quotationNo).ToList();
                }

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_Quotation>();
            }
        }

        public async Task<List<M_RFQ>> GetList_RFQ_combo()
        {

            var Invoices = await _context.RFQ.OrderBy(x => x.RFQNo).ToListAsync();

            if (Invoices == null)
                return new List<M_RFQ>();
            Invoices.Insert(0, new M_RFQ { RFQNo = "" });
            return Invoices;
        }

        public async Task<List<M_RFQ_Log>> GetListRFQ_Log(Guid ID)
        {
            try
            {
                var RFQ_Logs = await _context.RFQ_Log
                                    .Where(inv => inv.RFQID == ID)
                                    .ToListAsync();
                return RFQ_Logs;
            }
            catch (Exception ex)
            {
                return new List<M_RFQ_Log>();
            }
        }

        public async Task<BoolandMessReponse> UpdateRFQ_Log(M_RFQ_Log c,M_RFQ_Log c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.RFQ_Log.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                string RFQNo =await GetMaQuo_ByID(c.RFQID);
                await HistoryLogService.LogAsync(usr, "UpdateDetails", "RFQ_Log", c.RFQ_logID, RFQNo, new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update RFQ_Log Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update RFQ_Log with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> CreateRFQ_Log(M_RFQ_Log c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.RFQ_logID = Guid.NewGuid();
                _context.RFQ_Log.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                string RFQNo = await GetMaQuo_ByID(c.RFQID);
                await HistoryLogService.LogAsync(usr, "ADDDetails", "RFQ_Log", c.RFQ_logID, RFQNo,  c );
                return new BoolandMessReponse(true, "Create RFQ_Log Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add RFQ_Log with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteRFQ_Log(M_RFQ_Log c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.RFQ_logID == null || c?.RFQ_logID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.RFQ_Log.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete RFQ_Log with error code: " + ex.Message);
            }
        }

        public async Task UpdateTrangThai_HetHieuLuc_RFQ()
        {
            try
            {
                _context.ChangeTracker.Clear();

                var today = DateTime.Today;

                var listToUpdate = await _context.RFQ
                    .Where(x => x.CreatedAt.HasValue)
                    .ToListAsync();

            
                foreach (var item in listToUpdate)
                {
                    if (item.RFQStatus == "Sent" || item.RFQStatus == "Quoted" || item.RFQStatus == "Negotiating")
                    {
                        var daysDifference = (today - item.CreatedAt.Value.Date).TotalDays;
                        if (daysDifference > 7)
                        {
                            item.RFQStatus = "Expired";
                        }
                    }
                   
                }

                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi cập nhật trạng thái: " + ex.Message);
            }
        }
        //------------------------------------------------------------------------------------
        public async Task<List<M_Quotation>> GetReport_Quo_dagui(DateTime? fromDate, DateTime? toDate)
        {

            AuthUser user = new AuthUser();
            user = asv.GetUserDetail();
            if (user.Department == "ADMIN")
            {
                var RPT_Quos = await _context.Quotation
                    .Where(x => (!fromDate.HasValue || x.validDate >= fromDate.Value) && (!toDate.HasValue || x.validDate <= toDate.Value) && (x.status == "SEND"))
                    .ToListAsync();
                return RPT_Quos;
            }
            else
            {
                var RPT_Quos = await _context.Quotation
                    .Where(x => (!fromDate.HasValue || x.validDate >= fromDate.Value) && (!toDate.HasValue || x.validDate <= toDate.Value) && (x.status == "SEND") && (x.SaleName == user.Name))
                    .ToListAsync();
                return RPT_Quos;

            }
         
        }
        public async Task<List<M_Quotation>> GetReport_Quo_dagui_by_month(int fromthang, int fromnam, int tothang, int tonam)
        {
            double? total_debit_vnd;
            double? total_debit_usd;
            var fromDate = new DateTime(fromnam, fromthang, 1);
            var toDate = new DateTime(tonam, tothang, 1).AddMonths(1).AddDays(-1);

        

            AuthUser user = new AuthUser();
            user = asv.GetUserDetail();
            if (user.Department == "ADMIN")
            {
                var RPT_Quos = await _context.Quotation
                              .Where(x => (x.validDate >= fromDate && x.validDate <= toDate) && (x.status == "SEND"))
                              .ToListAsync();

                return RPT_Quos;
            }
            else
            {
          
                var RPT_Quos = await _context.Quotation
                         .Where(x => (x.validDate >= fromDate && x.validDate <= toDate) && (x.status == "SEND") && (x.SaleName == user.Name))
                         .ToListAsync();

                return RPT_Quos;
            }

        }

        //------------------------------------------------------------------------------------
        public async Task<List<M_Quotation>> GetReport_Quo_toBK(DateTime? fromDate, DateTime? toDate)
        {
            AuthUser user = new AuthUser();
            user = asv.GetUserDetail();
            if (user.Department == "ADMIN")
            {
                var RPT_Quos = await _context.Quotation
                            .Where(x => (!fromDate.HasValue || x.validDate >= fromDate.Value) && (!toDate.HasValue || x.validDate <= toDate.Value) && (!string.IsNullOrEmpty(x.BookingNo)))
                            .ToListAsync();

                return RPT_Quos;

            }
            else
            {
                var RPT_Quos = await _context.Quotation
                     .Where(x => (!fromDate.HasValue || x.validDate >= fromDate.Value) && (!toDate.HasValue || x.validDate <= toDate.Value) && (!string.IsNullOrEmpty(x.BookingNo)) && (x.SaleName == user.Name))
                     .ToListAsync();

                return RPT_Quos;

            }

        }
        public async Task<int> GetReport_SL_Quo_ALL(DateTime? fromDate, DateTime? toDate)
        {

            AuthUser user = new AuthUser();
            user = asv.GetUserDetail();
            if (user.Department == "ADMIN")
            {
                var count = await _context.Quotation
                .Where(x => (!fromDate.HasValue || x.validDate >= fromDate.Value) &&
                            (!toDate.HasValue || x.validDate <= toDate.Value))
                .CountAsync();

                return count;

            }
            else
            {
                var count = await _context.Quotation
                 .Where(x => (!fromDate.HasValue || x.validDate >= fromDate.Value) &&
                             (!toDate.HasValue || x.validDate <= toDate.Value) && (x.SaleName == user.Name))
                 .CountAsync();

                return count;

            }
        }
        public async Task<List<M_Quotation>> GetReport_Quo_toBK_by_month(int fromthang, int fromnam, int tothang, int tonam)
        {
            double? total_debit_vnd;
            double? total_debit_usd;
            var fromDate = new DateTime(fromnam, fromthang, 1);
            var toDate = new DateTime(tonam, tothang, 1).AddMonths(1).AddDays(-1);

    
            AuthUser user = new AuthUser();
            user = asv.GetUserDetail();
            if (user.Department == "ADMIN")
            {
                var RPT_Quos = await _context.Quotation
                                .Where(x => (x.validDate >= fromDate && x.validDate <= toDate) && (!string.IsNullOrEmpty(x.BookingNo)))
                                .ToListAsync();

                return RPT_Quos;


            }
            else
            {
                var RPT_Quos = await _context.Quotation
                     .Where(x => (x.validDate >= fromDate && x.validDate <= toDate) && (!string.IsNullOrEmpty(x.BookingNo)) && (x.SaleName == user.Name))
                     .ToListAsync();

                return RPT_Quos;

      

            }

        }

        public async Task<List<M_Quotation>> GetReport_SL_Quo_ALL_by_month(int fromthang, int fromnam, int tothang, int tonam)
        {
            double? total_debit_vnd;
            double? total_debit_usd;
            var fromDate = new DateTime(fromnam, fromthang, 1);
            var toDate = new DateTime(tonam, tothang, 1).AddMonths(1).AddDays(-1);

       
            AuthUser user = new AuthUser();
            user = asv.GetUserDetail();
            if (user.Department == "ADMIN")
            {
                var RPT_Quos = await _context.Quotation
                            .Where(x => (x.validDate >= fromDate && x.validDate <= toDate))
                            .ToListAsync();

                return RPT_Quos;

            }
            else
            {
                var RPT_Quos = await _context.Quotation
                        .Where(x => (x.validDate >= fromDate && x.validDate <= toDate) && (x.SaleName == user.Name))
                        .ToListAsync();

                return RPT_Quos;
   
            }

        }
        //------------------------------------------------------------------------------------
        public async Task<List<M_Quotation>> GetReport_Quo_accept_Deny(DateTime? fromDate, DateTime? toDate)
        {

       

            AuthUser user = new AuthUser();
            user = asv.GetUserDetail();
            if (user.Department == "ADMIN")
            {
                var RPT_Quos = await _context.Quotation
                             .Where(x => (!fromDate.HasValue || x.validDate >= fromDate.Value) && (!toDate.HasValue || x.validDate <= toDate.Value) && (x.status == "ACCEPT" || x.status == "DENY"))
                             .ToListAsync();

                return RPT_Quos;
            }
            else
            {
                var RPT_Quos = await _context.Quotation
                           .Where(x => (!fromDate.HasValue || x.validDate >= fromDate.Value) && (!toDate.HasValue || x.validDate <= toDate.Value) && (x.status == "ACCEPT" || x.status == "DENY") && (x.SaleName == user.Name))
                           .ToListAsync();

                return RPT_Quos;
       
            }

        }
        public async Task<List<M_Quotation>> GetReport_Quo_accept_deny_by_month(int fromthang, int fromnam, int tothang, int tonam)
        {
            double? total_debit_vnd;
            double? total_debit_usd;
            var fromDate = new DateTime(fromnam, fromthang, 1);
            var toDate = new DateTime(tonam, tothang, 1).AddMonths(1).AddDays(-1);

       
            AuthUser user = new AuthUser();
            user = asv.GetUserDetail();
            if (user.Department == "ADMIN")
            {
                var RPT_Quos = await _context.Quotation
                        .Where(x => (x.validDate >= fromDate && x.validDate <= toDate) && (x.status == "ACCEPT" || x.status == "DENY"))
                        .ToListAsync();

                return RPT_Quos;

            }
            else
            {
                var RPT_Quos = await _context.Quotation
                        .Where(x => (x.validDate >= fromDate && x.validDate <= toDate) && (x.status == "ACCEPT" || x.status == "DENY") && (x.SaleName == user.Name))
                        .ToListAsync();

                return RPT_Quos;
          

            }
        }

        //------------------------------------------------------------------------------------

        public async Task<List<M_Report_RFQ>> GetReport_RFQ(DateTime? fromDate, DateTime? toDate)
        {
            double? total_debit_vnd;
            double? total_debit_usd;
            var RFQs = await _context.RFQ
              .Where(x => (!fromDate.HasValue || x.CreatedAt >= fromDate.Value) && (!toDate.HasValue || x.CreatedAt <= toDate.Value))
              .Select(    x => new M_Report_RFQ
              {

                  RFQ_No =x.RFQNo,
                  Customer_id = x.CustomerID,
                  Status=x.RFQStatus,
                  Date_Created = x.CreatedAt,
                  Note =x.SpecialNote
 
              }).ToListAsync();

            // Tính toán tổng số tiền sau khi lấy dữ liệu về
            foreach (var rfq in RFQs)
            {
                rfq.Total_Amount_VND = GetTotalSotien_VND(rfq.RFQ_No);
                rfq.Total_Amount_USD = GetTotalSotien_USD(rfq.RFQ_No);
            }

            return RFQs.OrderByDescending(x => x.RFQ_No).ToList();
        }

        public async Task<List<M_Report_RFQ>> GetReport_RFQ_by_month(int fromthang,int fromnam,int tothang , int tonam)
        {
            double? total_debit_vnd;
            double? total_debit_usd;
            var fromDate = new DateTime(fromnam, fromthang, 1);
            var toDate = new DateTime(tonam, tothang, 1).AddMonths(1).AddDays(-1);

            var RFQs = await _context.RFQ
                .Where(x => x.CreatedAt >= fromDate && x.CreatedAt <= toDate)
                .Select(x => new M_Report_RFQ
                {
                    RFQ_No = x.RFQNo,
                    Customer_id = x.CustomerID,
                    Status = x.RFQStatus,
                    Date_Created = x.CreatedAt,
                    Note = x.SpecialNote
                })
                .ToListAsync();

            // Tính toán tổng số tiền sau khi lấy dữ liệu về
            foreach (var rfq in RFQs)
            {
                rfq.Total_Amount_VND = GetTotalSotien_VND(rfq.RFQ_No);
                rfq.Total_Amount_USD = GetTotalSotien_USD(rfq.RFQ_No);
            }

            return RFQs.OrderByDescending(x => x.RFQ_No).ToList();
        }

        public double? GetTotalSotien_VND(string RFQNo)
        {
            try
            {
     
                var quotations = _context.Quotation
                    .Where(q => q.RFQ_No == RFQNo)
                    .ToList();

                var totalSotien = (from debit in _context.Debit
                                   join quotation in _context.Quotation on debit.quotationid equals quotation.quotationID
                                   where quotation.RFQ_No == RFQNo && debit.tiente == "VND"
                                   select (double?)debit.thanhtiensauthue).Sum();

                return totalSotien ?? 0; 
            }
            catch (Exception ex)
            {
             
                return 0; 
            }
        }


        public double? GetTotalSotien_USD(string RFQNo)
        {
            try
            {

                var quotations = _context.Quotation
                    .Where(q => q.RFQ_No == RFQNo)
                    .ToList();

                var totalSotien = (from debit in _context.Debit
                                   join quotation in _context.Quotation on debit.quotationid equals quotation.quotationID
                                   where quotation.RFQ_No == RFQNo && debit.tiente == "USD"
                                   select (double?)debit.thanhtiensauthue).Sum();

                return totalSotien ?? 0;
            }
            catch (Exception ex)
            {

                return 0;
            }
        }


        public async Task<List<M_Quotation>> GetListQuotation_ByID(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();

                var Quo = await _context.Quotation.Where(_ => _.quotationID == id).ToListAsync();
                return Quo;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string?> GetMaQuo_ByID(Guid? id)
        {
            try
            {
                AuthUser user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                var rs = await _context.RFQ
                    .Where(x => x.RFQ_ID == id)
                    .OrderBy(x => x.RFQNo)
                    .Select(x => x.RFQNo) // chỉ lấy cột MaRFQ
                    .FirstOrDefaultAsync(); // lấy phần tử đầu tiên hoặc null nếu không có

                return rs;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

    }
}
