using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using NVOAMASIS.Components.BaoCaoQuyTienMat;
using NVOAMASIS.Components.BaoCaoQuyTienMat.Pages;
using static NVOAMASIS.Components.BaoCaoQuyTienMat.Pages.BaoCaoQuyTienMat_Index;
using Microsoft.JSInterop;
using Stimulsoft.Report.Blazor;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using System.Globalization;
using NVOAMASIS.Components.CUSTOMER.Pages;
using NVOAMASIS.Services.MultiTenant;

namespace NVOAMASIS.Services
{
    public class PhieuThu_Chi_Services(AppDbContext _context, IWebHostEnvironment _env, AccountService asv ,CustomerService Cussv, IJSRuntime JSRuntime, ITenantContext tenantContext)
    {
        public async Task<List<M_PhieuThu>> GetList_PhieuThu()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_PhieuThu> rs = new();

                rs = _context.Phieuthu.OrderByDescending(x => x.SoPhieuthu).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_PhieuThu>();
            }
        }
        public async Task<List<M_PhieuChi>> GetList_PhieuChi()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_PhieuChi> rs = new();

                rs = _context.Phieuchi.OrderByDescending(x => x.Sophieuchi).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_PhieuChi>();
            }
        }

        public async Task<List<M_PhieuKeToan>> GetList_PhieuKeToan()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_PhieuKeToan> rs = new();

                rs = _context.Phieuketoan.OrderByDescending(x => x.Sophieuketoan).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_PhieuKeToan>();
            }
        }
        //------------------------------------------------------
        public async Task<BoolandMessReponse> CreatePhieuThu_Detail(M_PhieuThu c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.PhieuthuID = Guid.NewGuid();
                _context.Phieuthu.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Receipt List Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Receipt List with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreatePhieuThu(M_PhieuThu IV)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ChangeTracker.Clear();
                var isNew = IV.PhieuthuID == Guid.Empty;
                if (isNew)
                {
                    IV.PhieuthuID = Guid.NewGuid();
                    // Phiếu mới (kể cả Duplicate) luôn chưa thanh toán.
                    IV.dathanhtoan = false;
                    IV.NgayThanhToan = null;
                    _context.Add(IV);
                }
                else
                {
                    _context.Update(IV);
                    // Trạng thái thanh toán chỉ đổi bằng webhook SePay hoặc checkbox trên lưới,
                    // không để dialog sửa ghi đè giá trị cũ.
                    _context.Entry(IV).Property(x => x.dathanhtoan).IsModified = false;
                    _context.Entry(IV).Property(x => x.NgayThanhToan).IsModified = false;
                }

                if (IV.DebitSelectionSpecified)
                    await SynchronizeReceiptDebitsAsync(IV.PhieuthuID, IV.SelectedDebitIds);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return [isNew ? "Create new Receipt Successfully" : "Update Receipt Successfully", "1"];
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return [$"Save Receipt Fail: {ex.Message}", "0"];
            }
        }

        public async Task<List<M_Debit>> GetReceiptDebitsByJobAsync(Guid jobId, Guid? receiptId = null)
        {
            return await (
                from debit in _context.Debit.AsNoTracking()
                join hbl in _context.HBL.AsNoTracking() on debit.hblid equals hbl.hblID
                where (hbl.Jobid == jobId || _context.MBL.Any(mbl => mbl.MblID == hbl.mblid && mbl.Jobid == jobId))
                      && debit.continued == true
                      && (debit.dathanhtoan != true || debit.PhieuthuID == receiptId)
                orderby debit.debitno
                select debit).ToListAsync();
        }

        public async Task<List<M_Debit>> GetReceiptDebitsAsync(Guid receiptId)
        {
            return await _context.Debit.AsNoTracking()
                .Where(x => x.PhieuthuID == receiptId)
                .ToListAsync();
        }

        private async Task SynchronizeReceiptDebitsAsync(Guid receiptId, IEnumerable<Guid> selectedDebitIds)
        {
            var selectedIds = selectedDebitIds.Distinct().ToHashSet();
            var currentDebits = await _context.Debit
                .Where(x => x.PhieuthuID == receiptId)
                .ToListAsync();

            foreach (var debit in currentDebits.Where(x => !selectedIds.Contains(x.debitId)))
            {
                debit.PhieuthuID = null;
                debit.dathanhtoan = false;
            }

            if (selectedIds.Count == 0)
                return;

            var selectedDebits = await _context.Debit
                .Where(x => selectedIds.Contains(x.debitId))
                .ToListAsync();

            if (selectedDebits.Count != selectedIds.Count)
                throw new InvalidOperationException("Một hoặc nhiều phí Debit không còn tồn tại.");

            if (selectedDebits.Any(x => x.dathanhtoan == true && x.PhieuthuID != receiptId))
                throw new InvalidOperationException("Một hoặc nhiều phí Debit đã được thanh toán bởi phiếu thu khác.");

            foreach (var debit in selectedDebits)
            {
                debit.PhieuthuID = receiptId;
                debit.dathanhtoan = true;
            }
        }
    

        public async Task<BoolandMessReponse> DeletePhieuThu_Detail(M_PhieuThu c)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.PhieuthuID == null || c?.PhieuthuID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                var debits = await _context.Debit.Where(x => x.PhieuthuID == c.PhieuthuID).ToListAsync();
                foreach (var debit in debits)
                {
                    debit.PhieuthuID = null;
                    debit.dathanhtoan = false;
                }

                _context.Phieuthu.Remove(c);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdatePhieuthu_Detail(M_PhieuThu c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Phieuthu.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Receipt  Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Receipt  with error code: " + ex.Message);
            }
        }

        public async Task<List<M_PhieuThu>> GetListPhieuthu_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.Phieuthu.Where(_ => _.PhieuthuID == id).ToListAsync();
            return Invoices;
        }
        //------------------------------------------------------------------------------------

        public async Task<BoolandMessReponse> CreatePhieuChi_Detail(M_PhieuChi c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.PhieuchiID = Guid.NewGuid();
                _context.Phieuchi.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Payment Voucher Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Payment Voucher with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreatePhieuChi(M_PhieuChi IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.PhieuchiID == null || IV.PhieuchiID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    IV.PhieuchiID = Guid.NewGuid();
                    // Phiếu chi mới luôn chưa thanh toán.
                    IV.dathanhtoan = false;
                    IV.NgayThanhToan = null;
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Receipt Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    // Trạng thái thanh toán chỉ đổi bằng webhook SePay hoặc checkbox trên lưới,
                    // không để dialog sửa ghi đè giá trị cũ.
                    _context.Entry(IV).Property(x => x.dathanhtoan).IsModified = false;
                    _context.Entry(IV).Property(x => x.NgayThanhToan).IsModified = false;
                    await _context.SaveChangesAsync();
                    return ["Update Receipt Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Receipt Fail", "0"];
            }
        }


        public async Task<BoolandMessReponse> DeletePhieuChi_Detail(M_PhieuChi c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.PhieuchiID == null || c?.PhieuchiID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Phieuchi.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdatePhieuChi_Detail(M_PhieuChi c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Phieuchi.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Receipt  Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Receipt  with error code: " + ex.Message);
            }
        }

        public async Task<List<M_PhieuChi>> GetListPhieuChi_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.Phieuchi.Where(_ => _.PhieuchiID == id).ToListAsync();
            return Invoices;
        }
        //------------------------------------------------------------------------------------
    
        public async Task<List<BaoCaoQuyTienMat_Index.M_BaocaoQuyTienMat>> GetBaoCaoQuyTienMat(DateTime? fromDate, DateTime? toDate)
        {

            var thu = await _context.Phieuthu
              .Where(x => (!fromDate.HasValue || x.Ngay >= fromDate.Value) && (!toDate.HasValue || x.Ngay <= toDate.Value))
              .Select(x => new M_BaocaoQuyTienMat
              {
                  Ngay = x.Ngay,
                  Customer_id = x.Customer_Id,
                  Loaigiaodich =x.Loaiphieu,
                  Sotien = x.Sotien,
                  Tigia =x.Tigia,
                  GhiChu=x.Noidung,
                  Currency =x.Currency,
                  Sotienthu = x.Sotien
                  


              }).ToListAsync();


            var chi = await _context.Phieuchi
                .Where(x => (!fromDate.HasValue || x.Ngay >= fromDate.Value) && (!toDate.HasValue || x.Ngay <= toDate.Value))
                .Select(x => new M_BaocaoQuyTienMat
                {
                    Ngay = x.Ngay,
                    Customer_id = x.Customer_ID,
                    Loaigiaodich = x.Loaiphieu,
                    Sotien = x.Sotien,
                    Tigia = x.Tigia,
                    GhiChu = x.Noidung,
                    Currency = x.Currency,
                    Sotienchi = x.Sotien
                }).ToListAsync();

            return thu.Concat(chi).OrderByDescending(x => x.Ngay).ToList();
        }

        public async Task<double> getsodudauky_bydate(DateTime? fromDate,string Cur)
        {
            if(Cur == "VND")
            {
                var tongthu = await _context.Phieuthu
                    .Where(x => (!fromDate.HasValue || x.Ngay < fromDate))
                    .SumAsync(x =>
                        x.Currency == "USD"
                            ? ((double?)x.Sotien ?? 0) * ((double?)x.Tigia ?? 1)  // nếu Currency là USD thì nhân với tỷ giá
                            : ((double?)x.Sotien ?? 0)                             // nếu Currency là VND thì chỉ lấy Sotien
                    );

                var tongchi = await _context.Phieuchi
                    .Where(x => (!fromDate.HasValue || x.Ngay < fromDate.Value))
                    .SumAsync(x =>
                        x.Currency == "USD"
                            ? ((double?)x.Sotien ?? 0) * ((double?)x.Tigia ?? 1)  
                            : ((double?)x.Sotien ?? 0)                             
                    );

                return tongthu - tongchi;
            }
            else
            {
                var tongthu = await _context.Phieuthu
                    .Where(x => (!fromDate.HasValue || x.Ngay < fromDate))
                    .SumAsync(x =>
                        x.Currency == "VND"
                            ? Math.Round(((double?)x.Sotien ?? 0) / ((double?)x.Tigia ?? 1), 3) 
                            : ((double?)x.Sotien ?? 0)                                              
                    );

                var tongchi = await _context.Phieuchi
                    .Where(x => (!fromDate.HasValue || x.Ngay < fromDate.Value))
                    .SumAsync(x =>
                        x.Currency == "VND"
                            ? Math.Round(((double?)x.Sotien ?? 0) / ((double?)x.Tigia ?? 1), 3)  
                            : ((double?)x.Sotien ?? 0)                                            
                    );

                return tongthu - tongchi;
            }
           
        }

        //------------------------------------------------------
        public async Task<BoolandMessReponse> CreatePhieuketoan_Detail(M_PhieuKeToan c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.PhieuketoanID = Guid.NewGuid();
                _context.Phieuketoan.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Accounting Voucher List Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Accounting Voucher List with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreatePhieuketoan(M_PhieuKeToan IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.PhieuketoanID == null || IV.PhieuketoanID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Accounting Voucher Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Accounting Voucher Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Accounting Voucher Fail", "0"];
            }
        }


        public async Task<BoolandMessReponse> DeletePhieuketoan_Detail(M_PhieuKeToan c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.PhieuketoanID == null || c?.PhieuketoanID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Phieuketoan.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdatePhieuketoan_Detail(M_PhieuKeToan c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Phieuketoan.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Receipt  Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Receipt  with error code: " + ex.Message);
            }
        }

        public async Task<List<M_PhieuKeToan>> GetListPhieuketoan_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.Phieuketoan.Where(_ => _.PhieuketoanID == id).ToListAsync();
            return Invoices;
        }

        public async Task<BoolandMessReponse> ExportBill_PKT(Guid id,string sophieu)
        {
            try
            {
                //Create empty report object
                var report = new StiReport();
                //Load report template
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "PhieuKeToan.mrt");

                StiBlazorHelper.Initialize(JSRuntime);

                report = StimulsoftLicenseHelper.CreateReport();
         
                report.Load(rpt);
                await ApplyPhieuReportSetupAsync(report);
                report.Culture = "en-US";
                report.Dictionary.Variables["ID"].Value = id.ToString();
                report.Dictionary.Variables["SoPhieuKeToan"].Value = sophieu;
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

                var list_PKT = await GetListPKT_SOphieu(sophieu);

                double? total = 0;
         
                foreach (var item_PKT in list_PKT)
                {
                    total += item_PKT.SotienVnd;
                }
                report.Dictionary.Variables["Total"].Value = (total ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(total, "VND");


                StimulsoftLicenseHelper.PrepareAndRender(report);
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

        public async Task<List<M_PhieuThu>> GetListPT_id(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Phieuthu.Where(x => x.PhieuthuID == id).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_PhieuThu>();
            }
        }
        public async Task<List<M_PhieuChi>> GetListPC_id(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Phieuchi.Where(x => x.PhieuchiID == id).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_PhieuChi>();
            }
        }
        public async Task<List<M_PhieuKeToan>> GetListPKT_SOphieu(string? sophieu)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Phieuketoan.Where(x => x.Sophieuketoan == sophieu ).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_PhieuKeToan>();
            }
        }

        public async Task<BoolandMessReponse> ExportBill_PT(Guid id)
        {
            try
            {
                //Create empty report object
                var report = new StiReport();
                //Load report template
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "PhieuThu.mrt");

                StiBlazorHelper.Initialize(JSRuntime);

                report = StimulsoftLicenseHelper.CreateReport();

                report.Load(rpt);
                await ApplyPhieuReportSetupAsync(report);
                report.Culture = "en-US";
                report.Dictionary.Variables["ID"].Value = id.ToString();

                report.Dictionary.Variables["DatetimeNow"].Value =
                  "Ngày " + DateTime.Now.ToString("dd", CultureInfo.InvariantCulture) +
                  " Tháng " + DateTime.Now.ToString("MM", CultureInfo.InvariantCulture) +
                  " Năm " + DateTime.Now.ToString("yyyy", CultureInfo.InvariantCulture);


                var list_PT = await GetListPT_id(id);


                double? sotien = 0;

                foreach (var item_PT in list_PT)
                {
                    if (item_PT.Currency == "VND")
                    {
                        sotien += item_PT.Sotien;
                    }
                    else
                    {
                        sotien += item_PT.Sotien * item_PT.Tigia;
                    }
                }
                report.Dictionary.Variables["Total"].Value = (sotien ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(sotien, "VND");

                report.Dictionary.Variables["lido"].Value = "Lí do nộp:";
                // Thu tiền mặt (Nợ 111x) in PHIẾU THU, thu chuyển khoản (CK / Nợ 112x) in ỦY NHIỆM THU
                var firstPT = list_PT.FirstOrDefault();
                var tkNo = firstPT?.TKNo?.Trim() ?? string.Empty;
                var isChuyenKhoan = !tkNo.StartsWith("111")
                    && (tkNo.StartsWith("112") || string.Equals(firstPT?.PTTT?.Trim(), "CK", StringComparison.OrdinalIgnoreCase));
                report.Dictionary.Variables["Tenphieu"].Value = isChuyenKhoan ? "ỦY NHIỆM THU" : "PHIẾU THU";
                report.Dictionary.Variables["nguoinhan_noptien"].Value = "Người Nộp Tiền";
                StimulsoftLicenseHelper.PrepareAndRender(report);
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

        public async Task<BoolandMessReponse> ExportBill_PC(Guid id)
        {
            try
            {
                //Create empty report object
                var report = new StiReport();
                //Load report template
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "PhieuChi.mrt");

                StiBlazorHelper.Initialize(JSRuntime);

                report = StimulsoftLicenseHelper.CreateReport();

                report.Load(rpt);
                await ApplyPhieuReportSetupAsync(report);
                report.Culture = "en-US";
                report.Dictionary.Variables["ID"].Value = id.ToString();

                report.Dictionary.Variables["DatetimeNow"].Value =
                  "Ngày " + DateTime.Now.ToString("dd", CultureInfo.InvariantCulture) +
                  " Tháng " + DateTime.Now.ToString("MM", CultureInfo.InvariantCulture) +
                  " Năm " + DateTime.Now.ToString("yyyy", CultureInfo.InvariantCulture);


                var list_PT = await GetListPT_id(id);


                double? sotien = 0;

                foreach (var item_PT in list_PT)
                {
                    if (item_PT.Currency == "VND")
                    {
                        sotien += item_PT.Sotien;
                    }
                    else
                    {
                        sotien += item_PT.Sotien * item_PT.Tigia;
                    }
                }
                report.Dictionary.Variables["Total"].Value = (sotien ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(sotien, "VND");

                report.Dictionary.Variables["lido"].Value = "Lí do chi:";
                report.Dictionary.Variables["Tenphieu"].Value = "PHIẾU CHI";
                report.Dictionary.Variables["nguoinhan_noptien"].Value = "Người Nhận Tiền";
                StimulsoftLicenseHelper.PrepareAndRender(report);
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

        private async Task ApplyPhieuReportSetupAsync(StiReport report)
        {
            ApplyReportConnectionString(report, ResolveReportConnectionString());
            var logo = await _context.CompanyInfomation
                .AsNoTracking()
                .Select(x => x.Logo)
                .FirstOrDefaultAsync();
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

        private static void ApplyCompanyLogoToReport(StiReport report, byte[]? logo)
        {
            if (logo is not { Length: > 0 })
                return;

            // Image1 dùng resource://Logo — chỉ thay nội dung resource, giữ ImageURL.
            if (report.Dictionary.Resources.Contains("Logo"))
                report.Dictionary.Resources["Logo"].Content = logo;

            foreach (StiComponent component in report.GetComponents())
            {
                if (component is not StiImage image)
                    continue;

                if (!string.Equals(image.Name, "Image1", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(image.Name, "Image2", StringComparison.OrdinalIgnoreCase))
                    continue;

                image.Enabled = true;
                image.Stretch = true;
                image.AspectRatio = true;
                var imageUrl = image.ImageURL?.Value ?? string.Empty;
                if (!imageUrl.Contains("Logo", StringComparison.OrdinalIgnoreCase))
                    image.ImageURL.Value = "resource://Logo";
            }
        }

        public static string ConvertToWords(double? number, string currency)
        {
            if (number == null) return currency == "VND" ? "Không đồng" : "Zero dollars";

            long dollars = (long)number;
            int cents = (int)Math.Round((number.Value - dollars) * 100);

            string dollarText = dollars > 0 ? ConvertIntegerToWords(dollars, currency) + (currency == "VND" ? " đồng" : " dollars") : "";
            string centText = cents > 0 ? ConvertIntegerToWords(cents, currency) + (currency == "VND" ? " xu" : " cents") : "";

            string result;
            if (currency == "VND")
            {
                result = dollarText != "" ? dollarText : "không đồng";
            }
            else // USD
            {
                if (dollars > 0 && cents > 0)
                    result = $"{dollarText} and {centText}";
                else if (dollars > 0)
                    result = dollarText;
                else
                    result = centText;
            }

            // Viết hoa chữ cái đầu
            return char.ToUpper(result[0]) + result.Substring(1);
        }


        private static readonly string[] UnitsMapEng = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
        "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };

        private static readonly string[] TensMapEng = { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

        private static readonly string[] UnitsMapVn = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín", "mười",
        "mười một", "mười hai", "mười ba", "mười bốn", "mười lăm", "mười sáu", "mười bảy", "mười tám", "mười chín" };

        private static readonly string[] TensMapVn = { "", "", "hai mươi", "ba mươi", "bốn mươi", "năm mươi", "sáu mươi", "bảy mươi", "tám mươi", "chín mươi" };

        private static string ConvertIntegerToWords(long number, string currency)
        {
            if (number == 0)
                return currency == "VND" ? "không" : "zero";

            string[] UnitsMap = currency == "VND" ? UnitsMapVn : UnitsMapEng;
            string[] TensMap = currency == "VND" ? TensMapVn : TensMapEng;

            if (number < 20)
                return UnitsMap[number];

            if (number < 100)
            {
                int unit = (int)(number % 10);
                string unitText = UnitsMap[unit];

                if (currency == "VND")
                {
                    if (unit == 5) unitText = "lăm";  // "5" ở cuối thành "lăm"
                    if (unit == 1) unitText = "mốt"; // "1" ở cuối thành "mốt"
                }

                return TensMap[number / 10] + (unit > 0 ? " " + unitText : "");
            }

            if (number < 1000)
                return UnitsMap[number / 100] + (currency == "VND" ? " trăm" : " hundred") +
                    (number % 100 > 0 ? (currency == "VND" ? " " : " and ") + ConvertIntegerToWords(number % 100, currency) : "");

            if (number < 1_000_000)
                return ConvertIntegerToWords(number / 1000, currency) + (currency == "VND" ? " nghìn" : " thousand") +
                    (number % 1000 > 0 ? " " + ConvertIntegerToWords(number % 1000, currency) : "");

            if (number < 1_000_000_000)
                return ConvertIntegerToWords(number / 1_000_000, currency) + (currency == "VND" ? " triệu" : " million") +
                    (number % 1_000_000 > 0 ? " " + ConvertIntegerToWords(number % 1_000_000, currency) : "");

            return ConvertIntegerToWords(number / 1_000_000_000, currency) + (currency == "VND" ? " tỷ" : " billion") +
                (number % 1_000_000_000 > 0 ? " " + ConvertIntegerToWords(number % 1_000_000_000, currency) : "");
        }


    }
}
