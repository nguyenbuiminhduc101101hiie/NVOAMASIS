using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Stimulsoft.Report;
using Stimulsoft.Report.Blazor;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using NVOAMASIS.Components.CUSTOMER.Pages;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using NVOAMASIS.Services.MultiTenant;

namespace NVOAMASIS.Services
{
    public class LenhDieuXeServices(
        AppDbContext _context,
        IWebHostEnvironment _env,
        IJSRuntime JSRuntime,
        AccountService asv,
        ITenantContext tenantContext,
        BillSeaLayoutFormService formService)
    {
        private async Task ApplyReportSetupAsync(StiReport report)
        {
            ApplyReportConnectionString(report, ResolveReportConnectionString());

            var logo = await _context.CompanyInfomation
                .AsNoTracking()
                .Select(x => x.Logo)
                .FirstOrDefaultAsync();

            if (logo is { Length: > 0 })
                ApplyCompanyLogoToReport(report, logo);
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

        private static void ApplyCompanyLogoToReport(StiReport report, byte[] logo)
        {
            // Template dùng Image1 -> ImageURL = resource://Logo
            if (report.Dictionary.Resources.Contains("Logo"))
                report.Dictionary.Resources["Logo"].Content = logo;

            foreach (StiComponent component in report.GetComponents())
            {
                if (component is not StiImage image)
                    continue;

                if (!string.Equals(image.Name, "Image1", StringComparison.OrdinalIgnoreCase))
                    continue;

                image.Enabled = true;
                image.Stretch = true;
                image.AspectRatio = true;

                if (image.ImageURL != null)
                    image.ImageURL.Value = "resource://Logo";

                // Set Image để đảm bảo hiển thị ngay cả khi runtime không resolve resource.
                image.Image = CreateLogoImage(logo);
            }
        }

        private static System.Drawing.Image? CreateLogoImage(byte[]? logo)
        {
            if (logo is not { Length: > 0 })
                return null;

            using var ms = new MemoryStream(logo);
            using var temp = System.Drawing.Image.FromStream(ms);
            return new System.Drawing.Bitmap(temp);
        }

        public async Task<List<M_LenhDieuXe>> GetListLenhDieuXe()
        {
            try
            {


                _context.ChangeTracker.Clear();
                List<M_LenhDieuXe> rs = new();

                rs = _context.LenhDieuXe.OrderByDescending(x => x.Lenhdieuxeno).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_LenhDieuXe>();
            }
        }
        public async Task<List<M_LenhDieuXe>> GetListLDX_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.LenhDieuXe.Where(_ => _.Id == id).ToListAsync();
            return Invoices;
        }

        public async Task<BoolandMessReponse> CreateLenhDieuXe_Detail(M_LenhDieuXe c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();
                _context.LenhDieuXe.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Lệnh Điều Xe Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Lệnh Điều Xe with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateLenhDieuXe(M_LenhDieuXe IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Lệnh Điều Xe Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Lệnh Điều Xe Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Lệnh Điều Xe Fail", "0"];
            }
        }


        public async Task<BoolandMessReponse> DeleteLenhDieuXe_Detail(M_LenhDieuXe c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.LenhDieuXe.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateLenhDieuXe_Detail(M_LenhDieuXe c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.LenhDieuXe.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Lệnh Điều Xe Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Lệnh Điều Xe with error code: " + ex.Message);
            }
        }
        public async Task<List<M_Customer>> GetList_Cus_Driver()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Customer.OrderBy(x => x.Customer_Code)
                    .Where(x => x.MainCode.Contains("Driver"))
                    .ToListAsync();
        
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Customer>();
            }

        }

        public async Task<List<M_Customer>> GetList_Cus_Personal()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Customer.OrderBy(x => x.Customer_Code)
                    .Where(x => x.MainCode.Contains("Personal"))
                    .ToListAsync();
            
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Customer>();
            }

        }
        public async Task<List<M_Customer>> GetList_Cus_Trucking()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Customer.OrderBy(x => x.Customer_Code)
                    .Where(x => x.MainCode.Contains("Trucking"))
                    .ToListAsync();
        
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Customer>();
            }

        }
        public async Task<List<PortModel>> GetList_port()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Port.OrderBy(x => x.PORT)
                
                    .ToListAsync();
     
                return rs;
            }
            catch (Exception ex)
            {
                return new List<PortModel>();
            }

        }

        public async Task<BoolandMessReponse> ExportLenhDieuXe(Guid? id, Guid? layoutFormId = null)
        {
            try
            {
                //Create empty report object
                var report = new StiReport();

                // Load MRT template (từ 1.17 nếu có chọn form)
                byte[] templateBytes;
                if (layoutFormId is Guid lfId)
                    templateBytes = await formService.GetFormBytesAsync(lfId);
                else
                    templateBytes = await formService.GetDefaultTemplateBytesAsync(
                        BillLayoutFormKindHelper.GetDefaultTemplateFile(BillLayoutFormKind.LenhDieuXe));

                StiBlazorHelper.Initialize(JSRuntime);
                report = StimulsoftLicenseHelper.CreateReport();
                report.Load(new MemoryStream(templateBytes));

                await ApplyReportSetupAsync(report);
                var lenhdieuxeinfo = await Get_LDXinfo(id);
       

                report.Dictionary.Variables["ID"].Value = id.ToString();
                report.Dictionary.Variables["tongkm"].Value = (lenhdieuxeinfo.Tongkm ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["dinhmuc"].Value = (lenhdieuxeinfo.Dinhmucdau ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["tamung"].Value = (lenhdieuxeinfo.Tamung ?? 0).ToString("#,##0.##");
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
        public async Task<M_LenhDieuXe?> GetLenhDieuXeByTruckNo(string? Lenhdieuxeno)
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.LenhDieuXe.FirstOrDefaultAsync(x => x.Lenhdieuxeno == Lenhdieuxeno);
            return rs;
        }
        public async Task<List<string>> GetListTruckNo()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.LenhDieuXe.Select(x => x.Lenhdieuxeno).ToListAsync();
            return rs;
        }

        public async Task<M_LenhDieuXe> Get_LDXinfo(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.LenhDieuXe.Where(x => x.Id == id).FirstOrDefaultAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new M_LenhDieuXe();
            }
        }
        public M_LenhDieuXe? GetDetailByNo(string? no)
        {
            _context.ChangeTracker.Clear();
            var rs = _context.LenhDieuXe.FirstOrDefault(_ => _.Lenhdieuxeno == no);
            return rs;
        }
    }
}
