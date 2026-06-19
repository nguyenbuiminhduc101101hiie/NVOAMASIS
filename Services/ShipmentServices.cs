using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using OfficeOpenXml;
using Org.BouncyCastle.Bcpg;
using Stimulsoft.Report;
using Stimulsoft.Report.Blazor;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.Export;
using Stimulsoft.Report.Web;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NVOAMASIS.Components.Quotation.Pages;
using NVOAMASIS.Components.Shipment.Pages;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using NVOAMASIS.Services.MultiTenant;

using ZXing;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static MudBlazor.CategoryTypes;
using static Stimulsoft.Report.Func;
using static Stimulsoft.Report.StiRecentConnections;
using static NVOAMASIS.Components.Layout.DynamicTabs;

using System.Text.Json;
using System.Linq;

namespace NVOAMASIS.Services
{
    public class ShipmentService(AppDbContext _context, IWebHostEnvironment _env, IJSRuntime JSRuntime, AccountService asv, SupportServices supsv, HistoryLogService HistoryLogService, IDbContextFactory<AppDbContext> _dbFactory, ITenantContext _tenantContext, BillSeaLayoutFormService billSeaLayoutFormService)
    {
        public async Task<List<M_Job>> GetListJobByMBLs(List<M_MBL> listdata)
        {
            _context.ChangeTracker.Clear();
            var jobids = listdata.Select(x => x.Jobid).Distinct().ToList();
            var ids = string.Join(",", jobids.Select(id => $"'{id}'"));
            var rs = await _context.Job.FromSqlRaw($"SELECT * FROM Job where Jobid in ({ids})").ToListAsync();
            return rs;
        }

        public async Task<List<M_Job>> GetListJob_by_roles_Dept()
        {
            AuthUser user = new AuthUser();
            user = asv.GetUserDetail();
            _context.ChangeTracker.Clear();

            //set them roles cua user
            var rs = await _context.Job
                .Where(x => x.Continued == true
                            && user.Roles_Dept.Contains(x.Loai)
                            && (string.IsNullOrEmpty(user.CompanyCode) || x.CompanyCode == user.CompanyCode))
                .OrderByDescending(x => x.Dateupdate)
                .ToListAsync();

            return rs;
        }

        public async Task<List<M_Job>> GetListJob()
        {
            AuthUser user = new AuthUser();
            user = asv.GetUserDetail();
            _context.ChangeTracker.Clear();


            var rs = await _context.Job.Where(x => x.Continued == true).OrderByDescending(x => x.Dateupdate).ToListAsync();
            return rs;
        }
        public async Task<List<M_Job>> GetListJob(MudBlazor.DateRange dateRange)
        {
            //_context.ChangeTracker.Clear();
            //var rs = await _context.Job.Where(x => x.Continued == true).ToListAsync();
            //rs = rs.Where(x => x.Datecreate >= dateRange.Start && x.Datecreate <= dateRange.End).OrderByDescending(x => x.Dateupdate).ToList();
            //return rs;
            _context.ChangeTracker.Clear();

            var start = dateRange.Start?.Date;
            var end = dateRange.End?.Date;

            var query = from job in _context.Job
                        join mbl in _context.MBL on job.JobID equals mbl.Jobid
                        join hbl in _context.HBL on mbl.MblID equals hbl.mblid
                        where job.Continued == true &&
                              hbl.datereport.HasValue &&
                              hbl.datereport.Value.Date >= start &&
                              hbl.datereport.Value.Date <= end
                        select job;

            var result = await query
                .Distinct() // tránh trùng nếu nhiều HBL cùng Job
                .OrderByDescending(j => j.Dateupdate)
                .ToListAsync();

            return result;
        }


        public async Task<List<M_HBL>> GetListHBL_DateRange_whereLoai(MudBlazor.DateRange dateRange, bool chkSI, bool chkSE, bool chkAI, bool chkAE, bool chkTruck, bool ChkCustom, bool chksearchsale, string Salename)
        {
            var listLoai = new List<string>();

            if (chkSI == true)
            {
                listLoai.Add("SI");
            }
            if (chkSE == true)
            {
                listLoai.Add("SE");
            }
            if (chkAE == true)
            {
                listLoai.Add("AE");
            }
            if (chkAI == true)
            {
                listLoai.Add("AI");
            }
            if (chkTruck == true)
            {
                listLoai.Add("Truck");
            }
            if (ChkCustom == true)
            {
                listLoai.Add("Customs");
            }



            _context.ChangeTracker.Clear();
            var rs = await (
           from hbl in _context.HBL
           join mbl in _context.MBL on hbl.mblid equals mbl.MblID
           join job in _context.Job on mbl.Jobid equals job.JobID
           where hbl.Continued == true &&
                 hbl.datereport >= dateRange.Start &&
                 hbl.datereport <= dateRange.End &&
                 (
                    (!chkSI && !chkSE && !chkAI && !chkAE && !chkTruck && !ChkCustom)
                    ||
                    (chkSI && job.Loai == "SI")
                    ||
                    (chkSE && job.Loai == "SE")
                    ||
                    (chkAI && job.Loai == "AI")
                    ||
                    (chkAE && job.Loai == "AE")
                    ||
                    (chkTruck && job.Loai == "Truck")
                    ||
                    (ChkCustom && job.Loai == "Customs")
                ) &&
                 (!chksearchsale || hbl.SaleName == Salename)
           select hbl
            ).ToListAsync();
            return rs;


        }


        public async Task<BoolandMessReponse> UpdateJobDetail(M_Job c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.Dateupdate = DateTime.Now;
                _context.Job.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Job Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Job with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateJobDetail(M_Job c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.Datecreate =
                c.Dateupdate = DateTime.Now;
                _context.Job.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Job Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Job with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateHBLDetailNotFromMBL(M_HBL c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.HBL.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create HBL Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add HBL with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteJobDetail(M_Job c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.JobID == null || c?.JobID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");
                var listdetailmbl = await _context.MBL.Where(x => x.Jobid == c.JobID).ToListAsync();
                var listdetailhbl = new List<M_HBL>();
                var listdebit = new List<M_Debit>();
                var listcredit = new List<M_Credit>();
                foreach (var itemmbl in listdetailmbl)
                {
                    var detailhbl = await _context.HBL.Where(x => x.mblid == itemmbl.MblID).ToListAsync();
                    var debitmbl = await _context.Debit.Where(x => x.mblid == itemmbl.MblID).ToListAsync();
                    var creditmbl = await _context.Credit.Where(x => x.mblid == itemmbl.MblID).ToListAsync();
                    listdebit.AddRange(debitmbl);
                    listcredit.AddRange(creditmbl);
                    listdetailhbl.AddRange(detailhbl);
                }
                foreach (var itemhbl in listdetailhbl)
                {
                    var debithbl = await _context.Debit.Where(x => x.hblid == itemhbl.hblID).ToListAsync();
                    var credithbl = await _context.Credit.Where(x => x.hblid == itemhbl.hblID).ToListAsync();
                    listdebit.AddRange(debithbl);
                    listcredit.AddRange(credithbl);
                }

                _context.Debit.RemoveRange(listdebit);
                _context.Credit.RemoveRange(listcredit);
                _context.MBL.RemoveRange(listdetailmbl);
                _context.HBL.RemoveRange(listdetailhbl);
                _context?.Job.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Job with error code: " + ex.Message);
            }
        }
        public async Task<List<M_HBL>> GetListHBL(Guid? mblid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.HBL.Where(x => x.mblid == mblid).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_HBL>();
            }
        }
        public async Task<List<M_HBL>> GetListHBLByJobid(Guid? Jobid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.HBL.Where(x => x.Jobid == Jobid).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_HBL>();
            }
        }

        public async Task<M_HBL> GetHBL_byHBLid(Guid? hblid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.HBL.Where(x => x.hblID == hblid).FirstOrDefaultAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new M_HBL();
            }
        }
        public async Task<List<M_CuocCont>> GetCuocCOnt_hblid(Guid? hblid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.CuocCont.Where(x => x.hblid == hblid).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_CuocCont>();
            }
        }
        public async Task<M_MBL> GetMBL_byHBLid(Guid? mblid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.MBL.Where(x => x.MblID == mblid).FirstOrDefaultAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new M_MBL();
            }
        }

        public async Task<List<M_MBL>> GetMBL_from_mblid(Guid? mblid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.MBL.Where(x => x.MblID == mblid).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_MBL>();
            }
        }

        public async Task<M_Job> GetJob_byid(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Job.Where(x => x.JobID == id).FirstOrDefaultAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new M_Job();
            }
        }

        public async Task<List<M_HBL>> GetListHBLALL()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.HBL.ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_HBL>();
            }
        }

        public async Task<List<M_HBL>> GetListHBLALLBY_Dept_Import()
        {
            try
            {
                _context.ChangeTracker.Clear();

                var rs = await _context.HBL
                    .Include(h => h.MBL)
                        .ThenInclude(m => m.Job)
                    .Where(h => h.MBL != null &&
                                h.MBL.Job != null &&
                                (h.MBL.Job.Loai == "SI" || h.MBL.Job.Loai == "AI"))
                    .ToListAsync();

                return rs;
            }
            catch (Exception)
            {
                return new List<M_HBL>();
            }
        }

        public async Task<List<M_HBL>> GetListHBLALLBY_Dept_Export()
        {
            try
            {
                _context.ChangeTracker.Clear();

                var rs = await _context.HBL
                    .Include(h => h.MBL)
                        .ThenInclude(m => m.Job)
                    .Where(h => h.MBL != null &&
                                h.MBL.Job != null &&
                                (h.MBL.Job.Loai == "SE" || h.MBL.Job.Loai == "AE"))
                    .ToListAsync();

                return rs;
            }
            catch (Exception)
            {
                return new List<M_HBL>();
            }
        }
        public async Task<List<M_HBL>> GetListHBLBySale(string? SaleName)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.HBL.Where(x => x.SaleName == SaleName).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_HBL>();
            }
        }


        public async Task<BoolandMessReponse> UpdateHBLDetail(M_HBL c, M_HBL c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.HBL.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update HBL", "Shipment", c.hblID, c.hbl, new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update HBL Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update HBL with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> UpdateHBLDetail_approve(M_HBL c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.HBL.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                //await HistoryLogService.LogAsync(usr, "Update HBL Approve", "Shipment", c.hblID, c.hbl, c);
                return new BoolandMessReponse(true, "Update HBL Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update HBL with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> CreateHBLDetail(M_HBL c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                //coppy list: deb,cre,cont tu mblid
                var listdeb = await GetListDebitMBL(c.mblid);
                var listcre = await GetListCreditMBL(c.mblid);
                var listcont = GetListContainerMBL(c.mblid);

                c.hblID = Guid.NewGuid();

                listdeb.ForEach(x => x.debitId = Guid.NewGuid());
                listdeb.ForEach(x => x.mblid = Guid.Empty);
                listdeb.ForEach(x => x.hblid = c.hblID);
                foreach (var group in listdeb.GroupBy(x => x.debitno)) // tạo số debit mới
                {
                    var debitnonew = await GetDebitNo(c.mblid);
                    foreach (var item in group)
                        item.debitno = debitnonew;
                }

                listcre.ForEach(x => x.creditid = Guid.NewGuid());
                listcre.ForEach(x => x.mblid = Guid.Empty);
                listcre.ForEach(x => x.hblid = c.hblID);

                listcont.ForEach(x => x.CTN_ID = Guid.NewGuid());
                listcont.ForEach(x => x.mblid = null);
                listcont.ForEach(x => x.hblid = c.hblID);

                _context.HBL.Add(c!);
                _context.Debit.AddRange(listdeb);
                _context.Credit.AddRange(listcre);
                _context.Container.AddRange(listcont);

                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create HBL Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add HBL with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateTruckDetail(M_HBL c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now.ToString("dd/MMM/yyyy");
                c.hblID = Guid.NewGuid();
                _context.HBL.Add(c!);

                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD", "Shipment", c.hblID, c.hbl, c);
                return new BoolandMessReponse(true, "Create HBL Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add HBL with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteHBLDetail(M_HBL c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.hblID == null || c?.hblID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.HBL.Remove(c!);
                var listdebitbyhblid = await _context.Debit.Where(x => x.hblid == c.hblID).ToListAsync();
                var listcreditbyhblid = await _context.Credit.Where(x => x.hblid == c.hblID).ToListAsync();
                _context.Debit.RemoveRange(listdebitbyhblid);
                _context.Credit.RemoveRange(listcreditbyhblid);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete HBL with error code: " + ex.Message);
            }
        }
        public async Task<List<M_MBL>> GetListMBL(Guid? Jobid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.MBL.Where(x => x.Jobid == Jobid).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_MBL>();
            }
        }
        public async Task<List<M_MBL>> GetListMBLALL()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.MBL.ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_MBL>();
            }
        }
        public async Task<List<M_MBL>> GetListMBLHBLs(List<M_HBL> Listdata)
        {
            try
            {
                _context.ChangeTracker.Clear();

                var mblids = Listdata
                    .Where(x => x.mblid != Guid.Empty)
                    .Select(x => x.mblid.ToString())  // Convert Guid -> string
                    .Distinct()
                    .ToList();

                if (!mblids.Any())
                {
                    return new List<M_MBL>();
                }

                var rs = await _context.MBL
                    .Where(m => m.MblID.ToString().Contains(mblids.ToString()))
                    .ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách MBL: {ex.Message}", ex);
            }
        }
        public async Task<BoolandMessReponse> UpdateMBLDetail(M_MBL c, M_MBL c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.MBL.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update MBL", "Shipment", c.MblID, c.Mbl, new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update MBL Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update MBL with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> UpdateMBLDetail_approve(M_MBL c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.MBL.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                //await HistoryLogService.LogAsync(usr, "Update MBL Approve", "Shipment", c.MblID, c.Mbl, c);
                return new BoolandMessReponse(true, "Update MBL Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update MBL with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> CreateMBLDetail(M_MBL c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.MblID = Guid.NewGuid();
                _context.MBL.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD MBL", "Shipment", c.MblID, c.Mbl, c);

                return new BoolandMessReponse(true, "Create MBL Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add MBL with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteMBLDetail(M_MBL c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.MblID == null || c?.MblID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");
                var detailhbl = await _context.HBL.Where(x => x.mblid == c!.MblID).ToListAsync();
                _context?.MBL.Remove(c!);
                _context?.HBL.RemoveRange(detailhbl);
                foreach (var item in detailhbl)
                {
                    var listdebitbyhblid = await _context!.Debit.Where(x => x.hblid == item.hblID).ToListAsync();
                    var listcreditbyhblid = await _context.Credit.Where(x => x.hblid == item.hblID).ToListAsync();
                    _context.Debit.RemoveRange(listdebitbyhblid);
                    _context.Credit.RemoveRange(listcreditbyhblid);
                }
                var listdebitbymblid = await _context!.Debit.Where(x => x.mblid == c!.MblID).ToListAsync();
                var listcreditbymblid = await _context.Credit.Where(x => x.hblid == c!.MblID).ToListAsync();
                _context.Debit.RemoveRange(listdebitbymblid);
                _context.Credit.RemoveRange(listcreditbymblid);

                await _context?.SaveChangesAsync()!;
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete MBL", "MBL", c.MblID, c.Mbl, new { OldData = c });
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete MBL with error code: " + ex.Message);
            }
        }
        public async Task<List<M_Container>> GetListContainerMBLAsync(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Container.Where(x => x.mblid == id && x.CONTINUED == true).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Container>();
            }
        }
        public List<M_Container> GetListContainerMBL(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.Container.Where(x => x.mblid == id && x.CONTINUED == true).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Container>();
            }
        }
        public async Task<BoolandMessReponse> UpdateContainerMBL(M_Container c, M_Container c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.UPDATETIME = DateTime.Now;
                _context.Container.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Continer", "Shipment", c.CTN_ID, c.CONTAINER_NO, new { OldData = c_old, NewData = c });

                return new BoolandMessReponse(true, "Update Container Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Container with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> UpdateContainerMBL_Approve(M_Container c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.UPDATETIME = DateTime.Now;
                _context.Container.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                //await HistoryLogService.LogAsync(usr, "Update Continer Approve", "Shipment", c.CTN_ID, c.CONTAINER_NO, c);

                return new BoolandMessReponse(true, "Update Container Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Container with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateDataSayContainerMBL(M_MBL m, M_Container c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.UPDATETIME = DateTime.Now;
                _context.Container.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Container Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Container with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateContainerMBL(M_Container c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.CTN_ID = Guid.NewGuid();
                _context.Container.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Continer", "Shipment", c.CTN_ID, c.CONTAINER_NO, c);
                return new BoolandMessReponse(true, "Create Container Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Container with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteContainerMBL(M_Container c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.CTN_ID == null || c?.CTN_ID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Container.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete MBL with error code: " + ex.Message);
            }
        }
        public List<M_Container> GetListContainerAll()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.Container.Where(x => x.CONTINUED == true).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Container>();
            }
        }

        public List<string> GetDistinctOwnerNames()
        {
            try
            {
                var rs = _context.Container
                    .Where(x => x.CONTINUED == true && x.OwnerName != null && x.OwnerName.Trim() != "")
                    .Select(x => x.OwnerName!.Trim())
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
                return rs ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
        public List<M_Container> GetListContainerHBL(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.Container.Where(x => x.hblid == id && x.CONTINUED == true).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Container>();
            }
        }
        public async Task<List<M_Credit>> GetListCreditMBL(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Credit.Where(x => x.mblid == id && x.continued == true).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Credit>();
            }
        }
        public async Task<List<M_Credit>> GetListCreditHBL(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Credit.Where(x => x.hblid == id && x.continued == true).ToListAsync();
            return rs;
        }
        public async Task<List<M_Credit>> GetListCreditHBL(Guid? id, bool isCommission)
        {
            _context.ChangeTracker.Clear();
            var query = $@"select c.* from credit c join CHARGE ch on c.itemid = ch.CHARGE_ID 
                        where CHARGE_CODE {(isCommission ? string.Empty : "not")} like '%COMMISSION%' and c.hblid = '{id}' and c.continued = 1";
            var rs = await _context.Credit.FromSqlRaw(query).ToListAsync();
            return rs;
        }
        public async Task<BoolandMessReponse> UpdateCreditMBL_DNTT(M_Credit c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now.ToString();

                _context.Credit.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Credit DNTT =true", "DNTT", c.creditid, "", c);
                return new BoolandMessReponse(true, "Update Credit Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Credit with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateCreditMBL(M_Credit c, M_Credit c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now.ToString();
                _context.Credit.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Credit", "Shipment", c.creditid, "", new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update Credit Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Credit with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateCreditMBL_approve(M_Credit c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now.ToString();
                _context.Credit.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                //await HistoryLogService.LogAsync(usr, "Update Credit Approve", "Shipment", c.creditid, "", c);
                return new BoolandMessReponse(true, "Update Credit Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Credit with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateCreditMBL_Copy_Credit(M_Credit c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now.ToString();
                _context.Credit.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;

                await HistoryLogService.LogAsync(usr, "Update Credit Copy", "Shipment", c.creditid, "", c);
                return new BoolandMessReponse(true, "Update Debit Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Debit with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> CreateCreditMBL(M_Credit c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.creditid = Guid.NewGuid();
                _context.Credit.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Credit", "Shipment", c.creditid, "", c);
                return new BoolandMessReponse(true, "Create Credit Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Credit with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteCreditMBL(M_Credit c)
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

        public async Task<List<M_Debit>> GetListDebitMBL(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Debit.Where(x => x.mblid == id && x.continued == true).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Debit>();
            }
        }

        public async Task<List<M_Debit>> GetListDebit_Debitno(string? debitno)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Debit.Where(x => x.debitno == debitno && x.continued == true).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Debit>();
            }
        }
        public async Task<BoolandMessReponse> UpdateDebitMBL(M_Debit c, M_Debit c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now.ToString();
                _context.Debit.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Debit", "Shipment", c.debitId, c.debitno, new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update Debit Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Debit with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> UpdateCuocCont(M_CuocCont c, M_CuocCont c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now.ToString();
                _context.CuocCont.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Cuoc Cont", "Shipment", c.id, c.No, new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update Cuoc Cont Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Cuoc Cont with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateDebitMBL_Approve(M_Debit c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now.ToString();
                _context.Debit.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                //await HistoryLogService.LogAsync(usr, "Update Debit Aprrove", "Shipment", c.debitId, c.debitno, c);
                return new BoolandMessReponse(true, "Update Debit Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Debit with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateDebitMBL_Copy_Debit(M_Debit c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now.ToString();
                _context.Debit.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;

                await HistoryLogService.LogAsync(usr, "Update Debit", "Shipment", c.debitId, c.debitno, c);
                return new BoolandMessReponse(true, "Update Debit Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Debit with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> UpdateDebitMBL(List<M_Debit> c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Debit.UpdateRange(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Debits Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Debit with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateDebitMBL(M_Debit c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.debitId = Guid.NewGuid();
                _context.Debit.Add(c!);

                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Debit", "Shipment", c.debitId, c.debitno, c);
                return new BoolandMessReponse(true, "Create Debit Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Debit with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateCuoccont(M_CuocCont c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.id = Guid.NewGuid();

                _context.CuocCont.Add(c!);

                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Cuoc Cont", "Shipment", c.id, c.No, c);
                return new BoolandMessReponse(true, "Create Cuoc Cont Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Cuoc Cont with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteDebitMBL(M_Debit c)
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
                return new BoolandMessReponse(false, "Cannot Delete Debit with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteCuocCont(M_CuocCont c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.id == null || c?.id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.CuocCont.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Cuoc Cont with error code: " + ex.Message);
            }
        }
        public async Task<List<M_Debit>> GetListDebitHBL(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Debit.Where(x => x.hblid == id && x.continued == true).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Debit>();
            }
        }

        public async Task<List<M_Debit>> GetListDebitHBL_cus(Guid? id, Guid? cus_id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Debit.Where(x => x.hblid == id && x.customerid == cus_id && x.continued == true).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Debit>();
            }
        }
        public async Task<List<M_Debit>> GetListDebitHBL(Guid? id, bool isDuty)
        {
            _context.ChangeTracker.Clear();
            var query = $@"select c.* from debit c join CHARGE ch on c.itemid = ch.CHARGE_ID 
                        where CHARGE_CODE {(isDuty ? string.Empty : "not")} like '%Duty%' and c.hblid = '{id}' and c.continued = 1";
            var rs = await _context.Debit.FromSqlRaw(query).ToListAsync();
            return rs;
        }
        public async Task<List<M_Debit>> GetListDebitHBL(List<M_HBL> listhbl)
        {
            _context.ChangeTracker.Clear();
            var hblIds = listhbl.Select(x => x.hblID).ToList();
            var querydebit = "(";
            foreach (var item in listhbl)
                querydebit += $"'{item.hblID}',";
            if (listhbl.Any())
                querydebit = querydebit.TrimEnd(',') + ")";

            var rs = await _context.Debit.FromSqlRaw(@$"SELECT * FROM DEBIT WHERE hblid IN {querydebit}").ToListAsync();
            return rs.Where(x => !x.daIndebit.HasValue || !x.daIndebit!.Value).ToList(); // daIndebit false hoặc null
        }
        public async Task<List<M_Credit>> GetListCreditHBL(List<M_HBL> listhbl)
        {
            _context.ChangeTracker.Clear();
            var hblIds = listhbl.Select(x => x.hblID).ToList();
            var querydebit = "(";
            foreach (var item in listhbl)
                querydebit += $"'{item.hblID}',";
            if (listhbl.Any())
                querydebit = querydebit.TrimEnd(',') + ")";

            var rs = await _context.Credit.FromSqlRaw(@$"SELECT * FROM Credit WHERE hblid IN {querydebit}").ToListAsync();
            return rs;
        }

        public async Task<List<M_Credit>> GetListCreditHBL_where_maincode(List<M_HBL> listhbl)
        {


            var hblList = listhbl.Select(x => x.hblID).ToList();
            var hblInClause = string.Join(",", hblList.Select(x => $"'{x}'"));


            var sql = $@"
                SELECT c.* 
                FROM Credit c 
                LEFT JOIN Customer cus ON c.customerid = cus.Customer_ID 
                WHERE c.hblid IN ({hblInClause}) AND c.dntt = 0";


            var rs = await _context.Credit
                .FromSqlRaw(sql)
                .AsNoTracking()
                .ToListAsync();
            return rs;

        }
        public async Task<List<M_Credit>> GetListCreditHBL_where_Credit_DNTU_False(List<M_HBL> listhbl)
        {


            var hblList = listhbl.Select(x => x.hblID).ToList();
            var hblInClause = string.Join(",", hblList.Select(x => $"'{x}'"));


            var sql = $@"
                SELECT c.* 
                FROM Credit c 
                LEFT JOIN Customer cus ON c.customerid = cus.Customer_ID 
                WHERE c.hblid IN ({hblInClause}) AND c.DNTU = 0";


            var rs = await _context.Credit
                .FromSqlRaw(sql)
                .AsNoTracking()
                .ToListAsync();
            return rs;

        }

        public async Task<List<M_HBL>> GetListHBL_ByCus(Guid? cusid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.HBL.Where(x => x.CustomerID == cusid).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<M_Debit>> GetListDebitALL()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Debit.Where(x => x.continued == true).ToListAsync();
            return rs;
        }
        public async Task<List<M_Debit>> GetListDebitALL_Item_Comm_Duty()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Debit.Where(x => x.continued == true).ToListAsync();
            return rs;
        }
        public async Task<List<M_Credit>> GetListCreditALL()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Credit.Where(x => x.continued == true).ToListAsync();
            return rs;
        }

        public async Task<List<M_Credit>> GetCredit_ByID(Guid id)
        {
            _context.ChangeTracker.Clear();
            return await _context.Credit
                                 .Where(x => x.continued == true && x.creditid == id)
                                 .ToListAsync();
        }

        public async Task<List<M_Debit>> GetListDebitHBLs(List<M_HBL> listdata)
        {
            // Use a fresh context instance to avoid parallel operations on the scoped _context
            // and rely on LINQ Contains instead of building raw SQL strings (avoids SQL injection & reader concurrency issues)
            if (listdata == null || listdata.Count == 0)
                return new List<M_Debit>();

            var hblIds = listdata.Select(x => x.hblID).Where(id => id != Guid.Empty).Distinct().ToList();
            if (hblIds.Count == 0)
                return new List<M_Debit>();

            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var parameters = new List<object>();
            var inClauseParts = new List<string>();
            for (int i = 0; i < hblIds.Count; i++)
            {
                var paramName = $"@p{i}";
                inClauseParts.Add(paramName);
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter(paramName, hblIds[i]));
            }
            var inClause = string.Join(",", inClauseParts);
            var sql = $"SELECT * FROM Debit WHERE continued = 1 AND hblid IN ({inClause})";
            return await ctx.Debit.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();
        }
        public async Task<List<M_Credit>> GetListCreditHBLs(List<M_HBL> listdata)
        {
            if (listdata == null || listdata.Count == 0)
                return new List<M_Credit>();

            var hblIds = listdata.Select(x => x.hblID).Where(id => id != Guid.Empty).Distinct().ToList();
            if (hblIds.Count == 0)
                return new List<M_Credit>();

            // Build parameterized IN clause to avoid direct Contains usage per coding guideline
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            var parameters = new List<object>();
            var inClauseParts = new List<string>();
            for (int i = 0; i < hblIds.Count; i++)
            {
                var paramName = $"@p{i}";
                inClauseParts.Add(paramName);
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter(paramName, hblIds[i]));
            }
            var inClause = string.Join(",", inClauseParts);
            var sql = $"SELECT * FROM Credit WHERE continued = 1 AND hblid IN ({inClause})";
            return await ctx.Credit.FromSqlRaw(sql, parameters.ToArray()).AsNoTracking().ToListAsync();
        }
        public async Task<List<M_HoaDonDauRa>> GetListHoaDonDauRaALL()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.HoaDonDauRa.Where(x => x.continued == true).ToListAsync();
            return rs;

        }
        public async Task<List<M_HoaDonDauRa>> GetListHoaDonDauRa_HDnoibo()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.HoaDonDauRa.Where(x => x.continued == true && !string.IsNullOrEmpty(x.sohoadonNoibo)).ToListAsync();
            return rs;

        }


        public async Task<List<M_HoaDonDauVao>> GetListHoaDonDauVaoALL()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.HoaDonDauVao.Where(x => x.continued == true).ToListAsync();
            return rs;

        }
        public async Task<List<M_HoaDonDauRa>> GetListHoaDonDauRaHBL(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.HoaDonDauRa.Where(x => x.hblid == id && x.continued == true).ToListAsync();
            return rs;

        }
        public async Task<List<M_HoaDonDauRa>> GetListHoaDonDauRaMBL(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.HoaDonDauRa.Where(x => x.mblid == id && x.continued == true).ToListAsync();
            return rs;
        }
        public async Task<BoolandMessReponse> UpdateHoaDonDauRaMBL(M_HoaDonDauRa c, M_HoaDonDauRa c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now.ToString();
                _context.HoaDonDauRa.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Hoa Don Dau Ra", "Shipment", c.hoadondauraid, "", new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update HoaDonDauRa Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update HoaDonDauRa with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateHoaDonDauRaMBL_approve(M_HoaDonDauRa c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now.ToString();
                _context.HoaDonDauRa.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                //await HistoryLogService.LogAsync(usr, "Update Hoa Don Dau Ra Approve", "Shipment", c.hoadondauraid, "", c);
                return new BoolandMessReponse(true, "Update HoaDonDauRa Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update HoaDonDauRa with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> UpdateSoHDHoaDonDauRa(List<M_HoaDonDauRa> c, string? invoiceNo)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.ForEach(x => x.dateupdate = DateTime.Now.ToString("dd/MMM/yyyy"));
                var listsohd = c.Select(x => x.sohoadonNoibo).Distinct();
                var sohds = string.Join(",", listsohd.Select(id => $"'{id}'"));
                var debitdata = await _context.Debit.FromSqlRaw($"SELECT * FROM Debit where sohoadondaura in ({sohds})").ToListAsync();
                // update so hd noi bo debit
                debitdata.ForEach(x => x.sohoadondaura = invoiceNo);
                // update sohd noi bo
                c.ForEach(x => x.sohoadonNoibo = invoiceNo);
                _context.HoaDonDauRa.UpdateRange(c);
                _context.Debit.UpdateRange(debitdata);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Invoice No. Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Update Invoice No. Fail with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateHoaDonDauRaMBL(M_HoaDonDauRa c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.hoadondauraid = Guid.NewGuid();
                _context.HoaDonDauRa.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Hoa Don Dau Ra", "Shipment", c.hoadondauraid, "", c);
                return new BoolandMessReponse(true, "Create HoaDonDauRa Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add HoaDonDauRa with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateHoaDonDauRaMBL(List<M_HoaDonDauRa> c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.ForEach(x => x.hoadondauraid = Guid.NewGuid());
                _context.HoaDonDauRa.AddRange(c!);
                await _context.SaveChangesAsync();

                return new BoolandMessReponse(true, "Create HoaDonDauRa Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add HoaDonDauRa with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteHoaDonDauRaMBL(M_HoaDonDauRa c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.hoadondauraid == null || c?.hoadondauraid == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.HoaDonDauRa.Remove(c!);
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete Hoa Don Dau Ra", "Shipment", c.hoadondauraid, "", new { OldData = c });
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete HoaDonDauRa with error code: " + ex.Message);
            }
        }
        public async Task<List<M_HoaDonDauVao>> GetListHoaDonDauVaoHBL(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.HoaDonDauVao.Where(x => x.hblid == id && x.continued == true).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_HoaDonDauVao>();
            }
        }

        public async Task<List<M_HoaDonDauVao>> GetListHoaDonDauVaoMBL(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.HoaDonDauVao.Where(x => x.mblid == id && x.continued == true).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_HoaDonDauVao>();
            }
        }
        public async Task<BoolandMessReponse> UpdateHoaDonDauVaoMBL(M_HoaDonDauVao c, M_HoaDonDauVao c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now.ToString();
                _context.HoaDonDauVao.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Hoa Don Dau Vao MBL", "Shipment", c.hoadondauvaoid, "", new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update HoaDonDauVao Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update HoaDonDauVao with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateHoaDonDauVaoMBL_approve(M_HoaDonDauVao c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now.ToString();
                _context.HoaDonDauVao.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                //await HistoryLogService.LogAsync(usr, "Update Hoa Don Dau Vao MBL Appove", "Shipment", c.hoadondauvaoid, "",c);
                return new BoolandMessReponse(true, "Update HoaDonDauVao Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update HoaDonDauVao with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateHoaDonDauVaoMBL(M_HoaDonDauVao c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.hoadondauvaoid = Guid.NewGuid();
                _context.HoaDonDauVao.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Hoa Don Dau Vao MBL", "Shipment", c.hoadondauvaoid, "", c);
                return new BoolandMessReponse(true, "Create HoaDonDauVao Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add HoaDonDauVao with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteHoaDonDauVaoMBL(M_HoaDonDauVao c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.hoadondauvaoid == null || c?.hoadondauvaoid == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.HoaDonDauVao.Remove(c!);
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete Hoa Don Dau Vao MBL", "Shipment", c.hoadondauvaoid, "", new { OldData = c });
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete HoaDonDauVao with error code: " + ex.Message);
            }
        }
        public async Task<List<M_CongNoHoaDonDauRa>> GetListCongNoHoaDonDauRaMBL(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.CongNoHoaDonDauRa.Where(x => x.hoadondauraid == id && x.continued == true).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_CongNoHoaDonDauRa>();
            }
        }
        public async Task<List<M_CongNoHoaDonDauRa>> GetListCongNoHoaDonDauRaALL()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.CongNoHoaDonDauRa.Where(x => x.continued == true).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_CongNoHoaDonDauRa>();
            }
        }
        public async Task<BoolandMessReponse> UpdateCongNoHoaDonDauRaMBL(M_CongNoHoaDonDauRa c, M_CongNoHoaDonDauRa c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now;
                _context.CongNoHoaDonDauRa.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;

                await HistoryLogService.LogAsync(usr, "Update Cong No Hoa Don Dau Ra MBL", "Shipment", c.congnoHoadonDauraid, "", new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update CongNoHoaDonDauRa Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update CongNoHoaDonDauRa with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateCongNoHoaDonDauRaMBL_Approve(M_CongNoHoaDonDauRa c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now;
                _context.CongNoHoaDonDauRa.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;

                //await HistoryLogService.LogAsync(usr, "Update Cong No Hoa Don Dau Ra MBL Approve", "Shipment", c.congnoHoadonDauraid, "", c);
                return new BoolandMessReponse(true, "Update CongNoHoaDonDauRa Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update CongNoHoaDonDauRa with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateCongNoHoaDonDauRaMBL(M_CongNoHoaDonDauRa c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.congnoHoadonDauraid = Guid.NewGuid();
                _context.CongNoHoaDonDauRa.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;

                await HistoryLogService.LogAsync(usr, "ADD Cong No Hoa Don Dau Ra MBL", "Shipment", c.congnoHoadonDauraid, "", c);
                return new BoolandMessReponse(true, "Create CongNoHoaDonDauRa Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add CongNoHoaDonDauRa with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteCongNoHoaDonDauRaMBL(M_CongNoHoaDonDauRa c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.congnoHoadonDauraid == null || c?.congnoHoadonDauraid == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.CongNoHoaDonDauRa.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete CongNoHoaDonDauRa with error code: " + ex.Message);
            }
        }

        public async Task<List<M_CongNoHoaDonDauVao>> GetListCongNoHoaDonDauVaoMBL(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.CongNoHoaDonDauVao.Where(x => x.hoadondauvaoid == id && x.continued == true).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_CongNoHoaDonDauVao>();
            }
        }
        public async Task<List<M_CongNoHoaDonDauVao>> GetListCongNoHoaDonDauVaoALL()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.CongNoHoaDonDauVao.Where(x => x.continued == true).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_CongNoHoaDonDauVao>();
            }
        }
        public async Task<BoolandMessReponse> UpdateCongNoHoaDonDauVaoMBL(M_CongNoHoaDonDauVao c, M_CongNoHoaDonDauVao c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now;
                _context.CongNoHoaDonDauVao.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Cong No Hoa Don Dau Vao MBL", "Shipment", c.congnoHoadonDauvaoid, "", new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update CongNoHoaDonDauVao Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update CongNoHoaDonDauVao with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateCongNoHoaDonDauVaoMBL_approve(M_CongNoHoaDonDauVao c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.dateupdate = DateTime.Now;
                _context.CongNoHoaDonDauVao.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                /*wait HistoryLogService.LogAsync(usr, "Update Cong No Hoa Don Dau Vao MBL Aprrove", "Shipment", c.congnoHoadonDauvaoid, "", c);*/
                return new BoolandMessReponse(true, "Update CongNoHoaDonDauVao Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update CongNoHoaDonDauVao with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateCongNoHoaDonDauVaoMBL(M_CongNoHoaDonDauVao c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.congnoHoadonDauvaoid = Guid.NewGuid();
                _context.CongNoHoaDonDauVao.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;

                await HistoryLogService.LogAsync(usr, "ADD Cong No Hoa Don Dau Vao MBL", "Shipment", c.congnoHoadonDauvaoid, "", c);
                return new BoolandMessReponse(true, "Create CongNoHoaDonDauVao Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add CongNoHoaDonDauVao with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteCongNoHoaDonDauVaoMBL(M_CongNoHoaDonDauVao c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.congnoHoadonDauvaoid == null || c?.congnoHoadonDauvaoid == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.CongNoHoaDonDauVao.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete CongNoHoaDonDauVao with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DuplicateDetailList(Guid? oldJobID, Guid? newJobID, CopyOptions CopyOPs)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ChangeTracker.Clear();
                var usr = asv.GetUserDetail();

                //mbl
                var listmblold = await GetListMBL(oldJobID);
                listmblold.ForEach(x => x.Jobid = newJobID);
                listmblold.ForEach(x => x.Mbl += "(Duplicate)");

                var today = DateTime.Now.ToString("dd-MMM-yyyy", new CultureInfo("en-US"));
                //debit, credit, conts
                List<M_Debit> _Fdebits = new();
                List<M_Credit> _Fcredits = new();
                List<M_Container> _Fconts = new();
                List<M_HBL> _FHBLs = new();
                foreach (var mbl in listmblold)
                {
                    var NewIDmbl = Guid.NewGuid();
                    //debit
                    if (CopyOPs.CopyDebit)
                    {
                        //mbl
                        var debits = await GetListDebitMBL(mbl.MblID);
                        debits.ForEach(x => x.debitId = Guid.NewGuid());
                        debits.ForEach(x => x.mblid = NewIDmbl);
                        foreach (var group in debits.GroupBy(x => x.debitno))
                        {
                            var dbno = await GetDebitNo(mbl.MblID);
                            foreach (var db in group)
                                db.debitno = dbno;
                        }
                        debits.ForEach(x => x.userupdate = usr.Usr);
                        debits.ForEach(x => x.dateupdate = today);
                        debits.ForEach(x => x.sohoadondaura = string.Empty);
                        _Fdebits.AddRange(debits);
                    }
                    //credit
                    if (CopyOPs.CopyCredit)
                    {
                        //mbl
                        var credits = await GetListCreditMBL(mbl.MblID);
                        credits.ForEach(x => x.mblid = NewIDmbl);
                        credits.ForEach(x => x.creditid = Guid.NewGuid());
                        credits.ForEach(x => x.userupdate = usr.Usr);
                        credits.ForEach(x => x.dateupdate = today);
                        _Fcredits.AddRange(credits);

                    }
                    if (CopyOPs.CopyContainer)
                    {
                        //mbl
                        var conts = await GetListContainerMBLAsync(mbl.MblID);
                        conts.ForEach(x => x.CTN_ID = Guid.NewGuid());
                        conts.ForEach(x => x.mblid = NewIDmbl);
                        conts.ForEach(x => x.UPDATETIME = DateTime.Now);
                        conts.ForEach(x => x.USERID = usr.Usr);
                        _Fconts.AddRange(conts);
                    }

                    //hbl
                    var listhblold = await GetListHBL(mbl.MblID);
                    foreach (var hbl in listhblold)
                    {
                        var NewIDhbl = Guid.NewGuid();
                        hbl.hbl = await UpdateHBLNo(mbl, usr.Usr);
                        //debit
                        if (CopyOPs.CopyDebit)
                        {

                            var hbldebits = await GetListDebitHBL(hbl.hblID);
                            hbldebits.ForEach(x => x.debitId = Guid.NewGuid());
                            hbldebits.ForEach(x => x.hblid = NewIDhbl);
                            foreach (var group in hbldebits.GroupBy(x => x.debitno))
                            {
                                var dbno = await GetDebitNo(mbl.MblID);
                                foreach (var db in group)
                                    db.debitno = dbno;
                            }
                            hbldebits.ForEach(x => x.userupdate = usr.Usr);
                            hbldebits.ForEach(x => x.dateupdate = today);
                            hbldebits.ForEach(x => x.sohoadondaura = string.Empty);
                            _Fdebits.AddRange(hbldebits);
                        }
                        //hbl
                        if (CopyOPs.CopyCredit)
                        {
                            var hblcredits = await GetListCreditHBL(hbl.hblID);
                            hblcredits.ForEach(x => x.hblid = NewIDhbl);
                            hblcredits.ForEach(x => x.creditid = Guid.NewGuid());
                            hblcredits.ForEach(x => x.userupdate = usr.Usr);
                            hblcredits.ForEach(x => x.dateupdate = today);
                            _Fcredits.AddRange(hblcredits);
                        }
                        //cont
                        if (CopyOPs.CopyContainer)
                        {
                            var hblconts = GetListContainerHBL(hbl.hblID);
                            hblconts.ForEach(x => x.CTN_ID = Guid.NewGuid());
                            hblconts.ForEach(x => x.hblid = NewIDhbl);
                            hblconts.ForEach(x => x.UPDATETIME = DateTime.Now);
                            hblconts.ForEach(x => x.USERID = usr.Usr);
                            _Fconts.AddRange(hblconts);
                        }
                        hbl.hblID = NewIDhbl;
                        hbl.mblid = NewIDmbl;
                    }
                    _FHBLs.AddRange(listhblold);
                    mbl.MblID = NewIDmbl;
                }

                //truck and customs
                var TC = await _context.HBL.Where(x => x.Jobid == oldJobID).ToListAsync();
                foreach (var tcitem in TC)
                {
                    tcitem.Jobid = newJobID;
                    var NewTCID = Guid.NewGuid();
                    if (CopyOPs.CopyDebit)
                    {
                        //debit 
                        var tcdebits = await GetListDebitHBL(tcitem.hblID);
                        tcdebits.ForEach(x => x.debitId = Guid.NewGuid());
                        tcdebits.ForEach(x => x.hblid = NewTCID);
                        foreach (var group in tcdebits.GroupBy(x => x.debitno))
                        {
                            var dbno = await GetDebitNo(tcitem.mblid);
                            foreach (var db in group)
                                db.debitno = dbno;
                        }
                        tcdebits.ForEach(x => x.userupdate = usr.Usr);
                        tcdebits.ForEach(x => x.dateupdate = today);
                        tcdebits.ForEach(x => x.sohoadondaura = string.Empty);
                        _Fdebits.AddRange(tcdebits);
                    }
                    if (CopyOPs.CopyCredit)
                    {
                        //credit
                        var tccredits = await GetListCreditHBL(tcitem.hblID);
                        tccredits.ForEach(x => x.hblid = NewTCID);
                        tccredits.ForEach(x => x.creditid = Guid.NewGuid());
                        tccredits.ForEach(x => x.userupdate = usr.Usr);
                        tccredits.ForEach(x => x.dateupdate = today);
                        _Fcredits.AddRange(tccredits);
                    }
                    if (CopyOPs.CopyContainer)
                    {
                        //cont
                        var tcconts = await GetListContainerMBLAsync(tcitem.hblID);
                        tcconts.ForEach(x => x.CTN_ID = Guid.NewGuid());
                        tcconts.ForEach(x => x.hblid = NewTCID);
                        tcconts.ForEach(x => x.UPDATETIME = DateTime.Now);
                        tcconts.ForEach(x => x.USERID = usr.Usr);
                        _Fconts.AddRange(tcconts);
                    }
                    tcitem.hblID = NewTCID;
                }
                var _en = new CultureInfo("en-US");
                listmblold.ForEach(x => x.DateUpdate = today);
                listmblold.ForEach(x => x.Approve = x.dachicredit = x.dathuchidaily = x.dachicredit = x.dathudebit = false);
                _FHBLs.ForEach(x => x.dateupdate = today);
                _FHBLs.ForEach(x => x.approve = x.dachicredit = x.dathuchidaily = x.dachicredit = x.dathudebit = false);
                TC.ForEach(x => x.dateupdate = today);
                TC.ForEach(x => x.approve = x.dachicredit = x.dathuchidaily = x.dachicredit = x.dathudebit = false);

                _Fcredits.ForEach(x => x.approve = false);
                _Fdebits.ForEach(x => x.approve = x.daIndebit = x.daXuatHoadon = false);
                _Fconts.ForEach(x => x.APPROVE = false);

                await Task.Delay(2500); // chờ các refno

                await _context.MBL.AddRangeAsync(listmblold); // lưu mbl
                await Task.Delay(50);
                await _context.HBL.AddRangeAsync(TC);
                await Task.Delay(50);
                await _context.Credit.AddRangeAsync(_Fcredits);
                await Task.Delay(50);
                await _context.Debit.AddRangeAsync(_Fdebits);
                await Task.Delay(50);
                await _context.Container.AddRangeAsync(_Fconts);
                await Task.Delay(50);
                await _context.HBL.AddRangeAsync(_FHBLs);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BoolandMessReponse(true, "Create MBLs Success");

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BoolandMessReponse(false, "Cannot Add MBLs with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DuplicateHBLDetailList(M_HBL oldHBL, Guid newHBLID, CopyOptions CopyOPs)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ChangeTracker.Clear();
                var usr = asv.GetUserDetail();



                var today = DateTime.Now.ToString("dd-MMM-yyyy", new CultureInfo("en-US"));
                //debit, credit, conts
                List<M_Debit> _Fdebits = new();
                List<M_Credit> _Fcredits = new();
                List<M_Container> _Fconts = new();
                if (CopyOPs.CopyDebit)
                {
                    //debit 
                    var tcdebits = await GetListDebitHBL(oldHBL.hblID);
                    await Task.Delay(500); // chờ các refno
                    tcdebits.ForEach(x => x.debitId = Guid.NewGuid());
                    tcdebits.ForEach(x => x.hblid = newHBLID);
                    foreach (var group in tcdebits.GroupBy(x => x.debitno))
                    {
                        var dbno = await GetDebitNo(oldHBL.mblid);
                        foreach (var db in group)
                            db.debitno = dbno;
                    }
                    await Task.Delay(1000); // chờ các refno
                    tcdebits.ForEach(x => x.userupdate = usr.Usr);
                    tcdebits.ForEach(x => x.dateupdate = today);
                    tcdebits.ForEach(x => x.sohoadondaura = string.Empty);
                    _Fdebits.AddRange(tcdebits);
                }
                if (CopyOPs.CopyCredit)
                {
                    //credit
                    var tccredits = await GetListCreditHBL(oldHBL.hblID);
                    await Task.Delay(500); // chờ các refno
                    tccredits.ForEach(x => x.hblid = newHBLID);
                    tccredits.ForEach(x => x.creditid = Guid.NewGuid());
                    tccredits.ForEach(x => x.userupdate = usr.Usr);
                    tccredits.ForEach(x => x.dateupdate = today);
                    _Fcredits.AddRange(tccredits);
                }
                if (CopyOPs.CopyContainer)
                {
                    //cont
                    var tcconts = GetListContainerHBL(oldHBL.hblID);
                    await Task.Delay(500); // chờ các refno
                    tcconts.ForEach(x => x.CTN_ID = Guid.NewGuid());
                    tcconts.ForEach(x => x.hblid = newHBLID);
                    tcconts.ForEach(x => x.UPDATETIME = DateTime.Now);
                    tcconts.ForEach(x => x.USERID = usr.Usr);
                    _Fconts.AddRange(tcconts);
                }
                var _en = new CultureInfo("en-US");
                _Fcredits.ForEach(x => x.approve = x.editable = false);
                _Fdebits.ForEach(x => x.approve = x.editable = x.daIndebit = x.daXuatHoadon = false);
                _Fconts.ForEach(x => x.APPROVE = x.EDITABLE = false);

                await _context.Credit.AddRangeAsync(_Fcredits);
                await Task.Delay(50);
                await _context.Debit.AddRangeAsync(_Fdebits);
                await Task.Delay(50);
                await _context.Container.AddRangeAsync(_Fconts);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new BoolandMessReponse(true, "Create HBLs Success");

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BoolandMessReponse(false, "Cannot Add HBLs with error code: " + ex.Message);
            }
        }
        /// <param name="showPreCarriage">true = show vessel-voy in "Pre-Carriage"; false = show in "Vessel & Voy No."</param>
        /// <param name="attach">true = export attached page template</param>
        /// <param name="isOriginal">true = Original (không logo); false = Draft (có logo)</param>
        public async Task<BoolandMessReponse> ExportBillSea(Guid id, bool showPreCarriage = true, bool attach = false, bool isOriginal = false, Guid? layoutFormId = null)
        {
            try
            {
                var report = new StiReport();
                byte[] templateBytes;
                if (attach)
                {
                    if (layoutFormId is Guid attachFormId && attachFormId != Guid.Empty)
                        templateBytes = await billSeaLayoutFormService.GetAttachFormBytesAsync(attachFormId);
                    else
                        templateBytes = await billSeaLayoutFormService.GetDefaultTemplateBytesAsync(BillSeaReportTemplateNames.Attach);
                }
                else if (layoutFormId is Guid formId && formId != Guid.Empty)
                {
                    templateBytes = await billSeaLayoutFormService.GetFormBytesAsync(formId);
                }
                else
                {
                    templateBytes = await billSeaLayoutFormService.GetDefaultTemplateBytesAsync(BillSeaReportTemplateNames.Main);
                }

                var connectionString = ResolveReportConnectionString();
                var companyLogo = await GetCompanyLogoAsync();
                var companyBillSeaForm = await GetCompanyBillSeaFormAsync();

                StiBlazorHelper.Initialize(JSRuntime);

                report = StiReport.CreateNewReport();
                report.Load(new MemoryStream(templateBytes));
                ApplyReportConnectionString(report, connectionString);
                ApplyCompanyLogoToReport(report, companyLogo, showLogo: !isOriginal);
                ApplyBillSeaFormToReport(report, companyBillSeaForm);
                report.Dictionary.Variables["ID"].Value = id.ToString();
                report.Dictionary.Variables["chk_show_pre_Carr"].Value = showPreCarriage ? "true" : "false";
                // Original = không logo, Draft = có logo
                if (report.Dictionary.Variables["IsOriginal"] != null)
                    report.Dictionary.Variables["IsOriginal"].Value = isOriginal ? "true" : "false";
                report.Render();
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
        /// <param name="isOriginal">true = Original (không logo); false = Draft (có logo)</param>
        public async Task<BoolandMessReponse> ExportBilAir(Guid id, bool isOriginal = false)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "BillAir.mrt");
                var connectionString = ResolveReportConnectionString();
                var companyLogo = await GetCompanyLogoAsync();

                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                ApplyReportConnectionString(report, connectionString);
                report.Dictionary.Variables["ID"].Value = id.ToString();
                if (report.Dictionary.Variables["IsOriginal"] != null)
                    report.Dictionary.Variables["IsOriginal"].Value = isOriginal ? "true" : "false";
                ApplyCompanyLogoToReport(report, companyLogo, showLogo: !isOriginal);
                report.Render();
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
        public async Task<BoolandMessReponse> ExportArrivalAir(M_HBL detail)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "BillArrivalNotice_Air.mrt");
                var companyLogo = await GetCompanyLogoAsync();

                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                ApplyArrivalReportSetup(report, companyLogo);
                report.Dictionary.Variables["hblid"].Value = detail.hblID.ToString();

                var flightdate = detail.Air_FlightDate1!.Split('/').ToList();
                var flightcode = flightdate.Count == 0 ? "" : flightdate.FirstOrDefault();
                report.Dictionary.Variables["flightNo"].Value = flightcode!.ToString();

                var mawb = await GetMBL_byHBLid(detail.mblid);
                report.Dictionary.Variables["MAWB"].Value = mawb.Mbl;

                var listdebit = await GetListDebitHBL(detail.hblID);
                listdebit = listdebit.Where(x => x.InArrival == true).ToList();
                var querydebit = "WHERE debitId IN (";
                foreach (var item in listdebit)
                    querydebit += $"'{item.debitId}',";
                if (listdebit.Any())
                    querydebit = querydebit.TrimEnd(',') + ")";
                else
                    querydebit = "WHERE 1=0";
                report.Dictionary.Variables["querydebit"].Value = querydebit.ToString();

                var ngaythangnam = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";
                report.Dictionary.Variables["ngaythangnam"].Value = ngaythangnam.ToString();
                report.Render();
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
        public async Task<BoolandMessReponse> ExportArrival(M_HBL detail)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "BillArrivalNoticeNVOCC.mrt");
                var companyLogo = await GetCompanyLogoAsync();

                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                ApplyArrivalReportSetup(report, companyLogo);
                report.Dictionary.Variables["hblid"].Value = detail.hblID.ToString();
                var mawb = await GetMBL_byHBLid(detail.mblid);
                report.Dictionary.Variables["MAWB"].Value = mawb.Mbl;

                var listdebit = await GetListDebitHBL(detail.hblID);
                listdebit = listdebit.Where(x => x.InArrival == true).ToList();
                var querydebit = "WHERE debitId IN (";
                foreach (var item in listdebit)
                    querydebit += $"'{item.debitId}',";
                if (listdebit.Any())
                    querydebit = querydebit.TrimEnd(',') + ")";
                else
                    querydebit = "WHERE 1=0";
                report.Dictionary.Variables["querydebit"].Value = querydebit.ToString();

                var ngaythangnam = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";
                report.Dictionary.Variables["ngaythangnam"].Value = ngaythangnam.ToString();
                report.Render();
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

        /// <summary>
        /// Generates Arrival Notice PDF bytes for Sea (SI/SE). Used for QR code public link - no JSRuntime.
        /// </summary>
        public async Task<byte[]?> GetArrivalNoticePdfBytesAsync(M_HBL detail)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "BillArrivalNoticeNVOCC.mrt");
                var companyLogo = await GetCompanyLogoAsync();

                report = StiReport.CreateNewReport();
                report.Load(rpt);
                ApplyArrivalReportSetup(report, companyLogo);
                report.Dictionary.Variables["hblid"].Value = detail.hblID.ToString();
                var mawb = await GetMBL_byHBLid(detail.mblid);
                report.Dictionary.Variables["MAWB"].Value = mawb.Mbl;

                var listdebit = await GetListDebitHBL(detail.hblID);
                listdebit = listdebit.Where(x => x.InArrival == true).ToList();
                var querydebit = "WHERE debitId IN (";
                foreach (var item in listdebit)
                    querydebit += $"'{item.debitId}',";
                if (listdebit.Any())
                    querydebit = querydebit.TrimEnd(',') + ")";
                else
                    querydebit = "WHERE 1=0";
                report.Dictionary.Variables["querydebit"].Value = querydebit;

                var ngaythangnam = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";
                report.Dictionary.Variables["ngaythangnam"].Value = ngaythangnam;
                report.Render();
                using var ms = new MemoryStream();
                report.ExportDocument(StiExportFormat.Pdf, ms);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Generates Arrival Notice PDF bytes for Air (AI/AE). Used for QR code public link - no JSRuntime.
        /// </summary>
        public async Task<byte[]?> GetArrivalNoticeAirPdfBytesAsync(M_HBL detail)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "BillArrivalNotice_Air.mrt");
                var companyLogo = await GetCompanyLogoAsync();

                report = StiReport.CreateNewReport();
                report.Load(rpt);
                ApplyArrivalReportSetup(report, companyLogo);
                report.Dictionary.Variables["hblid"].Value = detail.hblID.ToString();

                var flightdate = (detail.Air_FlightDate1 ?? "").Split('/').ToList();
                var flightcode = flightdate.Count == 0 ? "" : flightdate.FirstOrDefault();
                report.Dictionary.Variables["flightNo"].Value = flightcode ?? "";

                var mawb = await GetMBL_byHBLid(detail.mblid);
                report.Dictionary.Variables["MAWB"].Value = mawb.Mbl;

                var listdebit = await GetListDebitHBL(detail.hblID);
                listdebit = listdebit.Where(x => x.InArrival == true).ToList();
                var querydebit = "WHERE debitId IN (";
                foreach (var item in listdebit)
                    querydebit += $"'{item.debitId}',";
                if (listdebit.Any())
                    querydebit = querydebit.TrimEnd(',') + ")";
                else
                    querydebit = "WHERE 1=0";
                report.Dictionary.Variables["querydebit"].Value = querydebit;

                var ngaythangnam = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";
                report.Dictionary.Variables["ngaythangnam"].Value = ngaythangnam;
                report.Render();
                using var ms = new MemoryStream();
                report.ExportDocument(StiExportFormat.Pdf, ms);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<BoolandMessReponse> ExportArrival_NVOCC(M_HBL detail)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "BillArrivalNoticeNVOCC.mrt");
                var companyLogo = await GetCompanyLogoAsync();

                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                ApplyArrivalReportSetup(report, companyLogo);
                report.Dictionary.Variables["hblid"].Value = detail.hblID.ToString();
                var mawb = await GetMBL_byHBLid(detail.mblid);
                report.Dictionary.Variables["MAWB"].Value = mawb.Mbl;

                var listdebit = await GetListDebitHBL(detail.hblID);
                listdebit = listdebit.Where(x => x.InArrival == true).ToList();
                var querydebit = "WHERE debitId IN (";
                foreach (var item in listdebit)
                    querydebit += $"'{item.debitId}',";
                if (listdebit.Any())
                    querydebit = querydebit.TrimEnd(',') + ")";
                else
                    querydebit = "WHERE 1=0";
                report.Dictionary.Variables["querydebit"].Value = querydebit.ToString();

                var ngaythangnam = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";
                report.Dictionary.Variables["ngaythangnam"].Value = ngaythangnam.ToString();
                report.Render();
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

        public async Task<BoolandMessReponse> ExportDO(M_HBL detail, string billType = "PASL")
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "BillDeliveryOrder.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                report.Dictionary.Variables["hblid"].Value = detail.hblID.ToString();
                report.Dictionary.Variables["BillType"].Value = billType;
                var mawb = await GetMBL_byHBLid(detail.mblid);
                report.Dictionary.Variables["MAWB"].Value = mawb.Mbl;

                var ngaythangnam = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";
                report.Dictionary.Variables["ngaythangnam"].Value = ngaythangnam.ToString();
                report.Render();
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
        public async Task<BoolandMessReponse> ExportDO_NVOCC(M_HBL detail)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "BillDeliveryOrderNVOCC.mrt");
                var companyLogo = await GetCompanyLogoAsync();
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                ApplyArrivalReportSetup(report, companyLogo);
                report.Dictionary.Variables["hblid"].Value = detail.hblID.ToString();
                var mawb = await GetMBL_byHBLid(detail.mblid);
                report.Dictionary.Variables["MAWB"].Value = mawb.Mbl;

                var ngaythangnam = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";
                report.Dictionary.Variables["ngaythangnam"].Value = ngaythangnam.ToString();
                report.Render();
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
        public async Task<Guid?> GetIDCus_englishname(string cusname)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var customer = await _context.Customer
                    .FirstOrDefaultAsync(x => x.EnglishName == cusname);

                return customer?.Customer_ID;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<BoolandMessReponse> ExportBBGN(M_HBL detail)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "BienBanGiaoNhan.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                report.Dictionary.Variables["hblid"].Value = detail.hblID.ToString();

                var shippername = string.Join("\n", detail.shipper);
                shippername = shippername.Trim();
                var shipper_id = await GetIDCus_englishname(shippername);
                if (shipper_id.HasValue)
                {
                    report.Dictionary.Variables["shipper_id"].Value = shipper_id.Value.ToString();
                }
                else
                {
                    report.Dictionary.Variables["shipper_id"].Value = shipper_id?.ToString() ?? "00000000-0000-0000-0000-000000000000";

                }

                var consigneename = string.Join("\n", detail.consignee);
                consigneename = consigneename.Trim();
                var consignee_id = await GetIDCus_englishname(consigneename);
                if (shipper_id.HasValue)
                {
                    report.Dictionary.Variables["consignee_id"].Value = consignee_id.Value.ToString();
                }
                else
                {
                    report.Dictionary.Variables["consignee_id"].Value = consignee_id?.ToString() ?? "00000000-0000-0000-0000-000000000000";

                }


                report.Render();
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

        public async Task<BoolandMessReponse> ExportDOAir(M_HBL detail)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "BillDeliveryOrder_Air.mrt");
                var companyLogo = await GetCompanyLogoAsync();
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                ApplyArrivalReportSetup(report, companyLogo);
                report.Dictionary.Variables["hblid"].Value = detail.hblID.ToString();
                var mawb = await GetMBL_byHBLid(detail.mblid);
                report.Dictionary.Variables["MAWB"].Value = mawb.Mbl;

                var flightdate = detail.Air_FlightDate1!.Split('/').ToList();
                var flightcode = flightdate.Count == 0 ? "" : flightdate.FirstOrDefault();
                report.Dictionary.Variables["flightNo"].Value = flightcode!.ToString();

                var ngaythangnam = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";
                report.Dictionary.Variables["ngaythangnam"].Value = ngaythangnam.ToString();
                report.Render();
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

        /// <summary>
        /// Generates DO (Delivery Order) PDF bytes for Sea (SI/SE). Used for QR code public link - no JSRuntime.
        /// </summary>
        public async Task<byte[]?> GetDOPdfBytesAsync(M_HBL detail)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "BillDeliveryOrderNVOCC.mrt");
                var companyLogo = await GetCompanyLogoAsync();
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                ApplyArrivalReportSetup(report, companyLogo);
                report.Dictionary.Variables["hblid"].Value = detail.hblID.ToString();
                var mawb = await GetMBL_byHBLid(detail.mblid);
                report.Dictionary.Variables["MAWB"].Value = mawb.Mbl;
                var ngaythangnam = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";
                report.Dictionary.Variables["ngaythangnam"].Value = ngaythangnam;
                report.Render();
                using var ms = new MemoryStream();
                report.ExportDocument(StiExportFormat.Pdf, ms);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Generates DO (Delivery Order) PDF bytes for Air (AI/AE). Used for QR code public link - no JSRuntime.
        /// </summary>
        public async Task<byte[]?> GetDOAirPdfBytesAsync(M_HBL detail)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "BillDeliveryOrder_Air.mrt");
                var companyLogo = await GetCompanyLogoAsync();
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                ApplyArrivalReportSetup(report, companyLogo);
                report.Dictionary.Variables["hblid"].Value = detail.hblID.ToString();
                var mawb = await GetMBL_byHBLid(detail.mblid);
                report.Dictionary.Variables["MAWB"].Value = mawb.Mbl;
                var flightdate = (detail.Air_FlightDate1 ?? "").Split('/').ToList();
                var flightcode = flightdate.Count == 0 ? "" : flightdate.FirstOrDefault();
                report.Dictionary.Variables["flightNo"].Value = flightcode ?? "";
                var ngaythangnam = $"Ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}";
                report.Dictionary.Variables["ngaythangnam"].Value = ngaythangnam;
                report.Render();
                using var ms = new MemoryStream();
                report.ExportDocument(StiExportFormat.Pdf, ms);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private static readonly string[] CompanyLogoResourceNames =
        [
            "logo_bill",
            "logo_nvocc",
            "PASLLogo",
            "VietStartLogo",
            "AMSSLogo",
            "HDSLogo",
            "CompanyLogo"
        ];

        private static readonly string[] CompanyLogoComponentNames =
        [
            "Text1",
            "CompanyLogo"
        ];

        private static System.Drawing.Image? CreateLogoImage(byte[]? logo)
        {
            if (logo is not { Length: > 0 })
                return null;

            using var ms = new MemoryStream(logo);
            using var temp = System.Drawing.Image.FromStream(ms);
            return new System.Drawing.Bitmap(temp);
        }

        private string ResolveReportConnectionString()
        {
            _tenantContext.EnsureInitializedFromHttpContext();
            if (!string.IsNullOrWhiteSpace(_tenantContext.ConnectionString))
                return _tenantContext.ConnectionString;

            var connectionString = _context.Database.GetConnectionString();
            if (!string.IsNullOrWhiteSpace(connectionString))
                return connectionString;

            return _context.Database.GetDbConnection().ConnectionString;
        }

        private async Task<byte[]?> GetCompanyLogoAsync()
        {
            _context.ChangeTracker.Clear();
            return await _context.CompanyInfomation
                .AsNoTracking()
                .Select(x => x.Logo)
                .FirstOrDefaultAsync();
        }

        private async Task<byte[]?> GetCompanyBillSeaFormAsync()
        {
            _context.ChangeTracker.Clear();
            return await _context.CompanyInfomation
                .AsNoTracking()
                .Select(x => x.FormBillSea)
                .FirstOrDefaultAsync();
        }

        private void ApplyArrivalReportSetup(StiReport report, byte[]? companyLogo)
        {
            var connectionString = ResolveReportConnectionString();
            ApplyReportConnectionString(report, connectionString);
            ApplyCompanyLogoToReport(report, companyLogo, showLogo: true);
        }

        private static void ApplyReportConnectionString(StiReport report, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            if (report.Dictionary.Variables.Contains("connectDB"))
                report.Dictionary.Variables["connectDB"].Value = connectionString;

            const string defaultDatabaseName = "MS SQL";
            if (report.Dictionary.Databases.Contains(defaultDatabaseName))
            {
                ((StiSqlDatabase)report.Dictionary.Databases[defaultDatabaseName]).ConnectionString = connectionString;
                return;
            }

            foreach (StiDatabase database in report.Dictionary.Databases)
            {
                if (database is StiSqlDatabase sqlDatabase)
                {
                    sqlDatabase.ConnectionString = connectionString;
                    break;
                }
            }
        }

        private static void ApplyCompanyLogoToReport(StiReport report, byte[]? logo, bool showLogo = true)
        {
            ApplyLogoVariable(report, logo, showLogo);

            if (!showLogo || logo is not { Length: > 0 })
                return;

            foreach (var resourceName in CompanyLogoResourceNames)
            {
                if (!report.Dictionary.Resources.Contains(resourceName))
                    continue;

                report.Dictionary.Resources[resourceName].Content = logo;
            }

            foreach (StiComponent component in report.GetComponents())
            {
                if (component is not StiImage image)
                    continue;

                var imageUrl = image.ImageURL?.ToString() ?? string.Empty;
                var imageExpression = image.Image?.ToString() ?? string.Empty;

                if (CompanyLogoComponentNames.Contains(image.Name, StringComparer.OrdinalIgnoreCase)
                    && !imageUrl.Contains("AMSSform", StringComparison.OrdinalIgnoreCase))
                {
                    image.Image = CreateLogoImage(logo);
                    continue;
                }

                if (imageExpression.Contains("{logo}", StringComparison.OrdinalIgnoreCase))
                {
                    image.Image = CreateLogoImage(logo);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(imageUrl))
                    continue;

                if (!CompanyLogoResourceNames.Any(name =>
                        imageUrl.Contains(name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                image.Image = CreateLogoImage(logo);
            }
        }

        private static void ApplyLogoVariable(StiReport report, byte[]? logo, bool showLogo)
        {
            if (!report.Dictionary.Variables.Contains("logo"))
                return;

            var logoVariable = report.Dictionary.Variables["logo"];
            if (!showLogo || logo is not { Length: > 0 })
            {
                logoVariable.ValueObject = null;
                return;
            }

            logoVariable.Type = typeof(System.Drawing.Image);
            logoVariable.ValueObject = CreateLogoImage(logo);
        }

        private static void ApplyBillSeaFormToReport(StiReport report, byte[]? formBillSea)
        {
            if (formBillSea is not { Length: > 0 })
                return;

            // Support both old and new report templates.
            if (report.Dictionary.Resources.Contains("AMSSform"))
                report.Dictionary.Resources["AMSSform"].Content = formBillSea;

            if (report.Dictionary.Resources.Contains("logo_bill"))
                report.Dictionary.Resources["logo_bill"].Content = formBillSea;
        }

        private static void ApplyCompanyBranchVariable(StiReport report, string? branches)
        {
            if (string.IsNullOrWhiteSpace(branches))
            {
                return;
            }

            if (report.Dictionary.Variables.Contains("Branches"))
            {
                report.Dictionary.Variables["Branches"].Value = branches;
            }
        }

        public async Task<BoolandMessReponse> ExportDebitSI(M_Debit detail, string cur_type, string? billType = null, string? branches = null)
        {
            try
            {
                var report = new StiReport();
                string rpt;
                if (cur_type == "USD")
                {
                     rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportDebitNote_SeaImport_USD.mrt");
                }
                else 
                {
                    rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportDebitNote_SeaImport_VND.mrt");

                }
        
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                var connectionString = ResolveReportConnectionString();
                var companyLogo = await GetCompanyLogoAsync();

                report.Load(rpt);
                report.Culture = "en-US";
                ApplyReportConnectionString(report, connectionString);
                ApplyCompanyLogoToReport(report, companyLogo, showLogo: true);
                var hblinfo = await GetHBL_byHBLid(detail.hblid); // lay ra say volume
                var fcl = await GetFLCByMBLID(hblinfo.mblid); //

                var list_debit = await GetListDebit_Debitno(detail.debitno);

                double? total_payment = 0;
                double? total_amount_notvat = 0;

                double? total_amount_Tax = 0;
                foreach (var item_debit in list_debit)
                {
                    if (cur_type == "VND")
                    {
                        if (item_debit.tiente == "VND")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue * item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) * item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) * item_debit.tigiadebit;
                        }

                    }
                    else
                    {
                        if (item_debit.tiente == "USD")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue / item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) / item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) / item_debit.tigiadebit;

                        }
                    }
                }

                report.Dictionary.Variables["Total_shipment"].Value = (total_payment ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_notvat"].Value = (total_amount_notvat ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_Tax"].Value = (total_amount_Tax ?? 0).ToString("#,##0.##");

                if (fcl == "F")
                {
                    report.Dictionary.Variables["volume"].Value = hblinfo.say;
                }
                else
                {
                    report.Dictionary.Variables["volume"].Value = hblinfo.NoOfPackages + " / " + hblinfo.gross + " / " + " / " + hblinfo.cbm;
                }
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                report.Dictionary.Variables["eta"].Value = hblinfo.ETA.HasValue ? hblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                report.Dictionary.Variables["etd"].Value = hblinfo.ETD.HasValue ? hblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                total_payment = Math.Round(total_payment ?? 0, 2);
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(total_payment, cur_type);
                report.Dictionary.Variables["Cur_type"].Value = cur_type;
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy");
                var mblinfo = await GetMBL_byHBLid(hblinfo.mblid);
                var infojob = await GetJob_byid(mblinfo.Jobid);
                report.Dictionary.Variables["Refno"].Value = infojob.JobNo.ToString();
                report.Dictionary.Variables["HBLID"].Value = detail.hblid.ToString();
                report.Dictionary.Variables["debitno"].Value = detail.debitno;
                report.Render();
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

        public async Task<BoolandMessReponse> ExportDebitTruck(M_Debit detail, string cur_type, bool isMBL, string billType = "PASL", string branches = "")
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportDebitNote_Truck.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                report.Culture = "en-US";
                report.Dictionary.Variables["BillType"].Value = billType;
                ApplyCompanyBranchVariable(report, branches);

                var list_debit = await GetListDebit_Debitno(detail.debitno);

                double? total_payment = 0;
                double? total_amount_notvat = 0;

                double? total_amount_Tax = 0;
                foreach (var item_debit in list_debit)
                {
                    if (cur_type == "VND")
                    {
                        if (item_debit.tiente == "VND")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue * item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) * item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) * item_debit.tigiadebit;
                        }

                    }
                    else
                    {
                        if (item_debit.tiente == "USD")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue / item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) / item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) / item_debit.tigiadebit;

                        }
                    }
                }

                report.Dictionary.Variables["Total_shipment"].Value = (total_payment ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_notvat"].Value = (total_amount_notvat ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_Tax"].Value = (total_amount_Tax ?? 0).ToString("#,##0.##");


                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                total_payment = Math.Round(total_payment ?? 0, 2);
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(total_payment, cur_type);
                report.Dictionary.Variables["Cur_type"].Value = cur_type;
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy");
                var hblinfo = await GetHBL_byHBLid(detail.hblid);
                var mblinfo = await GetMBL_byHBLid(isMBL ? detail.mblid : hblinfo.mblid);
                var infojob = await GetJob_byid(mblinfo.Jobid);
                report.Dictionary.Variables["RefNo1"].Value = infojob.JobNo;
                report.Dictionary.Variables["debitno"].Value = detail.debitno;

                report.Dictionary.Variables["isMBL"].Value = isMBL.ToString();
                report.Dictionary.Variables["LDXNo"].Value = isMBL ? mblinfo.mbl_Truck_LenhDieuXeNo : hblinfo.Truck_LenhDieuXeNo;
                report.Dictionary.Variables["WhereHBL"].Value = isMBL ? $" Where 1=0" : $" Where hblid = '{detail.hblid}' ";
                report.Dictionary.Variables["WhereMBL"].Value = isMBL ? $" Where mblid = '{detail.mblid}' " : $" Where 1=0 "; ;
                report.Render();
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
        public async Task<BoolandMessReponse> ExportDebitSI(List<M_Debit> list_debit, List<M_HBL> hblinfo, string cur_type, string billType = "PASL")
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportListDebitNote_SeaImport.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();



                report.Load(rpt);
                report.Culture = "en-US";
                report.Dictionary.Variables["BillType"].Value = billType;
                var fcl = await GetFLCByMBLID(hblinfo.First().mblid);
                double? total_payment = 0;
                double? total_amount_notvat = 0;

                double? total_amount_Tax = 0;
                foreach (var item_debit in list_debit)
                {
                    if (cur_type == "VND")
                    {
                        if (item_debit.tiente == "VND")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue * item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) * item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) * item_debit.tigiadebit;
                        }

                    }
                    else
                    {
                        if (item_debit.tiente == "USD")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue / item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) / item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) / item_debit.tigiadebit;

                        }
                    }
                }

                report.Dictionary.Variables["Total_shipment"].Value = (total_payment ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_notvat"].Value = (total_amount_notvat ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_Tax"].Value = (total_amount_Tax ?? 0).ToString("#,##0.##");

                if (fcl == "F")
                {
                    report.Dictionary.Variables["volume"].Value = hblinfo.First().say;
                }
                else
                {
                    report.Dictionary.Variables["volume"].Value = hblinfo.First().NoOfPackages + " / " + hblinfo.First().gross + " / " + " / " + hblinfo.First().cbm;
                }
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                report.Dictionary.Variables["eta"].Value = hblinfo.First().ETA.HasValue ? hblinfo.First().ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                report.Dictionary.Variables["etd"].Value = hblinfo.First().ETD.HasValue ? hblinfo.First().ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                total_payment = Math.Round(total_payment ?? 0, 2);
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(total_payment, cur_type);
                report.Dictionary.Variables["Cur_type"].Value = cur_type;
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy");
                report.Dictionary.Variables["HBLID"].Value = hblinfo.First().hblID.ToString();

                var hbls = string.Join(";", hblinfo.Select(x => x.hbl));
                report.Dictionary.Variables["HBLs"].Value = hbls;
                report.Dictionary.Variables["MBLs"].Value = "";

                var querydebit = "WHERE debitId IN (";
                foreach (var item in list_debit)
                    querydebit += $"'{item.debitId}',";
                if (list_debit.Any())
                    querydebit = querydebit.TrimEnd(',') + ")";
                else
                    querydebit = "WHERE 1=0";
                report.Dictionary.Variables["debitnos"].Value = querydebit.ToString();

                report.Render();
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

        public async Task<BoolandMessReponse> ExportCreditSI(List<M_Credit> list_debit, List<M_HBL> hblinfo, string cur_type)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportCreditNote_SeaImport.mrt");
                var connectionString = ResolveReportConnectionString();
                var companyLogo = await GetCompanyLogoAsync();

                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                report.Culture = "en-US";
                ApplyReportConnectionString(report, connectionString);
                ApplyCompanyLogoToReport(report, companyLogo, showLogo: true);
                var fcl = await GetFLCByMBLID(hblinfo.First().mblid);
                double? total_payment = 0;
                double? total_amount_notvat = 0;

                double? total_amount_Tax = 0;
                foreach (var item_debit in list_debit)
                {
                    if (cur_type == "VND")
                    {
                        if (item_debit.tiente == "VND")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue * item_debit.tigiacredit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) * item_debit.tigiacredit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) * item_debit.tigiacredit;
                        }

                    }
                    else
                    {
                        if (item_debit.tiente == "USD")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue / item_debit.tigiacredit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) / item_debit.tigiacredit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) / item_debit.tigiacredit;

                        }
                    }
                }

                report.Dictionary.Variables["Total_shipment"].Value = (total_payment ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_notvat"].Value = (total_amount_notvat ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_Tax"].Value = (total_amount_Tax ?? 0).ToString("#,##0.##");

                if (fcl == "F")
                {
                    report.Dictionary.Variables["volume"].Value = hblinfo.First().say;
                }
                else
                {
                    report.Dictionary.Variables["volume"].Value = hblinfo.First().NoOfPackages + " / " + hblinfo.First().gross + " / " + " / " + hblinfo.First().cbm;
                }
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                report.Dictionary.Variables["eta"].Value = hblinfo.First().ETA.HasValue ? hblinfo.First().ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                report.Dictionary.Variables["etd"].Value = hblinfo.First().ETD.HasValue ? hblinfo.First().ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                total_payment = Math.Round(total_payment ?? 0, 2);
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(total_payment, cur_type);
                report.Dictionary.Variables["Cur_type"].Value = cur_type;
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy");
                report.Dictionary.Variables["HBLID"].Value = hblinfo.First().hblID.ToString();

                var hbls = string.Join(";", hblinfo.Select(x => x.hbl));
                //report.Dictionary.Variables["HBLs"].Value = hbls;
                //report.Dictionary.Variables["MBLs"].Value = "";

                var querydebit = "WHERE creditId IN (";
                foreach (var item in list_debit)
                    querydebit += $"'{item.creditid}',";
                if (list_debit.Any())
                    querydebit = querydebit.TrimEnd(',') + ")";
                else
                    querydebit = "WHERE 1=0";
                report.Dictionary.Variables["creditIDs"].Value = querydebit.ToString();

                report.Render();
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

        public async Task<BoolandMessReponse> ExportDebitSI_mbl(M_Debit detail, string cur_type, string billType = "PASL", string branches = "")
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportDebitNote_SeaImport_MBL.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Culture = "en-US";
                report.Load(rpt);
                report.Dictionary.Variables["BillType"].Value = billType;
                ApplyCompanyBranchVariable(report, branches);

                var mblinfo = await GetMBL_byHBLid(detail.mblid);
                var fcl = await GetFLCByMBLID(detail.mblid);

                var list_debit = await GetListDebit_Debitno(detail.debitno);

                double? total_payment = 0;
                double? total_amount_notvat = 0;

                double? total_amount_Tax = 0;
                foreach (var item_debit in list_debit)
                {
                    if (cur_type == "VND")
                    {
                        if (item_debit.tiente == "VND")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue * item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) * item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) * item_debit.tigiadebit;
                        }

                    }
                    else
                    {
                        if (item_debit.tiente == "USD")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue / item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) / item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) / item_debit.tigiadebit;

                        }
                    }
                }

                report.Dictionary.Variables["Total_shipment"].Value = (total_payment ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_notvat"].Value = (total_amount_notvat ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_Tax"].Value = (total_amount_Tax ?? 0).ToString("#,##0.##");

                if (fcl == "F")
                {
                    report.Dictionary.Variables["volume"].Value = mblinfo.Say;
                }
                else
                {
                    report.Dictionary.Variables["volume"].Value = mblinfo.NoOfPackages + " / " + mblinfo.Gross + " / " + " / " + mblinfo.CBM;
                }

                total_payment = Math.Round(total_payment ?? 0, 2);
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(total_payment, cur_type);
                report.Dictionary.Variables["Cur_type"].Value = cur_type;
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                report.Dictionary.Variables["eta"].Value = mblinfo.ETA.HasValue ? mblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                report.Dictionary.Variables["etd"].Value = mblinfo.ETD.HasValue ? mblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                var infojob = await GetJob_byid(mblinfo.Jobid);
                report.Dictionary.Variables["Refno"].Value = infojob.JobNo.ToString();
                report.Dictionary.Variables["HBLID"].Value = detail.hblid.ToString();
                report.Dictionary.Variables["MBLID"].Value = detail.mblid.ToString();
                report.Dictionary.Variables["debitno"].Value = detail.debitno;
                report.Render();
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

        public async Task<BoolandMessReponse> ExportDebitSE(M_Debit detail, string cur_type, string billType = "PASL", string branches = "")
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportDebitNote_SeaExport.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Culture = "en-US";
                report.Load(rpt);
                report.Dictionary.Variables["BillType"].Value = billType;
                ApplyCompanyBranchVariable(report, branches);

                var hblinfo = await GetHBL_byHBLid(detail.hblid); // lay ra say volume
                var fcl = await GetFLCByMBLID(hblinfo.mblid); //

                var list_debit = await GetListDebit_Debitno(detail.debitno);

                double? total_payment = 0;
                double? total_amount_notvat = 0;

                double? total_amount_Tax = 0;
                foreach (var item_debit in list_debit)
                {
                    if (cur_type == "VND")
                    {
                        if (item_debit.tiente == "VND")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue * item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) * item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) * item_debit.tigiadebit;
                        }

                    }
                    else
                    {
                        if (item_debit.tiente == "USD")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue / item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) / item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) / item_debit.tigiadebit;

                        }
                    }
                }

                report.Dictionary.Variables["Total_shipment"].Value = (total_payment ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_notvat"].Value = (total_amount_notvat ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_Tax"].Value = (total_amount_Tax ?? 0).ToString("#,##0.##");

                if (fcl == "F")
                {
                    report.Dictionary.Variables["volume"].Value = hblinfo.say;
                }
                else
                {
                    report.Dictionary.Variables["volume"].Value = hblinfo.NoOfPackages + " / " + hblinfo.gross + " / " + " / " + hblinfo.cbm;
                }

                total_payment = Math.Round(total_payment ?? 0, 2);
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(total_payment, cur_type);
                report.Dictionary.Variables["Cur_type"].Value = cur_type;
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                report.Dictionary.Variables["eta"].Value = hblinfo.ETA.HasValue ? hblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                report.Dictionary.Variables["etd"].Value = hblinfo.ETD.HasValue ? hblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                var mblinfo = await GetMBL_byHBLid(hblinfo.mblid);
                var infojob = await GetJob_byid(mblinfo.Jobid);
                report.Dictionary.Variables["Refno"].Value = infojob.JobNo.ToString();
                report.Dictionary.Variables["HBLID"].Value = detail.hblid.ToString();
                report.Dictionary.Variables["debitno"].Value = detail.debitno;
                report.Render();
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
        public async Task<BoolandMessReponse> ExportDebitSE_mbl(M_Debit detail, string cur_type, string billType = "PASL", string branches = "")
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportDebitNote_SeaExport_MBL.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Culture = "en-US";
                report.Load(rpt);
                report.Dictionary.Variables["BillType"].Value = billType;
                ApplyCompanyBranchVariable(report, branches);

                var mblinfo = await GetMBL_byHBLid(detail.mblid);
                var fcl = await GetFLCByMBLID(detail.mblid);

                var list_debit = await GetListDebit_Debitno(detail.debitno);

                double? total_payment = 0;
                double? total_amount_notvat = 0;

                double? total_amount_Tax = 0;
                foreach (var item_debit in list_debit)
                {
                    if (cur_type == "VND")
                    {
                        if (item_debit.tiente == "VND")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue * item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) * item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) * item_debit.tigiadebit;
                        }

                    }
                    else
                    {
                        if (item_debit.tiente == "USD")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue / item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) / item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) / item_debit.tigiadebit;

                        }
                    }
                }

                report.Dictionary.Variables["Total_shipment"].Value = (total_payment ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_notvat"].Value = (total_amount_notvat ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_Tax"].Value = (total_amount_Tax ?? 0).ToString("#,##0.##");

                if (fcl == "F")
                {
                    report.Dictionary.Variables["volume"].Value = mblinfo.Say;
                }
                else
                {
                    report.Dictionary.Variables["volume"].Value = mblinfo.NoOfPackages + " / " + mblinfo.Gross + " / " + " / " + mblinfo.CBM;
                }

                total_payment = Math.Round(total_payment ?? 0, 2);
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(total_payment, cur_type);
                report.Dictionary.Variables["Cur_type"].Value = cur_type;
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                report.Dictionary.Variables["eta"].Value = mblinfo.ETA.HasValue ? mblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                report.Dictionary.Variables["etd"].Value = mblinfo.ETD.HasValue ? mblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                var infojob = await GetJob_byid(mblinfo.Jobid);
                report.Dictionary.Variables["Refno"].Value = infojob.JobNo.ToString();
                report.Dictionary.Variables["HBLID"].Value = detail.hblid.ToString();
                report.Dictionary.Variables["MBLID"].Value = detail.mblid.ToString();
                report.Dictionary.Variables["debitno"].Value = detail.debitno;
                report.Render();
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

        public async Task<BoolandMessReponse> ExportDebitAE(M_Debit detail, string cur_type, string billType = "PASL", string branches = "")
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportDebitNote_AirExport.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Culture = "en-US";
                report.Load(rpt);
                report.Dictionary.Variables["BillType"].Value = billType;
                ApplyCompanyBranchVariable(report, branches);

                var hblinfo = await GetHBL_byHBLid(detail.hblid); // lay ra say volume
                var fcl = await GetFLCByMBLID(hblinfo.mblid); //

                var list_debit = await GetListDebit_Debitno(detail.debitno);

                double? total_payment = 0;
                double? total_amount_notvat = 0;

                double? total_amount_Tax = 0;
                foreach (var item_debit in list_debit)
                {
                    if (cur_type == "VND")
                    {
                        if (item_debit.tiente == "VND")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue * item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) * item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) * item_debit.tigiadebit;
                        }

                    }
                    else
                    {
                        if (item_debit.tiente == "USD")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue / item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) / item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) / item_debit.tigiadebit;

                        }
                    }
                }

                report.Dictionary.Variables["Total_shipment"].Value = (total_payment ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_notvat"].Value = (total_amount_notvat ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_Tax"].Value = (total_amount_Tax ?? 0).ToString("#,##0.##");

                if (fcl == "F")
                {
                    report.Dictionary.Variables["volume"].Value = hblinfo.say;
                }
                else
                {
                    report.Dictionary.Variables["volume"].Value = hblinfo.NoOfPackages + " / " + hblinfo.gross + " / " + " / " + hblinfo.cbm;
                }

                total_payment = Math.Round(total_payment ?? 0, 2);
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(total_payment, cur_type);
                report.Dictionary.Variables["Cur_type"].Value = cur_type;
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                report.Dictionary.Variables["eta"].Value = hblinfo.ETA.HasValue ? hblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                report.Dictionary.Variables["etd"].Value = hblinfo.ETD.HasValue ? hblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                var mblinfo = await GetMBL_byHBLid(hblinfo.mblid);
                var infojob = await GetJob_byid(mblinfo.Jobid);
                report.Dictionary.Variables["Refno"].Value = infojob.JobNo.ToString();
                report.Dictionary.Variables["HBLID"].Value = detail.hblid.ToString();
                report.Dictionary.Variables["debitno"].Value = detail.debitno;
                report.Render();
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

        public async Task<BoolandMessReponse> ExportDebitAE_mbl(M_Debit detail, string cur_type, string billType = "PASL", string branches = "")
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportDebitNote_Airxport_MBL.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Culture = "en-US";
                report.Load(rpt);
                report.Dictionary.Variables["BillType"].Value = billType;
                ApplyCompanyBranchVariable(report, branches);

                var mblinfo = await GetMBL_byHBLid(detail.mblid);
                var fcl = await GetFLCByMBLID(detail.mblid);

                var list_debit = await GetListDebit_Debitno(detail.debitno);

                double? total_payment = 0;
                double? total_amount_notvat = 0;

                double? total_amount_Tax = 0;
                foreach (var item_debit in list_debit)
                {
                    if (cur_type == "VND")
                    {
                        if (item_debit.tiente == "VND")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue * item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) * item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) * item_debit.tigiadebit;
                        }

                    }
                    else
                    {
                        if (item_debit.tiente == "USD")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue / item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) / item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) / item_debit.tigiadebit;

                        }
                    }
                }

                report.Dictionary.Variables["Total_shipment"].Value = (total_payment ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_notvat"].Value = (total_amount_notvat ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_Tax"].Value = (total_amount_Tax ?? 0).ToString("#,##0.##");

                if (fcl == "F")
                {
                    report.Dictionary.Variables["volume"].Value = mblinfo.Say;
                }
                else
                {
                    report.Dictionary.Variables["volume"].Value = mblinfo.NoOfPackages + " / " + mblinfo.Gross + " / " + " / " + mblinfo.CBM;
                }

                total_payment = Math.Round(total_payment ?? 0, 2);
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(total_payment, cur_type);
                report.Dictionary.Variables["Cur_type"].Value = cur_type;
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                report.Dictionary.Variables["eta"].Value = mblinfo.ETA.HasValue ? mblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                report.Dictionary.Variables["etd"].Value = mblinfo.ETD.HasValue ? mblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                var infojob = await GetJob_byid(mblinfo.Jobid);
                report.Dictionary.Variables["Refno"].Value = infojob.JobNo.ToString();
                report.Dictionary.Variables["HBLID"].Value = detail.hblid.ToString();
                report.Dictionary.Variables["MBLID"].Value = detail.mblid.ToString();
                report.Dictionary.Variables["debitno"].Value = detail.debitno;
                report.Render();
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
        public async Task<BoolandMessReponse> ExportDebitAI(M_Debit detail, string cur_type, string billType = "PASL", string branches = "")
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportDebitNote_AirImport.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Culture = "en-US";
                report.Load(rpt);
                report.Dictionary.Variables["BillType"].Value = billType;
                ApplyCompanyBranchVariable(report, branches);

                var hblinfo = await GetHBL_byHBLid(detail.hblid); // lay ra say volume
                var fcl = await GetFLCByMBLID(hblinfo.mblid); //

                var list_debit = await GetListDebit_Debitno(detail.debitno);

                double? total_payment = 0;
                double? total_amount_notvat = 0;

                double? total_amount_Tax = 0;
                foreach (var item_debit in list_debit)
                {
                    if (cur_type == "VND")
                    {
                        if (item_debit.tiente == "VND")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue * item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) * item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) * item_debit.tigiadebit;
                        }

                    }
                    else
                    {
                        if (item_debit.tiente == "USD")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue / item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) / item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) / item_debit.tigiadebit;

                        }
                    }
                }

                report.Dictionary.Variables["Total_shipment"].Value = (total_payment ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_notvat"].Value = (total_amount_notvat ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_Tax"].Value = (total_amount_Tax ?? 0).ToString("#,##0.##");

                if (fcl == "F")
                {
                    report.Dictionary.Variables["volume"].Value = hblinfo.say;
                }
                else
                {
                    report.Dictionary.Variables["volume"].Value = hblinfo.NoOfPackages + " / " + hblinfo.gross + " / " + " / " + hblinfo.cbm;
                }

                total_payment = Math.Round(total_payment ?? 0, 2);
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(total_payment, cur_type);
                report.Dictionary.Variables["Cur_type"].Value = cur_type;
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                report.Dictionary.Variables["eta"].Value = hblinfo.ETA.HasValue ? hblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                report.Dictionary.Variables["etd"].Value = hblinfo.ETD.HasValue ? hblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                var mblinfo = await GetMBL_byHBLid(hblinfo.mblid);
                var infojob = await GetJob_byid(mblinfo.Jobid);
                report.Dictionary.Variables["Refno"].Value = infojob.JobNo.ToString();
                report.Dictionary.Variables["HBLID"].Value = detail.hblid.ToString();
                report.Dictionary.Variables["debitno"].Value = detail.debitno;
                report.Render();
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
        public async Task<BoolandMessReponse> ExportDebitAI_mbl(M_Debit detail, string cur_type, string billType = "PASL", string branches = "")
        {
            try
            {



                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportDebitNote_AirImport_MBL.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Culture = "en-US";


                report.Load(rpt);
                report.Dictionary.Variables["BillType"].Value = billType;
                ApplyCompanyBranchVariable(report, branches);


                var mblinfo = await GetMBL_byHBLid(detail.mblid);
                var fcl = await GetFLCByMBLID(detail.mblid);

                var list_debit = await GetListDebit_Debitno(detail.debitno);

                double? total_payment = 0;
                double? total_amount_notvat = 0;

                double? total_amount_Tax = 0;
                foreach (var item_debit in list_debit)
                {
                    if (cur_type == "VND")
                    {
                        if (item_debit.tiente == "VND")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue * item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) * item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) * item_debit.tigiadebit;
                        }

                    }
                    else
                    {
                        if (item_debit.tiente == "USD")
                        {
                            total_payment += item_debit.thanhtiensauthue;
                            total_amount_notvat += item_debit.dongia * item_debit.soluong;
                            total_amount_Tax += (item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100);
                        }
                        else
                        {
                            total_payment += item_debit.thanhtiensauthue / item_debit.tigiadebit;
                            total_amount_notvat += (item_debit.dongia * item_debit.soluong) / item_debit.tigiadebit;
                            total_amount_Tax += ((item_debit.dongia * item_debit.soluong) * (item_debit.thue / 100)) / item_debit.tigiadebit;

                        }
                    }
                }

                report.Dictionary.Variables["Total_shipment"].Value = (total_payment ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_notvat"].Value = (total_amount_notvat ?? 0).ToString("#,##0.##");
                report.Dictionary.Variables["total_amount_Tax"].Value = (total_amount_Tax ?? 0).ToString("#,##0.##");

                if (fcl == "F")
                {
                    report.Dictionary.Variables["volume"].Value = mblinfo.Say;
                }
                else
                {
                    report.Dictionary.Variables["volume"].Value = mblinfo.NoOfPackages + " / " + mblinfo.Gross + " / " + " / " + mblinfo.CBM;
                }

                total_payment = Math.Round(total_payment ?? 0, 2);
                report.Dictionary.Variables["In_Word"].Value = ConvertToWords(total_payment, cur_type);
                report.Dictionary.Variables["Cur_type"].Value = cur_type;
                report.Dictionary.Variables["DatetimeNow"].Value = DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                report.Dictionary.Variables["eta"].Value = mblinfo.ETA.HasValue ? mblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                report.Dictionary.Variables["etd"].Value = mblinfo.ETD.HasValue ? mblinfo.ETA.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "";
                var infojob = await GetJob_byid(mblinfo.Jobid);
                report.Dictionary.Variables["Refno"].Value = infojob.JobNo.ToString();
                report.Dictionary.Variables["HBLID"].Value = detail.hblid.ToString();
                report.Dictionary.Variables["MBLID"].Value = detail.mblid.ToString();
                report.Dictionary.Variables["debitno"].Value = detail.debitno;
                report.Render();
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
        public async Task<List<string>> GetListShipperMBL()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.MBL.OrderBy(x => x.Shipper).Select(x => x.Shipper).Distinct().ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }
        public async Task<List<string>> GetListConsigneeMBL()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.MBL.OrderBy(x => x.Consignee).Select(x => x.Consignee).Distinct().ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }
        public async Task<List<string>> GetListNotiMBL()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.MBL.OrderBy(x => x.Notify1).Select(x => x.Notify1).Distinct().ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }

        public PortModel queryPort(string name)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.Port
                .Where(x => x.PORT.Contains(name) || x.PORT_CODE.Contains(name))
                .FirstOrDefault();
                return rs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string queryVessel(string name)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.VesselSpace
                .Where(x => x.Vessel.Contains(name))
                .Select(x => x.Vessel)
                .FirstOrDefault();
                return rs;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public string queryVoy(string name)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.VesselSpace
                .Where(x => x.Voy.Contains(name))
                .Select(x => x.Voy)
                .FirstOrDefault();
                return rs;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public string GetNumberandRemark(Guid? id)
        {
            var rs = "";
            List<M_Container> cont = this.GetListContainerMBL(id) ?? new List<M_Container>();
            foreach (M_Container c in cont)
            {
                rs += (c?.CONTAINER_NO ?? "") + "/" + (c?.CTN_SIZE_TYPE ?? "") + "/" + (c?.Seal ?? "") + "\n";
            }
            return rs;
        }

        public string GetNoOfPackages(Guid? id)
        {
            var rs = "";
            double tong = 0;
            var dvt = "";
            List<M_Container> cont = this.GetListContainerMBL(id) ?? new List<M_Container>();
            List<string> list = new List<string>();
            foreach (M_Container c in cont)
            {
                var pkgsVal = c?.pkgs ?? 0d;
                var code = string.IsNullOrWhiteSpace(c?.pkgsCode) ? "" : c!.pkgsCode!.Trim();
                rs += pkgsVal.ToString() + (code.Length > 0 ? " " + code : "") + "\n";
                tong += pkgsVal;
                if (code.Length > 0 && !list.Contains(code))
                    list.Add(code);
            }
            try
            {
                if (list.Count > 1)
                    dvt = "PACKAGE(S)";
                else if (list.Count == 0)
                    dvt = "PACKAGE(S)";
                else
                {
                    var single = list[0];
                    if (cont.Count > 1)
                        dvt = single;
                    else
                    {
                        if (single.Contains("(S)"))
                            dvt = single;
                        else
                            dvt = single + "(S)";
                    }
                }
            }
            catch
            {
                dvt = "PACKAGE(S)";
            }
            rs += "------------------------" + "\n" + tong.ToString() + " " + dvt;
            return rs;
        }
        public string GetSumOfGross(Guid? id)
        {
            var rs = "";
            double tong = 0;
            List<M_Container> cont = this.GetListContainerMBL(id) ?? new List<M_Container>();
            foreach (M_Container c in cont)
            {
                var gross = c?.GrossWeight ?? 0d;
                rs += gross.ToString() + " KGS" + "\n";
                tong += gross;
            }
            rs += "--------------------" + "\n" + tong.ToString() + " KGS";
            return rs;
        }
        public string GetSumOfCBM(Guid? id)
        {
            var rs = "";
            double tong = 0;
            List<M_Container> cont = this.GetListContainerMBL(id) ?? new List<M_Container>();
            foreach (M_Container c in cont)
            {
                var cbmText = c?.cbm;
                if (!double.TryParse(cbmText, out var cbmValue)) cbmValue = 0d;
                rs += cbmValue.ToString() + " CBM" + "\n";
                tong += cbmValue;
            }
            rs += "--------------------" + "\n" + tong.ToString() + " CBM";
            return rs;
        }
        public string GetDescriptionOfGoods(Guid? id, Guid? cusID)
        {
            var rs = "";
            List<M_Container> cont = this.GetListContainerMBL(id) ?? new List<M_Container>();
            var type = GetNameFLCByMBLID(id);
            if (type == "F")
                rs += (this.saycont(id).Replace("SAY: ", "").Replace("ONLY", "S.T.C")).ToUpper() + "\n";
            else
                rs += "PART OF CONTAINER S.T.C \n";
            var checkCold = false;
            foreach (M_Container c in cont)
            {
                rs += (c?.description ?? "") + "\n";
                if ((c?.CTN_SIZE_TYPE ?? "").Contains("F"))
                    checkCold = true;
            }
            //rs += "HS CODE: " + csv.GetHSCodeCompanyFromID(cusID);
            if (checkCold)
                rs += "\nContainer temperature to be set at  Degrees Celsius".ToUpper();
            //if (type == "F")
            //    rs += "\n" + "Shipper’s Load, Count & Seal";
            return rs;
        }
        public string GetDescriptionOfGoodsHBL(Guid? id, Guid? cusID)
        {
            var rs = "";
            List<M_Container> cont = this.GetListContainerHBL(id) ?? new List<M_Container>();
            var type = GetFLCByHBLID(id);
            if (type == "F")
                rs += (this.saycontHBL(id).Replace("SAY: ", "").Replace("ONLY", "S.T.C")).ToUpper() + "\n";
            else
                rs += "PART OF CONTAINER S.T.C \n";
            var checkCold = false;
            foreach (M_Container c in cont)
            {
                rs += (c?.description ?? "") + "\n";
                if ((c?.CTN_SIZE_TYPE ?? "").Contains("F"))
                    checkCold = true;
            }
            //rs += "HS CODE: " + csv.GetHSCodeCompanyFromID(cusID);
            if (checkCold)
                rs += "\nContainer temperature to be set at  Degrees Celsius".ToUpper();
            //if (type == "F")
            //    rs += "\n" + "Shipper’s Load, Count & Seal";
            return rs;
        }

        public string GetNumberandRemarkHBL(Guid? id)
        {
            var rs = "";
            List<M_Container> cont = this.GetListContainerHBL(id) ?? new List<M_Container>();
            foreach (M_Container c in cont)
            {
                rs += (c?.CONTAINER_NO ?? "") + "/" + (c?.Seal ?? "") + "/" + (c?.CTN_SIZE_TYPE ?? "") + "\n";
            }
            return rs;
        }

        public string GetNoOfPackagesHBL(Guid? id)
        {
            var rs = "";
            double tong = 0;
            var dvt = "";
            List<M_Container> cont = this.GetListContainerHBL(id) ?? new List<M_Container>();
            List<string> list = new List<string>();
            foreach (M_Container c in cont)
            {
                var pkgsVal = c?.pkgs ?? 0d;
                var code = string.IsNullOrWhiteSpace(c?.pkgsCode) ? "" : c!.pkgsCode!.Trim();

                rs += pkgsVal.ToString() + (code.Length > 0 ? " " + code : "") + "\n";
                tong += pkgsVal;
                if (code.Length > 0 && !list.Contains(code))
                    list.Add(code);
            }
            try
            {
                if (list.Count > 1)
                    dvt = "PACKAGE(S)";
                else if (list.Count == 0)
                    dvt = "PACKAGE(S)";
                else
                {
                    if (cont.Count > 1)
                        dvt = list.LastOrDefault();
                    else
                    {
                        var last = list.LastOrDefault() ?? "";
                        if (last.Contains("(S)"))
                            dvt = last;
                        else
                            dvt = last + "(S)";

                    }

                }

            }
            catch
            {
                dvt = "PACKAGE(S)";
            }
            rs += "------------------------" + "\n" + tong.ToString() + " " + dvt;
            return rs;
        }
        public string GetSumOfGrossHBL(Guid? id)
        {
            var rs = "";
            double tong = 0;
            List<M_Container> cont = this.GetListContainerHBL(id) ?? new List<M_Container>();
            foreach (M_Container c in cont)
            {
                var gross = c?.GrossWeight ?? 0d;
                rs += gross.ToString() + " KGS" + "\n";
                tong += gross;
            }
            rs += "--------------------" + "\n" + tong.ToString() + " KGS";
            return rs;
        }
        public string GetSumOfCBMHBL(Guid? id)
        {
            var rs = "";
            double tong = 0;
            List<M_Container> cont = this.GetListContainerHBL(id) ?? new List<M_Container>();
            foreach (M_Container c in cont)
            {
                var cbmText = c?.cbm;
                if (!double.TryParse(cbmText, out var cbmValue)) cbmValue = 0d;

                rs += cbmValue.ToString() + " CBM" + "\n";
                tong += cbmValue;
            }
            rs += "--------------------" + "\n" + tong.ToString() + " CBM";
            return rs;
        }
        public string saycont(Guid? id)
        {
            var chuoicont = "";
            var rs = "";
            List<M_Container> cont = this.GetListContainerMBL(id);
            foreach (M_Container c in cont)
            {
                chuoicont += c.CTN_SIZE_TYPE + ";";

            }
            rs = "SAY: " + DemContSay(chuoicont).ToUpper() + " Container(s) ONLY".ToUpper();

            return rs;

        }
        public string saycontHBL(Guid? id)
        {
            var chuoicont = "";
            var rs = "";
            List<M_Container> cont = this.GetListContainerHBL(id);
            foreach (M_Container c in cont)
            {
                chuoicont += c.CTN_SIZE_TYPE + ";";

            }
            rs = "SAY: " + DemContSay(chuoicont).ToUpper() + " Container(s) ONLY".ToUpper();

            return rs;

        }
        public string DemContSay(string chuoi)
        {
            try
            {

                int HQ20 = 0;
                int HQ40 = 0;
                int GP20 = 0;
                int GP40 = 0;
                int DC20 = 0;
                int DC40 = 0;
                int hc40 = 0;
                int RH40 = 0;
                int HC45 = 0;
                int rf20 = 0;
                int rf40 = 0;
                int ot20 = 0;
                int ot40 = 0;
                int fr20 = 0;
                int fr40 = 0;

                foreach (string i in chuoi.Split(";"))
                {
                    string containerType = i.ToUpper();

                    if (containerType == "20DC")
                    {
                        DC20++;
                    }
                    else if (containerType == "40DC")
                    {
                        DC40++;
                    }
                    else if (containerType == "20GP")
                    {
                        GP20++;
                    }
                    else if (containerType == "40GP")
                    {
                        GP40++;
                    }
                    else if (containerType == "20HQ")
                    {
                        HQ20++;
                    }
                    else if (containerType == "40HQ")
                    {
                        HQ40++;
                    }
                    else if (containerType == "40HC")
                    {
                        hc40++;
                    }
                    else if (containerType == "40RH")
                    {
                        RH40++;
                    }
                    else if (containerType == "45HC")
                    {
                        HC45++;
                    }
                    else if (containerType == "20RF")
                    {
                        rf20++;
                    }
                    else if (containerType == "40RF")
                    {
                        rf40++;
                    }
                    else if (containerType == "20OT")
                    {
                        ot20++;
                    }
                    else if (containerType == "40OT")
                    {
                        ot40++;
                    }
                    else if (containerType == "20FR")
                    {
                        fr20++;
                    }
                    else if (containerType == "40FR")
                    {
                        fr40++;
                    }
                }

                string demContSay = "";

                if (DC20 > 0)
                {
                    demContSay += ConvertNumberToWords(DC20) + "(" + DC20.ToString() + ") x 20DC ";
                }
                if (DC40 > 0)
                {
                    demContSay += ConvertNumberToWords(DC40) + "(" + DC40.ToString() + ") x 40DC ";
                }
                if (GP20 > 0)
                {
                    demContSay += ConvertNumberToWords(GP20) + "(" + GP20.ToString() + ") x 20GP ";
                }
                if (GP40 > 0)
                {
                    demContSay += ConvertNumberToWords(GP40) + "(" + GP40.ToString() + ") x 40GP ";
                }
                if (HQ20 > 0)
                {
                    demContSay += ConvertNumberToWords(HQ20) + "(" + HQ20.ToString() + ") x 20HQ ";
                }
                if (HQ40 > 0)
                {
                    demContSay += ConvertNumberToWords(HQ40) + "(" + HQ40.ToString() + ") x 40HQ ";
                }
                if (hc40 > 0)
                {
                    demContSay += ConvertNumberToWords(hc40) + "(" + hc40.ToString() + ") x 40HC ";
                }
                if (RH40 > 0)
                {
                    demContSay += ConvertNumberToWords(RH40) + "(" + RH40.ToString() + ") x 40RH ";
                }
                if (HC45 > 0)
                {
                    demContSay += ConvertNumberToWords(HC45) + "(" + HC45.ToString() + ") x 45HC ";
                }
                if (rf20 > 0)
                {
                    demContSay += ConvertNumberToWords(rf20) + "(" + rf20.ToString() + ") x 20RF ";
                }
                if (rf40 > 0)
                {
                    demContSay += ConvertNumberToWords(rf40) + "(" + rf40.ToString() + ") x 40RF ";
                }
                if (ot20 > 0)
                {
                    demContSay += ConvertNumberToWords(ot20) + "(" + ot20.ToString() + ") x 20OT ";
                }
                if (ot40 > 0)
                {
                    demContSay += ConvertNumberToWords(ot40) + "(" + ot40.ToString() + ") x 40OT ";
                }
                if (fr20 > 0)
                {
                    demContSay += ConvertNumberToWords(fr20) + "(" + fr20.ToString() + ") x 20FR ";
                }
                if (fr40 > 0)
                {
                    demContSay += ConvertNumberToWords(fr40) + "(" + fr40.ToString() + ") x 40FR ";
                }

                return demContSay.Trim(); // Remove trailing spaces

            }
            catch (Exception)
            {
                return ""; // or handle the exception appropriately
            }
        }

        //Function to convert number to words.
        private string ConvertNumberToWords(int number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "Minus " + ConvertNumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += ConvertNumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += ConvertNumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += ConvertNumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += " " + unitsMap[number % 10];
                }
            }

            return words.Trim().ToUpper();
        }
        // 7.4 Agent Report rows builder
        public async Task<List<AgentReportRow>> GetAgentReportRows(DateRange range)
        {
            _context.ChangeTracker.Clear();
            var start = range.Start?.Date;
            var end = range.End?.Date;

            var query = from job in _context.Job
                        join mbl in _context.MBL on job.JobID equals mbl.Jobid
                        join hbl in _context.HBL on mbl.MblID equals hbl.mblid
                        join agent in _context.Customer on hbl.AgentID equals agent.Customer_ID into ag
                        from agent in ag.DefaultIfEmpty()
                        join client in _context.Customer on hbl.CustomerID equals client.Customer_ID into cl
                        from client in cl.DefaultIfEmpty()
                        where job.Continued == true &&
                              hbl.datereport.HasValue &&
                              hbl.datereport.Value.Date >= start &&
                              hbl.datereport.Value.Date <= end
                        select new { hbl, mbl, agentName = agent.COMPANY, clientName = client.COMPANY, job.JobNo };

            var raw = await query.OrderBy(x => x.hbl.datereport).ToListAsync();
            if (raw.Count == 0) return new List<AgentReportRow>();

            var hblIds = raw.Select(x => x.hbl.hblID).Distinct().ToList();
            if (!hblIds.Any()) return new List<AgentReportRow>();

            //var agentIds = raw.Select(x => x.hbl.AgentID).Where(x => x != null).Distinct().ToList();
            var agentIds = await _context.Customer
                .Where(c => EF.Functions.Like(c.MainCode, "%Agent-Network%"))
                .Select(c => c.Customer_ID)
                .Distinct()
                .ToListAsync();

            List<M_Debit> debits;
            List<M_Credit> credits;

            if (!agentIds.Any())
            {
                debits = new List<M_Debit>();
                credits = new List<M_Credit>();
            }
            else
            {
                var inClauseHbl = string.Join(",", hblIds.Select(id => $"'{id}'"));
                var inClauseAgent = string.Join(",", agentIds.Select(id => $"'{id}'"));

                debits = await _context.Debit
                    .FromSqlRaw($@"
            SELECT * 
            FROM Debit 
            WHERE hblid IN ({inClauseHbl})
              AND CUSTOMERID IN ({inClauseAgent})")
                    .ToListAsync();

                credits = await _context.Credit
                    .FromSqlRaw($@"
            SELECT * 
            FROM Credit 
            WHERE hblid IN ({inClauseHbl})
              AND CUSTOMERID IN ({inClauseAgent})")
                    .ToListAsync();
            }

            double Convert(double? dongia, double? soluong, string? cur, double? tigia) =>
                (cur != null && cur.Equals("USD", StringComparison.OrdinalIgnoreCase))
                ? (dongia ?? 0) * (soluong ?? 0) * (tigia ?? 0)
                : (dongia ?? 0) * (soluong ?? 0);

            var result = new List<AgentReportRow>();
            int idx = 1;

            foreach (var r in raw)
            {
                // gom Debit theo Customer
                var debitGroups = debits
                    .Where(d => d.hblid == r.hbl.hblID)
                    .GroupBy(d => d.customerid);

                // gom Credit theo Customer
                var creditGroups = credits
                    .Where(c => c.hblid == r.hbl.hblID)
                    .GroupBy(c => c.customerid);

                // duyệt qua từng Customer trong Debit
                foreach (var dg in debitGroups)
                {
                    var custId = dg.Key;
                    var custDebits = dg.ToList();
                    var custCredits = creditGroups.FirstOrDefault(g => g.Key == custId)?.ToList() ?? new List<M_Credit>();

                    double amount = custDebits.Sum(d => Convert(d.dongia, d.soluong, d.tiente, d.tigiadebit))
                                     - custCredits.Sum(c => Convert(c.dongia, c.soluong, c.tiente, c.tigiacredit));

                    result.Add(new AgentReportRow
                    {
                        No = idx++,
                        JobFileNo = r.JobNo,
                        POL = r.hbl.polname ?? r.mbl.Polname,
                        POD = r.hbl.podname ?? r.mbl.Podname,
                        Carrier = r.agentName,
                        HBL = r.hbl.hbl,
                        MBL = r.mbl.Mbl,
                        Amount = Math.Round(amount, 2),
                        Client = r.clientName,
                        CustomerId = custId.ToString(),
                        Agentname = await GetCustomerNameAsync(custId)// 
                    });
                }
            }

            return result.OrderBy(r => r.Agentname).ToList();
        }
        private async Task<string?> GetCustomerNameAsync(Guid custId)
        {
            var customer = await _context.Customer
                .Where(c => c.Customer_ID == custId)
                .Select(c => c.COMPANY)
                .FirstOrDefaultAsync();

            return customer;
        }

        public async Task<List<DebtReportRow>> GetDebtReportRows(DateRange range, string type, Guid cusid, string cur)
        {
            _context.ChangeTracker.Clear();
            var start = range.Start?.Date;
            var end = range.End?.Date;

            // Lấy danh sách HBL trong khoảng thời gian
            var hblList = await (from job in _context.Job
                                 join mbl in _context.MBL on job.JobID equals mbl.Jobid
                                 join hbl in _context.HBL on mbl.MblID equals hbl.mblid
                                 where job.Continued == true &&
                                       hbl.datereport.HasValue &&
                                       hbl.datereport.Value.Date >= start &&
                                       hbl.datereport.Value.Date <= end
                                 orderby hbl.datereport
                                 select hbl).ToListAsync();

            if (!hblList.Any())
                return new List<DebtReportRow>();

            var hblIds = hblList.Select(x => x.hblID).Distinct().ToList();
            var inClauseHbl = string.Join(",", hblIds.Select(id => $"'{id}'"));

            // Lấy dữ liệu Debit, Credit, Container, HoaDonDauVao
            var debits = await _context.Debit
                .FromSqlRaw($"SELECT * FROM Debit WHERE hblid IN ({inClauseHbl}) AND CUSTOMERID = '{cusid}'")
                .ToListAsync();

            var credits = await _context.Credit
                .FromSqlRaw($"SELECT * FROM Credit WHERE hblid IN ({inClauseHbl}) AND CUSTOMERID = '{cusid}'")
                .ToListAsync();

            var containers = await _context.Container
                .FromSqlRaw($"SELECT * FROM Container WHERE hblid IN ({inClauseHbl})")
                .ToListAsync();

            var hddvs = await _context.HoaDonDauVao
                .FromSqlRaw($"SELECT * FROM HoaDonDauVao WHERE hblid IN ({inClauseHbl}) AND customerid = '{cusid}'")
                .ToListAsync();

            var hddrs = await _context.HoaDonDauRa
                      .FromSqlRaw($"SELECT * FROM HoaDonDaura WHERE hblid IN ({inClauseHbl}) AND customerid = '{cusid}'")
                      .ToListAsync();
            var result = new List<DebtReportRow>();
            int idx = 1;

            foreach (var hbl in hblList)
            {
                var hDebits = debits.Where(d => d.hblid == hbl.hblID).ToList();
                var hCredits = credits.Where(c => c.hblid == hbl.hblID).ToList();
                var hContainers = containers.Where(ct => ct.hblid == hbl.hblID).ToList();
                var hHddvs = hddvs.Where(h => h.hblid == hbl.hblID).ToList();
                var hHddrs = hddrs.Where(h => h.hblid == hbl.hblID).ToList();
                // Chỉ tạo row nếu có dữ liệu Debit hoặc Credit
                bool hasData = (type == "Debit" && hDebits.Any()) ||
                               (type == "Credit" && hCredits.Any());
                if (!hasData) continue;

                var row = new DebtReportRow
                {
                    No = idx++,
                    Description = hbl.description,
                    Hbl = hbl.hbl,
                    Qty = hContainers.Count,
                    Unit = string.Join(", ", hContainers.Select(ct => ct.CTN_SIZE_TYPE).Distinct()),

                    Total_Debit = type == "Debit"
                        ? hDebits
                                 .Sum(d => ConvertAmount(d.dongia, d.soluong, d.tiente, d.tigiadebit, cur))
                        : 0,

                    Total_Credit = type == "Credit"
                        ? hCredits
                                 .Sum(d => ConvertAmount(d.dongia, d.soluong, d.tiente, d.tigiacredit, cur))
                        : 0,

                    Total_HoaDonDauRa = type == "Debit"
                        ? hHddrs
                                 .Sum(d => ConvertAmount(d.dongia, d.soluong, d.tiente, d.tigia, cur))
                        : 0,

                    Total_HoaDonDauVao = type == "Credit"
                        ? hHddvs
                                 .Sum(d => ConvertAmount(d.dongia, d.soluong, d.tiente, d.tigia, cur))
                        : 0,

                    InvoiceNo = type == "Debit"
                        ? string.Join(", ", hDebits.Select(d => d.sohoadondaura)
                                                    .Where(inv => !string.IsNullOrEmpty(inv))
                                                    .Distinct())
                        : string.Join(", ", hCredits.Select(h => h.sodntt)
                                                   .Where(inv => !string.IsNullOrEmpty(inv))
                                                   .Distinct())
                };

                result.Add(row);
            }

            return result.OrderBy(r => r.No).ToList();
        }

        // Hàm convert theo flag chkusd
        double ConvertAmount(double? dongia, double? soluong, string? cur, double? tigia, string check_cur)
        {
            var amount = (dongia ?? 0) * (soluong ?? 0);

            if (string.IsNullOrEmpty(cur)) return amount;

            // Nếu muốn quy đổi sang VND
            if (check_cur.Equals("VND", StringComparison.OrdinalIgnoreCase))
            {
                if (cur.Equals("USD", StringComparison.OrdinalIgnoreCase))
                    return amount * (tigia ?? 0); // USD -> VND
                else
                    return amount; // VND giữ nguyên
            }

            // Nếu muốn quy đổi sang USD
            if (check_cur.Equals("USD", StringComparison.OrdinalIgnoreCase))
            {
                if (cur.Equals("USD", StringComparison.OrdinalIgnoreCase))
                    return amount; // USD giữ nguyên
                else
                    return (tigia ?? 0) == 0 ? 0 : amount / (tigia ?? 0); // VND -> USD
            }

            // Trường hợp check_cur không hợp lệ thì trả về amount gốc
            return amount;
        }
        public async Task<List<M_HBL>> GetListHBLTruckDateRange(MudBlazor.DateRange dateRange)
        {
            //_context.ChangeTracker.Clear();
            //var rs = await _context.Job.Where(x => x.Continued == true).ToListAsync();
            //rs = rs.Where(x => x.Datecreate >= dateRange.Start && x.Datecreate <= dateRange.End).OrderByDescending(x => x.Dateupdate).ToList();
            //return rs;
            _context.ChangeTracker.Clear();

            var start = dateRange.Start?.Date;
            var end = dateRange.End?.Date;

            var query = from job in _context.Job
                        join mbl in _context.MBL on job.JobID equals mbl.Jobid
                        join hbl in _context.HBL on mbl.MblID equals hbl.mblid
                        where job.Continued == true &&
                              hbl.datereport.HasValue &&
                              hbl.datereport.Value.Date >= start &&
                              hbl.datereport.Value.Date <= end &&
                              job.Loai.Contains("Truck")
                        select hbl;

            var result = await query

                .Distinct() // tránh trùng nếu nhiều HBL cùng Job
                .OrderByDescending(j => j.datereport)
                .ToListAsync();
            return result;
        }


        //public async Task<BoolandMessReponse> UpdateOrCreate(M_MBL p)
        //{
        //    try
        //    {
        //        if (p.MblID == null || p.MblID == Guid.Empty)
        //        {
        //            _context.ChangeTracker.Clear();
        //            _context.MBL.Add(p);
        //            await _context.SaveChangesAsync();
        //            return new BoolandMessReponse(true, "Create HBL/MBL Success");
        //        }
        //        else
        //        {
        //            _context.ChangeTracker.Clear();
        //            _context.MBL.Update(p);
        //            await _context.SaveChangesAsync();
        //            return new BoolandMessReponse(true, "Update HBL/MBL Success");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new BoolandMessReponse(false, "Cannot Update or Add HBL/MBL with error code: " + ex.Message);
        //    }
        //}

        public async Task<BoolandMessReponse> ExportBooking(Guid id,string billType)
        {
            try
            {
                //Create empty report object
                var report = new StiReport();
                //Load report template
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "BookingRequestNVOCC.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                report.Dictionary.Variables["ID"].Value = id.ToString();
                report.Dictionary.Variables["BillType"].Value = billType;

                
                try
                {
                    report.Render();
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

        public async Task<BoolandMessReponse> ExportLenhCapContRong(Guid id,Guid id_bk)
        {
            try
            {
                //Create empty report object
                var report = new StiReport();
                //Load report template
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "LenhCapContRong.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                report.Dictionary.Variables["ID"].Value = id_bk.ToString();
                report.Dictionary.Variables["ID_Lenh"].Value = id.ToString();
                try
                {
                    report.Render();
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

        public async Task<BoolandMessReponse> ExportCamKetMuonCont_TraRong(Guid id, Guid hblid)
        {
            try
            {
                //Create empty report object
                var report = new StiReport();
                //Load report template
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "CamKetMuonCont_TraRong.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                report.Dictionary.Variables["ID"].Value = id.ToString();
                report.Dictionary.Variables["HBL_ID"].Value = hblid.ToString();
                var now = DateTime.Now;
                var ngayHienTai = $"TP.Hồ Chí Minh, ngày {now.Day} tháng {now.Month} năm {now.Year}";
                report.Dictionary.Variables["Ngay"].Value = ngayHienTai.ToString();
            
                try
                {
                    report.Render();
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
        public async Task<string> GetTypeByMBLID(Guid? mblid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var mbl = await _context.MBL.FirstOrDefaultAsync(x => x.MblID == mblid);
                var rs = await _context.Job.FirstOrDefaultAsync(x => x.JobID == mbl!.Jobid);
                return rs!.Loai!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }
        public async Task<string> GetsaleinJob_byJobId(Guid? jobid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Job.FirstOrDefaultAsync(x => x.JobID == jobid);

                return rs!.Salecode!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }
        public string GetTypeByMBLIDNormal(Guid? mblid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var mbl = _context.MBL.FirstOrDefault(x => x.MblID == mblid);
                var rs = _context.Job.FirstOrDefault(x => x.JobID == mbl!.Jobid);
                return rs!.Loai!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }
        public async Task<List<(M_Job job, Guid mblid)>> GetListJobNoAndMBLID()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Job
                    .Join(_context.MBL,
                        job => job.JobID,
                        mbl => mbl.Jobid,
                        (job, mbl) => new { job, mbl.MblID })
                    .ToListAsync();
                return rs.Select(x => (x.job, x.MblID)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }
        public async Task<string> GetFLCByMBLID(Guid? mblid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var mbl = await _context.MBL.FirstOrDefaultAsync(x => x.MblID == mblid);
                var rs = await _context.Job.FirstOrDefaultAsync(x => x.JobID == mbl!.Jobid);
                return rs!.FLC!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }
        public string GetNameFLCByMBLID(Guid? mblid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var mbl = _context.MBL.FirstOrDefaultAsync(x => x.MblID == mblid);
                var rs = _context.Job.FirstOrDefaultAsync(x => x.JobID == mbl.Result!.Jobid);
                return rs.Result!.FLC!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }
        public string GetFLCByHBLID(Guid? hblid)
        {
            try
            {
                if (hblid is null) return null!;

                _context.ChangeTracker.Clear();

                // Lấy FLC theo đúng chain: HBL -> MBL -> Job
                var flc = (from h in _context.HBL
                           join m in _context.MBL on h.mblid equals m.MblID
                           join j in _context.Job on m.Jobid equals j.JobID
                           where h.hblID == hblid
                           select j.FLC).FirstOrDefault();

                return flc ?? null!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }
        public async Task<List<M_Theodoilohang>> GetListTheoDoiLoHang(Guid? hblid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Theodoilohang.Where(x => x.outboundid == hblid).OrderByDescending(x => x.dateUpdate).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Theodoilohang>();
            }
        }
        public async Task<List<string>> GetListItemTheoDoiLoHang()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Theodoilohang.Select(X => X.Items).Distinct().ToListAsync();
            return rs;
        }

        public async Task<List<M_Theodoilohang>> GetListTheoDoiLoHang(DateRange dateRange)
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Theodoilohang.ToListAsync();
            rs = rs.Where(x => DateTime.Parse(x.Timer!) >= dateRange.Start && DateTime.Parse(x.Timer!) <= dateRange.End)
                .OrderBy(x => DateTime.Parse(x.Timer!)).ToList();
            return rs;
        }
        public async Task<BoolandMessReponse> UpdateTheoDoiLoHang(M_Theodoilohang c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Theodoilohang.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Follow Shipment Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Follow Shipment with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateTheoDoiLoHang(M_Theodoilohang c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.theodoilohangID = Guid.NewGuid();
                _context.Theodoilohang.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Follow Shipment Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Follow Shipment with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteTheoDoiLoHang(M_Theodoilohang c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.theodoilohangID == null || c?.theodoilohangID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Theodoilohang.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Follow Shipment with error code: " + ex.Message);
            }
        }
        public async Task<List<M_HBLViTriLoHang>> GetlistVitrilohang(Guid? hblid)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.HBLViTriLoHang.Where(x => x.HBLID == hblid).OrderByDescending(x => x.ngay).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_HBLViTriLoHang>();
            }
        }



        public async Task<Dictionary<string, List<M_HBLViTriLoHang>>> GetlistVitrilohangByMultipleHBLNos(List<string> hblNos)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var result = new Dictionary<string, List<M_HBLViTriLoHang>>();

                foreach (var hblNo in hblNos)
                {
                    if (!string.IsNullOrWhiteSpace(hblNo))
                    {
                        var locations = await _context.HBLViTriLoHang
                            .Where(x => x.HBLNo != null && x.HBLNo.ToLower().Contains(hblNo.Trim().ToLower()))
                            .OrderByDescending(x => x.ngay)
                            .ToListAsync();

                        result[hblNo.Trim()] = locations;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new Dictionary<string, List<M_HBLViTriLoHang>>();
            }
        }

        private static readonly string[] UnitsMapEng = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
        "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };

        private static readonly string[] TensMapEng = { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

        private static readonly string[] UnitsMapVn = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín", "mười",
        "mười một", "mười hai", "mười ba", "mười bốn", "mười lăm", "mười sáu", "mười bảy", "mười tám", "mười chín" };

        private static readonly string[] TensMapVn = { "", "", "hai mươi", "ba mươi", "bốn mươi", "năm mươi", "sáu mươi", "bảy mươi", "tám mươi", "chín mươi" };

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
        public async Task<string> GetDebitNo(Guid MBLDetailID)
        {
            var value = DateTime.Now;
            var rs = asv.GetUserDetail();
            var refno = await supsv.GetRefNoByFunc("Debit", value.Month, value.Year, rs.Usr!);
            var tiepdaungu = await supsv.GetTiepDauNguByFunc("JOB");
            var cn = rs.Branch;
            var flc = await GetFLCByMBLID(MBLDetailID);
            var type = await GetTypeByMBLID(MBLDetailID);
            var debitno = cn + tiepdaungu + type + flc + value.ToString("yyMM") + (refno.HasValue ? refno.Value.ToString("D4") : "RefNoError!");
            return debitno;
        }
        public async Task<string> GetInvoiceNo()
        {
            var value = DateTime.Now;
            var rs = asv.GetUserDetail();
            var refno = await supsv.GetRefNoByFunc("Invoice", value.Month, value.Year, rs.Usr!);
            var invoiceno = "HD" + value.ToString("yyMM") + (refno.HasValue ? refno.Value.ToString("D4") : "RefNoError!");
            return invoiceno;
        }
        public async Task<bool> isExistInvoice(List<M_Debit> list)
        {
            // Lấy danh sách mã hoadondaura từ listdebit
            var soHoaDonList = list
                .Where(x => !string.IsNullOrWhiteSpace(x.sohoadondaura))
                .Select(x => x.sohoadondaura)
                .Distinct()
                .ToList();

            // Kiểm tra xem có tồn tại trong bảng HoaDonDauRa không
            _context.ChangeTracker.Clear();
            var listhd = await _context.HoaDonDauRa.Select(x => x.sohoadonNoibo).Distinct().ToListAsync();
            bool isExist = listhd.Any(sohd => soHoaDonList.Contains(sohd));
            return isExist;
        }

        public async Task<List<M_MBL>> GetListMBLDateRange(DateRange dateRange)
        {

            _context.ChangeTracker.Clear();
            var rs = await _context.MBL.ToListAsync();
            rs = rs.Where(x => DateTime.Parse(x.DateUpdate!) >= dateRange.Start && DateTime.Parse(x.DateUpdate!) <= dateRange.End)
                .OrderByDescending(x => DateTime.Parse(x.DateUpdate!)).ToList();
            return rs;
        }
        public async Task<List<M_HBL>> GetListHBLDateRange(DateRange dateRange)
        {
            _context.ChangeTracker.Clear();

            AuthUser user = new AuthUser();
            user = asv.GetUserDetail();
            if (user.Department == "ADMIN")
            {
                var rs = await _context.HBL.ToListAsync();
                rs = rs.Where(x => x.datereport >= dateRange.Start && x.datereport <= dateRange.End)
                    .OrderByDescending(x => x.dateupdate).ToList();
                return rs;
            }
            else
            {
                var rs = await _context.HBL.ToListAsync();
                rs = rs.Where(x => x.datereport >= dateRange.Start && x.datereport <= dateRange.End && (x.SaleName == user.Name))
                    .OrderByDescending(x => x.dateupdate).ToList();
                return rs;

            }


        }
        public async Task<List<M_Debit>> GetListDebitDateRange(DateRange dateRange)
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Debit.ToListAsync();
            rs = rs.Where(x => DateTime.Parse(x.dateupdate!) >= dateRange.Start && DateTime.Parse(x.dateupdate!) <= dateRange.End)
                .OrderByDescending(x => DateTime.Parse(x.dateupdate!)).ToList();
            return rs;
        }
        public async Task<List<M_Credit>> GetListCreditDateRange(DateRange dateRange)
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Credit.ToListAsync();
            rs = rs.Where(x => DateTime.Parse(x.dateupdate!) >= dateRange.Start && DateTime.Parse(x.dateupdate!) <= dateRange.End)
                .OrderByDescending(x => DateTime.Parse(x.dateupdate!)).ToList();
            return rs;
        }
        public async Task<List<M_Credit>> GetListCredit_sodntt(string sodntt)
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Credit.ToListAsync();
            rs = rs.Where(x => x.sodntt == sodntt && x.DNTT == true)
              .ToList();
            return rs;
        }


        public async Task<BoolandMessReponse> ExportProfitHBL(M_HBL hblinfo, string cur_type)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportProfitHBL.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                report.Culture = "en-US";
                var fcl = await GetFLCByMBLID(hblinfo.mblid); //
                var mblinfo = await GetMBL_byHBLid(hblinfo.mblid);
                var infojob = await GetJob_byid(mblinfo.Jobid);
                if (fcl == "F")
                {
                    report.Dictionary.Variables["volume"].Value = hblinfo.say;
                }
                else
                {
                    report.Dictionary.Variables["volume"].Value = infojob.Loai.Contains("A")
                        ? (hblinfo.Air_NoOfPiecesRCP + " / " + hblinfo.Air_GrossWieght + " / " + hblinfo.Air_NatureAndquantityOfgoods)
                        : (hblinfo.NoOfPackages + " / " + hblinfo.gross + " / " + hblinfo.cbm);
                }
                report.Dictionary.Variables["Cur_type"].Value = cur_type;
                report.Dictionary.Variables["Type"].Value = infojob.Loai;
                report.Dictionary.Variables["Refno"].Value = infojob.JobNo;
                report.Dictionary.Variables["HBLID"].Value = hblinfo.hblID.ToString();
                var list_debit = await GetListDebitHBL(hblinfo.hblID);
                var list_credit = await GetListCreditHBL(hblinfo.hblID);
                var list_cont = GetListContainerHBL(hblinfo.hblID).Select(x => x.CONTAINER_NO).Distinct();
                var querydebit = "WHERE debitId IN (";
                foreach (var item in list_debit)
                    querydebit += $"'{item.debitId}',";
                if (list_debit.Any())
                    querydebit = querydebit.TrimEnd(',') + ")";
                else
                    querydebit = "WHERE 1=0";

                var querycredit = "WHERE creditId IN (";
                foreach (var item in list_credit)
                    querycredit += $"'{item.creditid}',";
                if (list_credit.Any())
                    querycredit = querycredit.TrimEnd(',') + ")";
                else
                    querycredit = "WHERE 1=0";
                report.Dictionary.Variables["debitnos"].Value = querydebit;
                report.Dictionary.Variables["creditnos"].Value = querycredit;
                report.Dictionary.Variables["Containers"].Value = string.Join(";", list_cont);
                report.Render();
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

        public async Task<BoolandMessReponse> ExportProfitMBL(M_MBL mblinfo, string cur_type)
        {
            try
            {
                var report = new StiReport();
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "ReportProfitMBL.mrt");
                StiBlazorHelper.Initialize(JSRuntime);
                report = StiReport.CreateNewReport();
                report.Load(rpt);
                report.Culture = "en-US";
                var fcl = await GetFLCByMBLID(mblinfo.MblID); //
                var infojob = await GetJob_byid(mblinfo.Jobid);
                if (fcl == "F")
                {
                    report.Dictionary.Variables["volume"].Value = mblinfo.Say;
                }
                else
                {

                    report.Dictionary.Variables["volume"].Value = infojob.Loai.Contains("A")
                        ? (mblinfo.Air_NoOfPiecesRCP + " / " + mblinfo.Air_GrossWieght + " / " + mblinfo.Air_NatureAndquantityOfgoods)
                        : (mblinfo.NoOfPackages + " / " + mblinfo.Gross + " / " + mblinfo.CBM);
                }
                report.Dictionary.Variables["Cur_type"].Value = cur_type;
                report.Dictionary.Variables["Type"].Value = infojob.Loai;
                report.Dictionary.Variables["Refno"].Value = infojob.JobNo;
                report.Dictionary.Variables["MBLID"].Value = mblinfo.MblID.ToString();

                var hblinfos = await GetListHBL(mblinfo.MblID);
                var list_debit = new List<M_Debit>();
                var list_credit = new List<M_Credit>();
                var list_cont = new List<string>();
                foreach (var hbl in hblinfos)
                {
                    var debits = await GetListDebitHBL(hbl.hblID);
                    list_debit.AddRange(debits);
                    var credits = await GetListCreditHBL(hbl.hblID);
                    list_credit.AddRange(credits);
                    var cons = GetListContainerHBL(hbl.hblID).Select(x => x.CONTAINER_NO).Distinct();
                    list_cont.AddRange(cons);
                }
                var querydebit = "WHERE debitId IN (";
                foreach (var item in list_debit)
                    querydebit += $"'{item.debitId}',";
                if (list_debit.Any())
                    querydebit = querydebit.TrimEnd(',') + ")";
                else
                    querydebit = "WHERE 1=0";

                var querycredit = "WHERE creditId IN (";
                foreach (var item in list_credit)
                    querycredit += $"'{item.creditid}',";
                if (list_credit.Any())
                    querycredit = querycredit.TrimEnd(',') + ")";
                else
                    querycredit = "WHERE 1=0";
                report.Dictionary.Variables["debitids"].Value = querydebit;
                report.Dictionary.Variables["creditids"].Value = querycredit;
                report.Dictionary.Variables["Containers"].Value = string.Join(";", list_cont);
                report.Render();
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
        public async Task<List<string>> GetListTypeInDebitCredit()
        {
            _context.ChangeTracker.Clear();
            var rsdebit = await _context.Debit.Select(x => x.type).Distinct().ToListAsync();
            var rscredit = await _context.Credit.Select(x => x.type).Distinct().ToListAsync();
            var rs = rsdebit
                .Union(rscredit)
                .Distinct()
                .ToList();
            return rs!;
        }

        async Task<string?> UpdateHBLNo(M_MBL MBLDetail, string? usr)
        {
            var value = DateTime.Now;
            var refno = await supsv.GetRefNoByFunc("HBL", value.Month, value.Year, usr);
            var polcode = MBLDetail!.Polcode != null ? MBLDetail!.Polcode!.Substring(MBLDetail.Polcode.Length - 3) : "";
            var podcode = MBLDetail!.Podcode != null ? MBLDetail!.Podcode!.Substring(MBLDetail.Podcode.Length - 3) : "";
            var hbl = polcode + podcode + value.ToString("yyMM") + (refno.HasValue ? refno.Value.ToString("D5") : "RefNoError!");
            return hbl;
        }

        public async Task<List<M_Debit>> GetListDebit_FromQuo()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Debit
                    .Where(x => x.quotationid != null && x.quotationid != Guid.Empty && x.continued == true).ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Debit>();
            }
        }
        public async Task<List<M_Credit>> GetListCredit_FromQuo()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Credit
                    .Where(x => x.quotationid != null && x.quotationid != Guid.Empty && x.continued == true).ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Credit>();
            }
        }
        public async Task<List<M_Credit>> GetListCrebit_FromQuo()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Credit
                    .Where(x => x.quotationid != null && x.quotationid != Guid.Empty && x.continued == true).ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Credit>();
            }
        }

        public async Task<List<M_HBL>> GetListHBLALL_bysale()
        {
            try
            {
                //_context.ChangeTracker.Clear();
                //var rs = await _context.HBL.ToListAsync();
                //return rs;

                AuthUser user = new AuthUser();
                user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();
                List<M_HBL> rs = new();

                if (user.Department == "ADMIN")
                {
                    rs = await _context.HBL.ToListAsync();
                }
                else
                {

                    rs = await _context.HBL
                        .Where(x => x.SaleName == user.Name)
                        .ToListAsync();
                }

                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_HBL>();
            }
        }

        public async Task<List<M_HoaDonDauRa>> GetListHoaDonDauRaALL_bysale()
        {
            AuthUser user = asv.GetUserDetail();
            // use isolated context to prevent disposed / parallel reader issues
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            ctx.ChangeTracker.Clear();
            IQueryable<M_HoaDonDauRa> baseQuery = ctx.HoaDonDauRa.AsNoTracking().Where(x => x.continued == true);

            if (user.Department != "ADMIN")
            {
                baseQuery = from h in ctx.HoaDonDauRa.AsNoTracking()
                            join m in ctx.HBL.AsNoTracking() on h.hblid equals m.hblID
                            where h.continued == true && m.hblID != Guid.Empty && m.SaleName == user.Name
                            select h;
            }

            return await baseQuery.ToListAsync();
        }

        public async Task<List<M_Debit>> GetListDebitQuotation(List<M_Quotation> listQuo)
        {


            var QuoList = listQuo.Select(x => x.quotationID).ToList();
            var QuoInClause = string.Join(",", QuoList.Select(x => $"'{x}'"));


            var sql = $@"
                SELECT c.* 
                FROM Debit c 
                LEFT JOIN Quotation Quo ON c.quotationid = Quo.Quotationid 
                WHERE c.quotationid IN ({QuoInClause})";


            var rs = await _context.Debit
                .FromSqlRaw(sql)
                .AsNoTracking()
                .ToListAsync();
            return rs;

        }

        public async Task<List<M_Credit>> GetListCreditQuotation(List<M_Quotation> listQuo)
        {


            var QuoList = listQuo.Select(x => x.quotationID).ToList();
            var QuoInClause = string.Join(",", QuoList.Select(x => $"'{x}'"));


            var sql = $@"
                SELECT c.* 
                FROM Credit c 
                LEFT JOIN Quotation Quo ON c.quotationid = Quo.Quotationid 
                WHERE c.quotationid IN ({QuoInClause})";


            var rs = await _context.Credit
                .FromSqlRaw(sql)
                .AsNoTracking()
                .ToListAsync();
            return rs;

        }

        public async Task<List<M_HoaDonDauRa>> GetList_HoaDonDauRa()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.HoaDonDauRa.ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_HoaDonDauRa>();
            }
        }
        public async Task<List<M_HoaDonDauVao>> GetList_HoaDonDauVao()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.HoaDonDauVao.ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_HoaDonDauVao>();
            }
        }
        public async Task<List<M_HoaDonDauRa>> GetList_HoaDonDauRa_From_HBLID(List<M_HBL> listHbl)
        {
            try
            {
                _context.ChangeTracker.Clear();

                // Lấy danh sách các hblid từ listHbl (Guid không nullable nên không cần check null)
                var hblIds = listHbl
                    .Select(h => h.hblID)
                    .ToList();

                if (hblIds == null || !hblIds.Any())
                    return new List<M_HoaDonDauRa>();

                var rs = await (from hd in _context.HoaDonDauRa
                                join id in hblIds on hd.hblid equals id
                                where hd.continued == true
                                select hd)
                       .ToListAsync();

                // Lấy mỗi customer_id duy nhất và giữ lại hóa đơn đầu tiên của mỗi nhóm
                var distinctByCustomer = rs
                    .GroupBy(x => x.customerid)
                    .Select(g => g.First())
                    .ToList();

                // Nếu bạn muốn trả distinct theo customerid thì trả distinctByCustomer
                // Nếu muốn trả tất cả thì trả rs

                return distinctByCustomer;
            }
            catch (Exception ex)
            {
                // Bạn có thể log ex.Message ở đây nếu muốn
                return new List<M_HoaDonDauRa>();
            }
        }


        public async Task<List<M_CongNoHoaDonDauRa>> GetList_CongNoHoaDonDauRa()
        {

            _context.ChangeTracker.Clear();
            var rs = await _context.CongNoHoaDonDauRa.ToListAsync();
            return rs;
        }
        public async Task<List<M_CongNoHoaDonDauVao>> GetList_CongNoHoaDonDauVao()
        {

            _context.ChangeTracker.Clear();
            var rs = await _context.CongNoHoaDonDauVao.ToListAsync();
            return rs;
        }

        // Container Movement methods
        public List<M_ContainerMovement> GetContainerMovementByContainerId(Guid ctnId)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.ContainerMovement.Where(x => x.CTN_ID == ctnId).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_ContainerMovement>();
            }
        }

        public async Task<BoolandMessReponse> SaveOrUpdateContainerMovement(M_ContainerMovement movement)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (movement.MovementId == Guid.Empty)
                {
                    movement.MovementId = Guid.NewGuid();
                    movement.CreatedDate = DateTime.Now;
                    movement.CreatedBy = asv.GetAuth().Result.User.Identity!.Name;
                    _context.ContainerMovement.Add(movement);
                }
                else
                {
                    _context.ContainerMovement.Update(movement);
                }
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Save Container Movement Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Save Container Movement with error: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteContainerMovement(M_ContainerMovement movement)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (movement?.MovementId == null || movement.MovementId == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context.ContainerMovement.Remove(movement);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Container Movement with error: " + ex.Message);
            }
        }

        // Container Seal methods
        public List<M_ContainerSeal> GetContainerSealByContainerId(Guid ctnId)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.ContainerSeal.Where(x => x.CTN_ID == ctnId).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_ContainerSeal>();
            }
        }

        public async Task<BoolandMessReponse> SaveOrUpdateContainerSeal(M_ContainerSeal seal)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (seal.SealId == Guid.Empty)
                {
                    seal.SealId = Guid.NewGuid();
                    seal.CreatedDate = DateTime.Now;
                    seal.CreatedBy = asv.GetAuth().Result.User.Identity!.Name;
                    _context.ContainerSeal.Add(seal);
                }
                else
                {
                    _context.ContainerSeal.Update(seal);
                }
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Save Container Seal Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Save Container Seal with error: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteContainerSeal(M_ContainerSeal seal)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (seal?.SealId == null || seal.SealId == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context.ContainerSeal.Remove(seal);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Container Seal with error: " + ex.Message);
            }
        }

        // Container Damage methods
        public List<M_ContainerDamage> GetContainerDamageByContainerId(Guid ctnId)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.ContainerDamage.Where(x => x.CTN_ID == ctnId).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_ContainerDamage>();
            }
        }

        public async Task<BoolandMessReponse> SaveOrUpdateContainerDamage(M_ContainerDamage damage)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (damage.DamageId == Guid.Empty)
                {
                    damage.DamageId = Guid.NewGuid();
                    damage.CreatedDate = DateTime.Now;
                    damage.CreatedBy = asv.GetAuth().Result.User.Identity!.Name;
                    _context.ContainerDamage.Add(damage);
                }
                else
                {
                    damage.LastUpdatedDate = DateTime.Now;
                    damage.LastUpdatedBy = asv.GetAuth().Result.User.Identity!.Name;
                    _context.ContainerDamage.Update(damage);
                }
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Save Container Damage Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Save Container Damage with error: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteContainerDamage(M_ContainerDamage damage)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (damage?.DamageId == null || damage.DamageId == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context.ContainerDamage.Remove(damage);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Container Damage with error: " + ex.Message);
            }
        }

        // GateIn methods
        public List<M_GateIn> GetGateInByContainerId(Guid containerId)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.GateIn.Where(x => x.Containerid == containerId).OrderBy(x=>x.Seq).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_GateIn>();
            }
        }

        public async Task<BoolandMessReponse> SaveOrUpdateGateIn(M_GateIn gateIn)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (gateIn.GateinID == Guid.Empty)
                {
                    gateIn.GateinID = Guid.NewGuid();
                    gateIn.InsDate = DateTime.Now;
                    _context.GateIn.Add(gateIn);
                }
                else
                {
                    _context.GateIn.Update(gateIn);
                }
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Save Gate In Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Save Gate In with error: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteGateIn(M_GateIn gateIn)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (gateIn?.GateinID == null || gateIn.GateinID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context.GateIn.Remove(gateIn);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Gate In with error: " + ex.Message);
            }
        }

        // GateOut methods
        public List<M_GateOut> GetGateOutByContainerId(Guid containerId)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.GateOut.Where(x => x.Containerid == containerId).OrderBy(x => x.Seq).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_GateOut>();
            }
        }

        public Dictionary<Guid, DateTime?> GetLatestGateInDates(IEnumerable<Guid> containerIds)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var ids = (containerIds ?? Array.Empty<Guid>()).Where(x => x != Guid.Empty).Distinct().ToHashSet();
                if (ids.Count == 0) return new Dictionary<Guid, DateTime?>();

                // Use a "simple SQL" projection first to avoid provider generating complex SQL (e.g., WITH/CTE)
                var rows = _context.GateIn
                    .Select(g => new { g.Containerid, g.DateIn })
                    .ToList();

                return rows
                    .Where(x => ids.Contains(x.Containerid))
                    .GroupBy(x => x.Containerid)
                    .ToDictionary(g => g.Key, g => g.Max(x => x.DateIn));
            }
            catch
            {
                return new Dictionary<Guid, DateTime?>();
            }
        }

        public Dictionary<Guid, DateTime?> GetLatestGateOutDates(IEnumerable<Guid> containerIds)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var ids = (containerIds ?? Array.Empty<Guid>()).Where(x => x != Guid.Empty).Distinct().ToHashSet();
                if (ids.Count == 0) return new Dictionary<Guid, DateTime?>();

                // Use a "simple SQL" projection first to avoid provider generating complex SQL (e.g., WITH/CTE)
                var rows = _context.GateOut
                    .Select(g => new { g.Containerid, g.DateOut })
                    .ToList();

                return rows
                    .Where(x => ids.Contains(x.Containerid))
                    .GroupBy(x => x.Containerid)
                    .ToDictionary(g => g.Key, g => g.Max(x => x.DateOut));
            }
            catch
            {
                return new Dictionary<Guid, DateTime?>();
            }
        }

        public async Task<BoolandMessReponse> SaveOrUpdateGateOut(M_GateOut gateOut)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (gateOut.GateOutID == Guid.Empty)
                {
                    gateOut.GateOutID = Guid.NewGuid();
                    _context.GateOut.Add(gateOut);
                }
                else
                {
                    _context.GateOut.Update(gateOut);
                }
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Save Gate Out Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Save Gate Out with error: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteGateOut(M_GateOut gateOut)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (gateOut?.GateOutID == null || gateOut.GateOutID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context.GateOut.Remove(gateOut);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Gate Out with error: " + ex.Message);
            }
        }

        public string CheckContainerNumber(string strNumber)
        {
            if (string.IsNullOrWhiteSpace(strNumber) || strNumber.Length < 11)
                return "Số Container không đúng.! (Invalid length or format)";

            string strHeader;
            string strBody;
            string strCD;
            try
            {
                strHeader = strNumber.Substring(0, 4);
                strBody = strNumber.Substring(4, 6);
                strCD = strNumber.Substring(10, 1).ToUpperInvariant();
            }
            catch
            {
                return "Số Container không đúng.! (Invalid length or format)";
            }

            int intCalcCD = 0;
            int intMult = 1;

            foreach (char cCharacter in strHeader)
            {
                int intTemp = cCharacter - 55; // A = 10
                if (intTemp > 9 && intTemp < 36)
                {
                    if (intTemp > 10) intTemp += 1;
                    if (intTemp > 21) intTemp += 1;
                    if (intTemp > 32) intTemp += 1;
                    intCalcCD += intTemp * intMult;
                }
                else
                {
                    return "Số Container không đúng.! (Container Number has non alphabetic character in Shipping Company Code)";
                }
                intMult *= 2;
            }

            if (strBody.All(char.IsDigit))
            {
                foreach (char cCharacter in strBody)
                {
                    int intNum = cCharacter - '0';
                    intCalcCD += intNum * intMult;
                    intMult *= 2;
                }
            }
            else
            {
                return "Số Container không đúng.! (Non numeric character in container identifier)";
            }

            if (!int.TryParse(strCD, out int intCD))
                return "Số Container không đúng.! (Container Number has non numeric Check Digit)";

            intCalcCD = intCalcCD % 11;
            if (intCalcCD == 10) intCalcCD = 0;

            if (intCalcCD == intCD)
                return "Container Number is OK!";
            return "Số Container không đúng.! (Container Number has incorrect Check Digit)";
        }
    }
}