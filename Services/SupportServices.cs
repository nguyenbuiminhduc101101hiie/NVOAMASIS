using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using OfficeOpenXml;
using SixLabors.ImageSharp.ColorSpaces;
using NVOAMASIS.Components.Charge.Pages;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using static MudBlazor.CategoryTypes;
using static Stimulsoft.Report.StiRecentConnections;
namespace NVOAMASIS.Services
{
    public class SupportServices(AppDbContext _context, AccountService asv, HistoryLogService HistoryLogService)
    {
        public async Task<List<M_Agency>> GetListAgency()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Agency.ToListAsync();
                return rs;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_Agency>();
            }
            
        }
        public async Task<List<PortModel>> GetListPort()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Port.Where(x=>x.show == true && x.APPROVE == true && x.dept != null).OrderByDescending(x=>x.UPDATETIME).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<PortModel>();
            }
            
        }
        public async Task<List<M_VesselSpace>> GetListVesselSpace()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.VesselSpace.OrderByDescending(x => x.Voy).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_VesselSpace>();
            }

        }
        public async Task<List<ChargeModel>> GetListCharge()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Charge.OrderByDescending(x => x.UPDATETIME).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<ChargeModel>();
            }

        }
        public async Task<List<ChargeTemplete>> GetListChargeTemplate(Guid ChargeID)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.ChargeTemplete.Where(x=>x.CHARGEID == ChargeID).OrderByDescending(x => x.Dateupdate).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<ChargeTemplete>();
            }

        }
        public async Task<List<ChargeModel>> GetListCharge(bool isCommission)
        {
            _context.ChangeTracker.Clear();
            var query = isCommission ? $"SELECT * FROM CHARGE WHERE CHARGE_CODE like '%COMMISSION%'" : $"SELECT * FROM CHARGE WHERE CHARGE_CODE not like '%COMMISSION%'";
            var rs = await _context.Charge.FromSqlRaw($"{query}").OrderByDescending(x => x.UPDATETIME).ToListAsync();
            return rs;
        }
        public async Task<List<ChargeModel>> GetListChargeDuty(bool isDuty)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var query = isDuty ? $"SELECT * FROM CHARGE WHERE CHARGE_CODE like '%Duty%'" : $"SELECT * FROM CHARGE WHERE CHARGE_CODE not like '%Duty%'";
                var rs = await _context.Charge.FromSqlRaw($"{query}").OrderByDescending(x => x.UPDATETIME).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<ChargeModel>();
            }

        }
        public async Task<List<string>> GetListDanhMucTaiKhoan()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.DanhMucTaiKhoan.OrderBy(x => x.Taikhoan).Select(x=>x.Taikhoan + "-" + x.Tentaikhoan).ToListAsync();
            return rs;

        }
        public async Task<List<ShippingLine>> GetListShippingLine()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.ShippingLine.OrderBy(x => x.SHIPPINGLINE).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<ShippingLine>();
            }

        }
        public async Task<BoolandMessReponse> DeletePortDetail(PortModel c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.PORT_ID == null || c?.PORT_ID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Port.Remove(c!);
                await _context?.SaveChangesAsync()!;
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete Port", "ListPort", c.PORT_ID, c.PORT, new { OldData = c });
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Port with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdatePortDetail(PortModel c, PortModel c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Port.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Port", "ListPort", c.PORT_ID, c.PORT, new { OldData = c_old ,NewData =c});
                return new BoolandMessReponse(true, "Update Port Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Port with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreatePortDetail(PortModel c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.PORT_ID = Guid.NewGuid();
                _context.Port.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Port", "ListPort", c.PORT_ID, c.PORT, c);
                return new BoolandMessReponse(true, "Create Port Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Port with error code: " + ex.Message);
            }
        }
        public async Task<List<TrangThai>> GetListTrangThai()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.TrangThai.OrderBy(x => x.Tieude).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<TrangThai>();
            }

        }

        public async Task<List<M_Ykien_HQCO>> GetListYkien_HQCO()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Ykien_HQCO.OrderBy(x => x.Ykien).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_Ykien_HQCO>();
            }

        }
        public async Task<BoolandMessReponse> DeleteTrangThai(TrangThai c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.TrangThai.Remove(c!);
                await _context?.SaveChangesAsync()!;
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete Status", "Status", c.Id, c.Tieude, new { OldData = c });
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Status with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateTrangThai(TrangThai c, TrangThai c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.TrangThai.Update(c);
                await _context.SaveChangesAsync();

                string usr = asv.GetAuth().Result.User.Identity!.Name!;

                await HistoryLogService.LogAsync(usr, "Update Status", "Status", c.Id,c.Tieude, new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update Status Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Status with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateTrangThai(TrangThai c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();
                _context.TrangThai.Add(c!);
                await _context.SaveChangesAsync();

                string usr = asv.GetAuth().Result.User.Identity!.Name!;

                await HistoryLogService.LogAsync(usr, "ADD Status", "Status", c.Id, c.Tieude,c);
                return new BoolandMessReponse(true, "Create Status Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Status with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteChargeDetail(ChargeModel c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.CHARGE_ID == null || c?.CHARGE_ID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Charge.Remove(c!);
                await _context?.SaveChangesAsync()!;
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete Charge", "ListCharge", c.CHARGE_ID, c.CHARGE, new { OldData = c });
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Charge with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateChargeDetail(ChargeModel c,ChargeModel c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Charge.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Charge", "ListCharge", c.CHARGE_ID, c.CHARGE, new { OldData = c_old ,NewData=c});
                return new BoolandMessReponse(true, "Update Charge Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Charge with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateChargeDetail(ChargeModel c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.CHARGE_ID = Guid.NewGuid();
                _context.Charge.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Charge", "ListCharge", c.CHARGE_ID, c.CHARGE, c);
                return new BoolandMessReponse(true, "Create Charge Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Charge with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteChargeTempleteDetail(ChargeTemplete c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.ChargeTemplete.Remove(c!);
                await _context?.SaveChangesAsync()!;
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete ChargeTemplete", "ListCharge", c.Id, "", new { OldData = c });
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete ChargeTemplete with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateChargeTempleteDetail(ChargeTemplete c,ChargeTemplete c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.ChargeTemplete.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update ChargeTemplete", "ListCharge", c.Id, "", new { OldData = c_old,NewData =c });
                return new BoolandMessReponse(true, "Update ChargeTemplete Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update ChargeTemplete with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateChargeTempleteDetail(ChargeTemplete c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();
                _context.ChargeTemplete.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD ChargeTemplete", "ListCharge", c.Id, "", c);
                return new BoolandMessReponse(true, "Create ChargeTemplete Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add ChargeTemplete with error code: " + ex.Message);
            }
        }

        public async Task<List<Department>> GetListDepartments()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Department.OrderBy(x => x.Code).ToListAsync();
                return rs;
            }
            catch(Exception ex)
            {
                return new List<Department>();
            }
        }
        public async Task<List<M_RefNo>> GetListRefNo()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.RefNo.ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_RefNo>();
            }
        }
        public async Task<BoolandMessReponse> InsertRefNo(int tuthang, int denthang, int nam, int soluongsoref)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var listinsert = new List<M_RefNo>();
                for(int j = tuthang; j <= denthang; j++)
                {
                    var checkreftontai = await _context.RefNo.Where(x => x.thang == j && x.nam == nam).ToListAsync();
                    int? startwith = 1;
                    if (checkreftontai.Count > 0)
                        startwith = checkreftontai.Max(x => x.ref_number)+1;
                    for (int i = 1; i <= soluongsoref; i++)
                    {
                        var item = new M_RefNo();
                        item.id = Guid.NewGuid();
                        item.thang = j;
                        item.nam = nam;
                        item.ref_number = startwith;
                        startwith++;
                        listinsert.Add(item);
                    }
                }
                _context.RefNo.AddRange(listinsert);
                await _context.SaveChangesAsync();
     

                return new BoolandMessReponse(true, "Insert Ref No Successfully!");
            }
            catch(Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Insert RefNo with Error Code: " + ex.Message);
            }
        } 
        public async Task<int?> GetRefNoByFunc(string func, int thang, int nam, string usr)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.RefNo.Where(x=> x.thang == thang && x.nam == nam).ToListAsync();
                if (rs.Count == 0)
                    return null;
                var item = new M_RefNo();
                if (func == "JOB")
                {
                    item = rs.Where(x => x.used_job == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_job = usr;
                    item!.used_job = true;
                }
                else if (func == "HBL")
                {
                    item = rs.Where(x => x.used_hbl == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_hbl = usr;
                    item!.used_hbl = true;
                }
                else if (func == "Pricing")
                {
                    item = rs.Where(x => x.used_pricing == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_pricing = usr;
                    item!.used_pricing = true;
                }
                else if (func == "ProductPrice")
                {
                    item = rs.Where(x => x.used_pricing == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_pricing = usr;
                    item!.used_pricing = true;
                }
                else if (func == "LenhDieuXe")
                {
                    item = rs.Where(x => x.used_LDX == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_LDX = usr;
                    item!.used_LDX = true;
                }
                else if (func == "YeuCauTrucking")
                {
                    item = rs.Where(x => x.used_YCTrucking == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_YCTrucking = usr;
                    item!.used_YCTrucking = true;
                }
                else if (func == "TKHQ")
                {
                    item = rs.Where(x => x.used_TKHQ == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_TKHQ = usr;
                    item!.used_TKHQ = true;
                }
                else if (func == "YeuCauTuVanHQ")
                {
                    item = rs.Where(x => x.used_YeuCauTuVanHQ == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_YeuCauTuVanHq = usr;
                    item!.used_YeuCauTuVanHQ = true;
                }
                else if (func == "BGNH")
                {
                    item = rs.Where(x => x.used_BGNH == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_BGNH = usr;
                    item!.used_BGNH = true;
                }
                else if (func == "BGRR_")
                {
                    item = rs.Where(x => x.used_BGRR == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_BGRR = usr;
                    item!.used_BGRR = true;
                }
                else if (func == "KDTV")
                {
                    item = rs.Where(x => x.used_KDTV == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_KDTV = usr;
                    item!.used_KDTV = true;
                }
                else if (func == "KTCL")
                {
                    item = rs.Where(x => x.used_KTCL == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_KTCL = usr;
                    item!.used_KTCL = true;
                }
                else if (func == "BGLK")
                {
                    item = rs.Where(x => x.used_BGLK == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_BGLK = usr;
                    item!.used_BGLK = true;
                }
                else if (func == "Invoice")
                {
                    item = rs.Where(x => x.used_Invoice == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_Invoice = usr;
                    item!.used_Invoice = true;
                }
                else if (func == "DNTU")
                {
                    item = rs.Where(x => x.used_DNTU == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_DNTU = usr;
                    item!.used_DNTU = true;
                }
                else if (func == "DNHU")
                {
                    item = rs.Where(x => x.used_DNHU == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_DNHU = usr;
                    item!.used_DNHU = true;
                }
                else if (func == "CTBGLK")
                {
                    item = rs.Where(x => x.used_ChitietLuuKho == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_ChitietLuuKho = usr;
                    item!.used_ChitietLuuKho = true;
                }
                else if (func == "Phieuthu")
                {
                    item = rs.Where(x => x.used_PT == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_PT= usr;
                    item!.used_PT = true;
                }
                else if (func == "Phieuchi")
                {
                    item = rs.Where(x => x.used_PC == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_PC = usr;
                    item!.used_PC = true;
                }
                else if (func == "PKT")
                {
                    item = rs.Where(x => x.used_PkT == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_PKT = usr;
                    item!.used_PkT = true;
                }
                else if (func == "RFQ")
                {
                    item = rs.Where(x => x.used_RFQ == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_RFQ = usr;
                    item!.used_RFQ = true;
                }
                else if (func == "RFQ_Log")
                {
                    item = rs.Where(x => x.used_RFQ_Log == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_RFQ_Log = usr;
                    item!.used_RFQ_Log = true;
                }
                else if (func == "QUO")
                {
                    item = rs.Where(x => x.used_QUO == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_QUO = usr;
                    item!.used_QUO = true;
                }
                else if (func == "DNTT")
                {
                    item = rs.Where(x => x.used_DNTT == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_DNTT = usr;
                    item!.used_DNTT = true;
                }
                else if (func == "Duyet_DNTT")
                {
                    item = rs.Where(x => x.used_DNTT == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_DNTT = usr;
                    item!.used_DNTT = true;
                }
                else if (func == "ProductPrice_Import")
                {
                    item = rs.Where(x => x.used_pricing_import == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_pricing_import = usr;
                    item!.used_pricing_import = true;
                }
                else if (func == "ProductPrice_Truck")
                {
                    item = rs.Where(x => x.used_pricing_truck == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_pricing_truck = usr;
                    item!.used_pricing_truck = true;
                }
                else if (func == "ProductPrice_KTCL")
                {
                    item = rs.Where(x => x.used_pricing_KTCL == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_pricing_KTCL = usr;
                    item!.used_pricing_KTCL = true;
                }
                else if (func == "ProductPrice_Customs")
                {
                    item = rs.Where(x => x.used_pricing_Customs == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_pricing_Customs = usr;
                    item!.used_pricing_Customs = true;
                }
                
                else if (func == "IssueReports")
                {
                    item = rs.Where(x => x.used_IssueRpt == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_IssueRpt = usr;
                    item!.used_IssueRpt = true;
                }
                else if (func == "CC")
                {
                    item = rs.Where(x => x.used_CC == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_CC = usr;
                    item!.used_CC = true;
                }
                else if (func == "OrderNo")
                {
                    item = rs.Where(x => x.used_Orderno == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_Orderno = usr;
                    item!.used_Orderno = true;
                }

                else if (func == "CamKet")
                {
                    item = rs.Where(x => x.used_CamKet == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_CamKet= usr;
                    item!.used_CamKet = true;
                }
                else if (func == "Account")
                {
                    item = rs.Where(x => x.used_ACC == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_ACC = usr;
                    item!.used_ACC = true;
                }

                else
                {
                    item = rs.Where(x => x.used_debit == null).OrderBy(x => x.ref_number).FirstOrDefault();
                    item!.userused_debit = usr;
                    item!.used_debit = true;
                }
                _context.RefNo.Update(item!);
                await _context.SaveChangesAsync();
                return item!.ref_number;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<List<string>> GetListSale()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.UserList.Where(x => x.Department == "SALES").Select(x => x.Name).ToListAsync();
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<string>();
            }
        }

        public async Task<List<string>> GetListUsers()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.UserList.Select(x => x.Name).ToListAsync();
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<string>();
            }
        }

        public async Task<string> GetTiepDauNguByFunc(string fucn)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.TiepDauNgu.Where(x=>x.Loai == fucn).Select(x=>x.Hangso).FirstOrDefaultAsync();
                return rs!;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Theo <paramref name="jobLoai"/> (= <see cref="M_Job.Loai"/>), lấy dòng TiepDauNgu và xem cột POL/POD có giá trị không
        /// để quyết định có ghép mã POL/POD (3 ký tự cuối từ MBL) vào số HBL hay không.
        /// </summary>
        /// <returns>(IncludePol, IncludePod): true nếu cột tương ứng trên TiepDauNgu có chuỗi khác rỗng.</returns>
        public async Task<(bool IncludePol, bool IncludePod)> GetPODPOL_BY_TiepDauNgu(string? jobLoai)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jobLoai))
                    return (false, false);

                _context.ChangeTracker.Clear();
                var row = await _context.TiepDauNgu
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Loai == jobLoai);
                if (row == null)
                    return (false, false);

                return (
                    !string.IsNullOrWhiteSpace(row.POL),
                    !string.IsNullOrWhiteSpace(row.POD));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return (false, false);
            }
        }

        public async Task<List<string>> GetlistVoy()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var voyhbl = await _context.HBL.Select(x => x.voy).ToListAsync();
                var voymbl = await _context.MBL.Select(x => x.Voy).ToListAsync();
                var ds = voyhbl;
                ds.AddRange(voymbl);
                var list = ds.Distinct().Where(x => x != null).ToList();
                return list!;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public async Task<List<string>> GetlistVessel()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var vhbl = await _context.HBL.Select(x => x.vessel).ToListAsync();
                var vmbl = await _context.MBL.Select(x => x.Vessel).ToListAsync();
                var ds = vhbl;
                ds.AddRange(vmbl);
                var list = ds.Distinct().Where(x=> x != null).ToList();
                return list!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public async Task<List<string>> GetlistCFS()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.HBL.Where(x=> x.Air_type != null).Select(x => x.Air_type).Distinct().ToListAsync();
            return rs!;
        }
        public async Task<List<string>> GetlistWhouse()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Terminal.Where(x => x.Continued != false && x.Code != "" && x.TermiNalName != "").Select(x => x.TermiNalName + "-" + x.Code).Distinct().ToListAsync();
            return rs!;
        }

        public async Task<List<HistoryLog>> Get_HistoryLog(MudBlazor.DateRange dateRange)
        {
            try
            {

 
                _context.ChangeTracker.Clear();

                List<HistoryLog> rs = new();

                rs = await _context.HistoryLogs
                       .OrderByDescending(x => x.Timestamp)
                       .Where(x=>x.Timestamp >= dateRange.Start && x.Timestamp <= dateRange.End ).ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<HistoryLog>();
            }
        }
        public async Task<List<HistoryLog>> Get_HistoryLog_search(string user,string form,string text_changes, MudBlazor.DateRange dateRange)
        {
            try
            {


                _context.ChangeTracker.Clear();

                List<HistoryLog> rs = new();

                 rs = await _context.HistoryLogs
               .Where(x =>
                   (string.IsNullOrEmpty(user) || x.UserName.ToLower() == user.ToLower()) &&
                (string.IsNullOrEmpty(form) || x.EntityName.ToLower() == form.ToLower()) &&
                   (string.IsNullOrEmpty(text_changes) || x.Changes.ToLower().Contains(text_changes.ToLower()) &&
                  x.Timestamp >= dateRange.Start && x.Timestamp <= dateRange.End))

               .OrderByDescending(x => x.Timestamp)
               .ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<HistoryLog>();
            }
        }

        public async Task<List<HistoryLog>> Get_HistoryLog_Form(string Form)
        {
            try
            {


                _context.ChangeTracker.Clear();

                List<HistoryLog> rs = new();

                rs = await _context.HistoryLogs
                        .Where(x => x.EntityName == Form)
                       .OrderByDescending(x => x.Timestamp).ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<HistoryLog>();
            }
        }

        public async Task<List<HistoryLog>> Get_HistoryLog_changes(string changes)
        {
            try
            {


                _context.ChangeTracker.Clear();

                List<HistoryLog> rs = new();

                rs = await _context.HistoryLogs
                    .Where(x => EF.Functions.Like(x.Changes, $"%{changes}%"))
                    .OrderByDescending(x => x.Timestamp)
                    .ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<HistoryLog>();
            }
        }

        public async Task<BoolandMessReponse> ImportChargesFromExcelAsync(Stream fileStream)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Đọc stream về memory để tránh lỗi synchronous read
                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var package = new ExcelPackage(memoryStream);
                var worksheet = package.Workbook.Worksheets[0];

                if (worksheet == null)
                    return new BoolandMessReponse(false, "Không tìm thấy sheet trong Excel!");

                int rowCount = worksheet.Dimension.Rows;
                int successCount = 0;
                var newCharges = new List<ChargeModel>();

                string currentUser = asv.GetAuth().Result.User?.Identity?.Name ?? "Unknown";

                for (int row = 3; row <= rowCount; row++) // Dữ liệu bắt đầu từ dòng 4
                {
                    var chargeCode = worksheet.Cells[row, 3].Text?.Trim(); // Cột C (3)

                    if (string.IsNullOrWhiteSpace(chargeCode))
                        continue;

                    
                    bool isDuplicate = await _context.Charge.AnyAsync(x => x.CHARGE_CODE == chargeCode);
                    if (isDuplicate)
                        continue;

                    var charge = new ChargeModel
                    {
                        CHARGE_ID = Guid.NewGuid(),
                        CHARGE = worksheet.Cells[row, 2].Text?.Trim(),             // Cột B
                        CHARGE_CODE = chargeCode,                                 // Cột C
                        DVT = worksheet.Cells[row, 4].Text?.Trim(),               // Cột D
                        unit = worksheet.Cells[row, 5].Text?.Trim(),              // Cột E
                        loaidichvu_charge = worksheet.Cells[row, 6].Text?.Trim(), // Cột F
                        tiengtrung = worksheet.Cells[row, 7].Text?.Trim(),        // Cột G
                        accselling = worksheet.Cells[row, 8].Text?.Trim(),        // Cột H
                        accbuying = worksheet.Cells[row, 9].Text?.Trim(),         // Cột I
                        tk1 = worksheet.Cells[row, 10].Text?.Trim(),              // Cột J
                        tk2 = worksheet.Cells[row, 11].Text?.Trim(),              // Cột K
                        tk3 = worksheet.Cells[row, 12].Text?.Trim(),              // Cột L

                        EDITABLE = true,
                        APPROVE = true,
                        CONTINUED = true,
                        USERID = currentUser,
                        UPDATETIME = DateTime.Now,
                    };

                    newCharges.Add(charge);
                    successCount++;
                }

                if (newCharges.Any())
                {
                    _context.Charge.AddRange(newCharges);
                    await _context.SaveChangesAsync();
                }

                return new BoolandMessReponse(true, $"Import thành công {successCount} dòng.");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Import thất bại: " + ex.Message);
            }
        }


        public async Task<List<ChargeIndex.ChargePreviewModel>> PreviewChargesFromExcelAsync(Stream fileStream)
        {
            var result = new List<ChargeIndex.ChargePreviewModel>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];

            int row = 3;
            while (true)
            {
                var chargeCode = worksheet.Cells[row, 2].Text?.Trim();
                if (string.IsNullOrEmpty(chargeCode)) break;

                var preview = new ChargeIndex.ChargePreviewModel
                {
                    CHARGE = worksheet.Cells[row, 2].Text?.Trim(),
                    CHARGE_CODE = worksheet.Cells[row, 3].Text?.Trim(),
                    DVT = worksheet.Cells[row, 4].Text?.Trim(),
                    Unit = worksheet.Cells[row, 5].Text?.Trim(),
                    LoaiDichVu_Charge = worksheet.Cells[row, 6].Text?.Trim(),
                    TiengTrung = worksheet.Cells[row, 7].Text?.Trim(),
                    AccSelling = worksheet.Cells[row, 8].Text?.Trim(),
                    AccBuying = worksheet.Cells[row, 9].Text?.Trim(),
                    TK1 = worksheet.Cells[row, 10].Text?.Trim(),
                    TK2 = worksheet.Cells[row, 11].Text?.Trim(),
                    TK3 = worksheet.Cells[row, 12].Text?.Trim()
                };

                result.Add(preview);
                row++;
            }

            return result;
        }
        public async Task<List<DebitCreditTemplate>> GetAllDebitCreditAsync()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.DebitCreditTemplate
                    .AsNoTracking()
                    .ToListAsync();
            return rs;
        }

        public async Task<DebitCreditTemplate> AddDebitCreditAsync(DebitCreditTemplate dto)
        {
            dto.Id = Guid.NewGuid();
            _context.DebitCreditTemplate.Add(dto);
            await _context.SaveChangesAsync();
            return dto;
        }

        public async Task UpdateDebitCreditAsync(DebitCreditTemplate dto)
        {
            // nếu bạn muốn patch từng field, load entity rồi gán, nhưng Update cũng ổn
            _context.DebitCreditTemplate.Update(dto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDebitCreditAsync(Guid id)
        {
            var e = await _context.DebitCreditTemplate.FindAsync(id);
            if (e != null)
            {
                _context.DebitCreditTemplate.Remove(e);
                await _context.SaveChangesAsync();
            }
        }
    }
}
