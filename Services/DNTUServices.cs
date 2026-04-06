using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Org.BouncyCastle.Asn1.Ocsp;
using Stimulsoft.Report;
using Stimulsoft.Report.Blazor;
using System.Globalization;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;

namespace NVOAMASIS.Services
{
    public class DNTUServices(AppDbContext _context, IWebHostEnvironment _env, AccountService asv, IJSRuntime JSRuntime)
    {
        public async Task<List<M_DNTU>> GetList_DNTU()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_DNTU> rs = new();

                rs = _context.DeNghiTamUng.OrderByDescending(x => x.So).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_DNTU>();
            }
        }


        public async Task<List<M_DNTT_Logistics>> GetList_DNTT_approved()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_DNTT_Logistics> rs = new();

                rs = _context.DNTT_Logistics.Where(x=>x.Approve==true).OrderByDescending(x => x.So).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_DNTT_Logistics>();
            }
        }

        public async Task<List<M_Duyet_DNTT>> GetList_Duyet_DNTT()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_Duyet_DNTT> rs = new();

                rs = _context.Duyet_DNTT.OrderByDescending(x => x.So).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_Duyet_DNTT>();
            }
        }

        public List<M_DNHU> GetListDNHU()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.deNghiHoanUng.Where(x => x.Continued == true).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_DNHU>();
            }
        }

        public List<M_ChiTietBieuGiaLuuKho> GetListChitietBGLuuKho()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.chiTietBieuGiaLuuKho.Where(x => x.Continued == true).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_ChiTietBieuGiaLuuKho>();
            }
        }
        public M_DNTU? GetDNTUById(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var dntu = _context.DeNghiTamUng.FirstOrDefault(x => x.Id == id);
                return dntu;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public List<M_DNHU> GetListDNHU_by_idTU(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.deNghiHoanUng.Where(x => x.DeNghiTamUngID == id).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_DNHU>();
            }
        }
        public List<M_ChiTietBieuGiaLuuKho> GetListCTLK_by_idTU(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.chiTietBieuGiaLuuKho.Where(x => x.BieuGiaLuuKhoID == id).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_ChiTietBieuGiaLuuKho>();
            }
        }

        public List<M_DNTU> GetListDNTU()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.DeNghiTamUng.Where(x => x.Continued == true).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_DNTU>();
            }
        }
        public async Task<BoolandMessReponse> CreateDeNghiTamUng_Detail(M_DNTU c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();
                _context.DeNghiTamUng.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Advance Request List Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Advance Request List with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateDeNghiTamUng(M_DNTU IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Advance Request Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Advance Request Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Advance Request Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> DeleteDeNghiTamUng_Detail(M_DNTU c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.DeNghiTamUng.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateDeNghiTamUng_Detail(M_DNTU c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.DeNghiTamUng.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Advance Request  Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Advance Request  with error code: " + ex.Message);
            }
        }

        public async Task<List<M_DNTU>> GetListDeNghiTamUng_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.DeNghiTamUng.Where(_ => _.Id == id).ToListAsync();
            return Invoices;
        }

        //--------------------------------------------------------------
        public async Task<BoolandMessReponse> CreateDeNghiHoanUng_Detail(M_DNHU c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.DeNghiHoanUngID = Guid.NewGuid();
                _context.deNghiHoanUng.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Refund Request List Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Refund Request List with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreatedeNghiHoanUng(M_DNHU IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.DeNghiHoanUngID == null || IV.DeNghiHoanUngID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Refund Request Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Refund Request Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Refund Request Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> DeletedeNghiHoanUng_Detail(M_DNHU c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.DeNghiHoanUngID == null || c?.DeNghiHoanUngID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.deNghiHoanUng.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdatedeNghiHoanUng_Detail(M_DNHU c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.deNghiHoanUng.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Refund Request  Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Refund Request  with error code: " + ex.Message);
            }
        }

        public async Task<List<M_DNHU>> GetListdeNghiHoanUng_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.deNghiHoanUng.Where(_ => _.DeNghiHoanUngID == id).ToListAsync();
            return Invoices;
        }
        //--------------------------------------------------------------
        public async Task<BoolandMessReponse> CreateDuyet_DNTT_Detail(M_Duyet_DNTT c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();
                _context.Duyet_DNTT.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Approve Payment Request List Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Approve Payment Request List with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateDuyet_DNTT(M_Duyet_DNTT IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Approve Payment Request Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Approve Payment Request Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Refund Request Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> DeleteDuyet_DNTT_Detail(M_Duyet_DNTT c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Duyet_DNTT.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateDuyet_DNTT_Detail(M_Duyet_DNTT c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Duyet_DNTT.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Approve Payment Request Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Approve Payment Request with error code: " + ex.Message);
            }
        }

        public async Task<List<M_Duyet_DNTT>> GetListDuyet_DNTT_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.Duyet_DNTT.Where(_ => _.Id == id).ToListAsync();
            return Invoices;
        }

        //--------------------------------------------------------------
        public async Task<BoolandMessReponse> CreatechiTietBieuGiaLuuKho_Detail(M_ChiTietBieuGiaLuuKho c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.ChiTietBieuGiaLuuKhoID = Guid.NewGuid();
                _context.chiTietBieuGiaLuuKho.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Warehouse price list details Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Warehouse price list details with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreatchiTietBieuGiaLuuKho(M_ChiTietBieuGiaLuuKho IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.ChiTietBieuGiaLuuKhoID == null || IV.ChiTietBieuGiaLuuKhoID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Warehouse price list details Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Warehouse price list details Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Warehouse price list details Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> DeletchiTietBieuGiaLuuKho_Detail(M_ChiTietBieuGiaLuuKho c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.ChiTietBieuGiaLuuKhoID == null || c?.ChiTietBieuGiaLuuKhoID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.chiTietBieuGiaLuuKho.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdatechiTietBieuGiaLuuKho_Detail(M_ChiTietBieuGiaLuuKho c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.chiTietBieuGiaLuuKho.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Warehouse price list details Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Warehouse price list details with error code: " + ex.Message);
            }
        }

        public async Task<List<M_ChiTietBieuGiaLuuKho>> GetListchiTietBieuGiaLuuKhog_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.chiTietBieuGiaLuuKho.Where(_ => _.ChiTietBieuGiaLuuKhoID == id).ToListAsync();
            return Invoices;
        }

        public double SumHoanung(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();

                double total = _context.deNghiHoanUng
                    .Where(x => x.DeNghiTamUngID == id)
                    .Sum(x => (double?)x.Sotien) ?? 0;

                return total;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public M_DNTU? Get_DNTU_byID(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var invoice = _context.DeNghiTamUng.FirstOrDefault(_ => _.Id == id);
            return invoice;
        }

        public M_DNTT_Logistics? Get_DNTT_byID(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var invoice = _context.DNTT_Logistics.FirstOrDefault(_ => _.Id == id);
            return invoice;
        }

        public M_DNTT_Logistics? Get_DNTT_BY_id(Guid? sodntt)
        {
            _context.ChangeTracker.Clear();
            var invoice = _context.DNTT_Logistics.FirstOrDefault(_ => _.Id == sodntt);
            return invoice;
        }

        public async Task<List<M_DNTU>> GetList_DNTU_bydate(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_DNTU> rs = new();

                rs = _context.DeNghiTamUng.OrderByDescending(x => x.So)
                    .Where(x=> (!fromDate.HasValue || x.NGAY >= fromDate.Value) && (!toDate.HasValue || x.NGAY <= toDate.Value))
                    .ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_DNTU>();
            }
        }

        public async Task<M_DNHU?> Get_DNHU_byID_DNTU(Guid? idDNTU)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var result = _context.deNghiHoanUng
                    .Where(x => x.DeNghiTamUngID == idDNTU)
                    .OrderByDescending(x => x.So)
                    .FirstOrDefault();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<List<M_DNTU>> GetList_DNTU_bymonths(int fromthang, int fromnam, int tothang, int tonam)
        {
            double? total_debit_vnd;
            double? total_debit_usd;
            var fromDate = new DateTime(fromnam, fromthang, 1);
            var toDate = new DateTime(tonam, tothang, 1).AddMonths(1).AddDays(-1);

            var RPT_Quos = await _context.DeNghiTamUng
                             .Where(x => (x.NGAY >= fromDate && x.NGAY <= toDate))
                             .ToListAsync();

            return RPT_Quos;

        }

        //---------------------------DNTT Logistics-------------------------
        public async Task<List<M_HBL>> GetListHBL_ByCus(Guid? cusid)
        {
            try
            {
                _context.ChangeTracker.Clear();

                //var rs = await (from hbl in _context.HBL
                //                join credit in _context.Credit on hbl.hblID equals credit.hblid
                //                where credit.customerid == cusid
                //                select hbl).DistinctBy(x => x.hblID).ToListAsync();
                var rs = await (from hbl in _context.HBL
                                join credit in _context.Credit on hbl.hblID equals credit.hblid
                                where credit.customerid == cusid
                                group hbl by hbl.hblID into g
                                select g.FirstOrDefault())
                .ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<M_HBL>> GetListHBL_Bydate(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                _context.ChangeTracker.Clear();

                //var rs = await (from hbl in _context.HBL
                //                join credit in _context.Credit on hbl.hblID equals credit.hblid
                //                where credit.customerid == cusid
                //                select hbl).DistinctBy(x => x.hblID).ToListAsync();
                var rs = await (from hbl in _context.HBL
                                join credit in _context.Credit on hbl.hblID equals credit.hblid
                                where (!fromDate.HasValue || hbl.datereport >= fromDate.Value) &&
                                      (!toDate.HasValue || hbl.datereport <= toDate.Value)
                                group hbl by hbl.hblID into g
                                select g.FirstOrDefault())
                .ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<M_HBL>> GetListHBL_Byjob(Guid? jobid)
        {
            try
            {
                _context.ChangeTracker.Clear();


                var rs = await (
                    from job in _context.Job
                    join mbl in _context.MBL on job.JobID equals mbl.Jobid
                    join hbl in _context.HBL on mbl.MblID equals hbl.mblid
                    join credit in _context.Credit on hbl.hblID equals credit.hblid

                    where ((job.JobID == jobid) && (credit.hblid == hbl.hblID))
                    group hbl by hbl.hblID into g
                    select g.FirstOrDefault())
                    .ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<M_HBL>> GetListHBL_Byjob_cus(Guid? jobid, Guid? cusid)
        {
            try
            {
                _context.ChangeTracker.Clear();


                var rs = await (
                     from job in _context.Job
                     join mbl in _context.MBL on job.JobID equals mbl.Jobid
                     join hbl in _context.HBL on mbl.MblID equals hbl.mblid
                     join credit in _context.Credit on hbl.hblID equals credit.hblid

                     where ((credit.hblid == hbl.hblID) && (credit.customerid == cusid) && (job.JobID == jobid))
                    group hbl by hbl.hblID into g
                    select g.FirstOrDefault())
                    .ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<M_HBL>> GetListHBL_ByHBL(Guid? HBLID)
        {
            try
            {
                _context.ChangeTracker.Clear();

                //var rs = await (from hbl in _context.HBL
                //                join credit in _context.Credit on hbl.hblID equals credit.hblid
                //                where credit.hblid == HBLID
                //                select hbl)
                //                 .DistinctBy(x => x.hblID).ToListAsync();
                var rs = await (from hbl in _context.HBL
                                join credit in _context.Credit on hbl.hblID equals credit.hblid
                                where credit.hblid == HBLID
                                group hbl by hbl.hblID into g
                                select g.FirstOrDefault())
                .ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<M_DNTT_Logistics> GetListDNTT_all()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.DNTT_Logistics.Where(x => x.Continued == true).OrderByDescending(x=>x.So).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_DNTT_Logistics>();
            }
        }
        public List<M_DNTT_Logistics> GetListDNTT_all_sodntt(string sodntt)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.DNTT_Logistics.Where(x => x.Continued == true && x.So.ToUpper() ==sodntt.ToUpper() ).OrderByDescending(x => x.So).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_DNTT_Logistics>();
            }
        }
        public List<M_DNTT_Logistics> GetListDNTT_all_approved()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.DNTT_Logistics.Where(x => x.Continued == true && x.Approve==true).OrderByDescending(x => x.So).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_DNTT_Logistics>();
            }
        }

        public List<M_DNTT_Logistics> GetListDNTT_all_Notapproved()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.DNTT_Logistics.Where(x => x.Continued == true && x.Approve == false).OrderByDescending(x => x.So).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_DNTT_Logistics>();
            }
        }

        public List<M_DNTT_Logistics> GetListDNTT( string type)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.DNTT_Logistics.Where(x => x.Continued == true && x.Type == type).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_DNTT_Logistics>();
            }
        }

        public async Task<BoolandMessReponse> CreateDNTT_Detail(M_DNTT_Logistics c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();
                _context.DNTT_Logistics.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Payment Request Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Payment Request with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateDNTT(M_DNTT_Logistics IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Payment Request Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Payment Request Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Payment Request Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> DeleteDNTT_Detail(M_DNTT_Logistics c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.DNTT_Logistics.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Payment Request with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateDNTT_Detail(M_DNTT_Logistics c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.DNTT_Logistics.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Payment Request Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Payment Request with error code: " + ex.Message);
            }
        }

        public async Task<List<M_DNTT_Logistics>> GetListDNTT_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.DNTT_Logistics.Where(_ => _.Id == id).ToListAsync();
            return Invoices;
        }

        public async Task<List<string>> GetListBKno_from_hbl(string distinctHBL)
        {
            _context.ChangeTracker.Clear();

            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(distinctHBL))
                return result;

            var hblList = distinctHBL
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(h => h.Trim())
                            .ToList();

            foreach (var hblNo in hblList)
            {
                var bkno = await _context.HBL
                                         .Where(x => x.hbl == hblNo && !string.IsNullOrEmpty(x.bkno))
                                         .Select(x => x.bkno)
                                         .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(bkno))
                {
                    result.Add(bkno);
                }
            }

            return result;
        }

        public async Task<BoolandMessReponse> ExportBill_DNTT(Guid id)
        {
            try
            {
                //Create empty report object
                var report = new StiReport();
                //Load report template
                var rpt = Path.Combine(_env.WebRootPath, "Reports", "PhieuDNTT.mrt");

                StiBlazorHelper.Initialize(JSRuntime);

                report = StiReport.CreateNewReport();

                report.Load(rpt);
                report.Culture = "en-US";
                report.Dictionary.Variables["ID"].Value = id.ToString();

                var list_PT = await GetListDNTT_id(id);
     

                double? sotien = 0;

                foreach (var item_PT in list_PT)
                {
                    report.Dictionary.Variables["sodntt"].Value = item_PT.So;
                }
                report.Dictionary.Variables["Total"].Value = (sotien ?? 0).ToString("#,##0.##");
    
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

        public async Task<List<M_DNTT_Logistics>> GetListDNTT_id(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.DNTT_Logistics.Where(x => x.Id == id).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_DNTT_Logistics>();
            }
        }

        public async Task<string> get_DNTTno_byid(Guid? id)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var dnttNo = await _context.DNTT_Logistics
                    .Where(x => x.Id == id)
                    .Select(x => x.So) 
                    .FirstOrDefaultAsync();
                return dnttNo;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
