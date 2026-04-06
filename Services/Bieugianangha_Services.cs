using Microsoft.EntityFrameworkCore;
using System.Globalization;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;

namespace NVOAMASIS.Services
{
    public class Bieugianangha_Services(AppDbContext _context, IWebHostEnvironment _env, AccountService asv, HistoryLogService HistoryLogService)
    {
        public async Task<List<M_Bieugianangha>> GetList_BieuGiaNangHa()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_Bieugianangha> rs = new();

                rs = _context.BieuGiaNangHa.OrderByDescending(x => x.BieugiananghaNo).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_Bieugianangha>();
            }
        }


       

        public async Task<List<M_Bieugiarutruot>> GetList_BieuGiaRutRuot()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_Bieugiarutruot> rs = new();

                rs = _context.Bieugiarutruot.OrderByDescending(x => x.BieugiarutruotNo).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_Bieugiarutruot>();
            }
        }

        public async Task<List<M_BieuGiaKDTV>> GetList_BieuGiaKDTV()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_BieuGiaKDTV> rs = new();

                rs = _context.BieugiaKDTV.OrderByDescending(x => x.BieuGiaKDTVNo).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_BieuGiaKDTV>();
            }
        }

        public async Task<List<M_BieuGiaKTCL>> GetList_BieuGiaKTCL()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_BieuGiaKTCL> rs = new();

                rs = _context.BieugiaKTCL.OrderByDescending(x => x.BieuGiaKTCLNo).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_BieuGiaKTCL>();
            }
        }
     
        public async Task<List<M_BieuGiaLuuKho>> GetList_BieuGiaLuuKho()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_BieuGiaLuuKho> rs = new();

                rs = _context.Bieugialuukho.OrderByDescending(x => x.Bieugialuukhono).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_BieuGiaLuuKho>();
            }
        }

        public async Task<BoolandMessReponse> CreateBieugiarutruot_Detail(M_Bieugiarutruot c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.RutRuotID = Guid.NewGuid();
                _context.Bieugiarutruot.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Exorbitant price list List Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Exorbitant price list List with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateBieugiarutruot(M_Bieugiarutruot IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.RutRuotID == null || IV.RutRuotID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Exorbitant price list Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Exorbitant price list Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Exorbitant price list Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> DeleteBieugiarutruot_Detail(M_Bieugiarutruot c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.RutRuotID == null || c?.RutRuotID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Bieugiarutruot.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateBieugiarutruot_Detail(M_Bieugiarutruot c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Bieugiarutruot.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Exorbitant price list  Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Exorbitant price list  with error code: " + ex.Message);
            }
        }

        public async Task<List<M_Bieugiarutruot>> GetListBieugiarutruot_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.Bieugiarutruot.Where(_ => _.RutRuotID == id).ToListAsync();
            return Invoices;
        }
        //----------------------------------------------------------------------------------
        public async Task<BoolandMessReponse> CreateBieugiaKDTV_Detail(M_BieuGiaKDTV c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.KDTVID = Guid.NewGuid();
                _context.BieugiaKDTV.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Plant Quarantine price list List Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Plant Quarantine price list List with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateBieugiaKDTV(M_BieuGiaKDTV IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.KDTVID == null || IV.KDTVID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Plant Quarantine price list Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Plant Quarantine price list Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Plant Quarantine price list Fail", "0"];
            }
        }
        public async Task<List<string>> UpdateCreateChitietBGLuuKho(M_ChiTietBieuGiaLuuKho IV)
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

        public async Task<BoolandMessReponse> DeleteBieugiaKDTV_Detail(M_BieuGiaKDTV c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.KDTVID == null || c?.KDTVID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.BieugiaKDTV.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateBieugiaKDTV_Detail(M_BieuGiaKDTV c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.BieugiaKDTV.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Plant Quarantine price list  Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Plant Quarantine price list  with error code: " + ex.Message);
            }
        }

        public async Task<List<M_BieuGiaKDTV>> GetListBieugiaKDTV_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.BieugiaKDTV.Where(_ => _.KDTVID == id).ToListAsync();
            return Invoices;
        }
        //------------------------------------------------------------------------------------
        public async Task<BoolandMessReponse> CreateBieugiaKTCL_Detail(M_BieuGiaKTCL c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.BieuGiaKTCLID = Guid.NewGuid();
                _context.BieugiaKTCL.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Quality Control price list List Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Quality Control price list List with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateBieugiaKTCL(M_BieuGiaKTCL IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.BieuGiaKTCLID == null || IV.BieuGiaKTCLID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Quality Control price list Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Quality Control price list Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Quality Control price list Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> DeleteBieugiaKTCL_Detail(M_BieuGiaKTCL c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.BieuGiaKTCLID == null || c?.BieuGiaKTCLID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.BieugiaKTCL.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateBieugiaKTCL_Detail(M_BieuGiaKTCL c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.BieugiaKTCL.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Quality Control price list  Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Quality Control price list  with error code: " + ex.Message);
            }
        }

        public async Task<List<M_BieuGiaKTCL>> GetListBieugiaKTCL_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.BieugiaKTCL.Where(_ => _.BieuGiaKTCLID == id).ToListAsync();
            return Invoices;
        }
        //------------------------------------------------------------------------------------
        public async Task<BoolandMessReponse> CreateBieugiaLuukho_Detail(M_BieuGiaLuuKho c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.BieuGiaLuuKhoID = Guid.NewGuid();
                _context.Bieugialuukho.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Warehouse price list list List Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Warehouse price list list List with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateBieugiaLuukho(M_BieuGiaLuuKho IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.BieuGiaLuuKhoID == null || IV.BieuGiaLuuKhoID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Warehouse price list list Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Warehouse price list list Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Warehouse price list list Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> DeleteBieugiaLuukhoL_Detail(M_BieuGiaLuuKho c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.BieuGiaLuuKhoID == null || c?.BieuGiaLuuKhoID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Bieugialuukho.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateBieugiaLuukho_Detail(M_BieuGiaLuuKho c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Bieugialuukho.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Warehouse price list Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Warehouse price list with error code: " + ex.Message);
            }
        }

        public async Task<List<M_BieuGiaLuuKho>> GetListBieugiaLuukho_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.Bieugialuukho.Where(_ => _.BieuGiaLuuKhoID == id).ToListAsync();
            return Invoices;
        }
        public async Task<List<M_ChiTietBieuGiaLuuKho>> GetListChitietBgLuuKho(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.chiTietBieuGiaLuuKho.Where(_ => _.BieuGiaLuuKhoID == id).ToListAsync();
            return Invoices;
        }

        public async Task<List<M_ChiTietBieuGiaLuuKho>> GetList_CTBGLK_All()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<M_ChiTietBieuGiaLuuKho> rs = new();

                rs = _context.chiTietBieuGiaLuuKho.OrderByDescending(x => x.Dateupdate).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_ChiTietBieuGiaLuuKho>();
            }
        }
        //------------------------------------------------------------------------------------
        public async Task<BoolandMessReponse> CreateChitietBGLuuKho_Detail(M_ChiTietBieuGiaLuuKho c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.ChiTietBieuGiaLuuKhoID = Guid.NewGuid();
                _context.chiTietBieuGiaLuuKho.Add(c!);
                await _context.SaveChangesAsync();
                       string usr = asv.GetAuth().Result.User.Identity!.Name!;
        
                string No = await GetBGNo_ByID(c.BieuGiaLuuKhoID);
                await HistoryLogService.LogAsync(usr, "ADD Chi Tiet BG Luu Kho", "ChiTietBieuGiaLuuKho", c.ChiTietBieuGiaLuuKhoID,No, c);

                return new BoolandMessReponse(true, "Create Warehouse price list details Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Warehouse price list details with error code: " + ex.Message);
            }
        }

      
        public async Task<BoolandMessReponse> DeleteChitietBGLuuKhoL_Detail(M_ChiTietBieuGiaLuuKho c)
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
        public async Task<BoolandMessReponse> UpdateChitietBGLuuKho_Detail(M_ChiTietBieuGiaLuuKho c, M_ChiTietBieuGiaLuuKho c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.chiTietBieuGiaLuuKho.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                string No = await GetBGNo_ByID(c.BieuGiaLuuKhoID);
                await HistoryLogService.LogAsync(usr, "Update Chi Tiet BG Luu Kho", "ChiTietBieuGiaLuuKho", c.ChiTietBieuGiaLuuKhoID, No, new { OldData = c_old, NewData = c });

                return new BoolandMessReponse(true, "Update Warehouse price list Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Warehouse price list with error code: " + ex.Message);
            }
        }

        public async Task<List<M_ChiTietBieuGiaLuuKho>> GetListChitietBGLuuKho_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.chiTietBieuGiaLuuKho.Where(_ => _.ChiTietBieuGiaLuuKhoID == id).ToListAsync();
            return Invoices;
        }
    
        //----------------------------------------------------------------------------------

        public async Task<BoolandMessReponse> CreateBieuGiaNangHa_Detail(M_Bieugianangha c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.BieuGiaNangHaID = Guid.NewGuid();
                _context.BieuGiaNangHa.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Lifting and Lowering Price List Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Lifting and Lowering Price List with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateBieuGiaNangHa(M_Bieugianangha IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.BieuGiaNangHaID == null || IV.BieuGiaNangHaID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Lifting and Lowering Price List Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Lifting and Lowering Price List Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Lifting and Lowering Price List Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> DeleteBieuGiaNangHa_Detail(M_Bieugianangha c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.BieuGiaNangHaID == null || c?.BieuGiaNangHaID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.BieuGiaNangHa.Remove(c!);
                await _context?.SaveChangesAsync()!;
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete Bieu Gia Nang Ha", "BieuGiaNangHa", c.BieuGiaNangHaID, c.BieugiananghaNo, new { OldData = c });
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateBieuGiaNangHa_Detail(M_Bieugianangha c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.BieuGiaNangHa.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Lifting and Lowering Price List Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Lifting and Lowering Price List with error code: " + ex.Message);
            }
        }

        public async Task<List<M_Bieugianangha>> GetListBieuGiaNangHa_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.BieuGiaNangHa.Where(_ => _.BieuGiaNangHaID == id).ToListAsync();
            return Invoices;
        }
        //------------------------------------
        public async Task UpdateTrangThai_HetHieuLuc_Nangha()
        {
            try
            {
                _context.ChangeTracker.Clear();

                var today = DateTime.Today;

                var listToUpdate = await _context.BieuGiaNangHa
                    .Where(x => x.ThoiGianApDung.HasValue)
                    .ToListAsync();

                foreach (var item in listToUpdate)
                {
                    if (item.ThoiGianApDung.Value < today)
                    {
                        item.TrangThai = "Hết Hiệu Lực";
                    }
                    else
                    {
                        item.TrangThai = "Còn Hiệu Lực";
                    }
                }

                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi cập nhật trạng thái: " + ex.Message);
            }
        }


        public async Task UpdateTrangThai_HetHieuLuc_RutRuot()
        {
            try
            {
                _context.ChangeTracker.Clear();

                var today = DateTime.Today;

                var listToUpdate = await _context.Bieugiarutruot
                    .Where(x => x.NgayHieuLuc.HasValue)
                    .ToListAsync();

                foreach (var item in listToUpdate)
                {
                    if (item.NgayHieuLuc.Value < today)
                    {
                        item.TrangThai = "Hết Hiệu Lực";
                    }
                    else
                    {
                        item.TrangThai = "Còn Hiệu Lực";
                    }
                }

                await _context.SaveChangesAsync();


            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật trạng thái: " + ex.Message);
            }
        }

        public async Task UpdateTrangThai_HetHieuLuc_KDTV()
        {
            try
            {
                _context.ChangeTracker.Clear();

                var today = DateTime.Today;

                var listToUpdate = await _context.BieugiaKDTV
                    .Where(x => x.NgayApDung.HasValue)
                    .ToListAsync();

                foreach (var item in listToUpdate)
                {
                    if (item.NgayApDung.Value < today)
                    {
                        item.TrangThai = "Hết Hiệu Lực";
                    }
                    else
                    {
                        item.TrangThai = "Còn Hiệu Lực";
                    }
                }

                await _context.SaveChangesAsync();


            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật trạng thái: " + ex.Message);
            }
        }

        public async Task UpdateTrangThai_HetHieuLuc_KTCL()
        {
            try
            {
                _context.ChangeTracker.Clear();

                var today = DateTime.Today;

                var listToUpdate = await _context.BieugiaKTCL
                    .Where(x => x.NgayHieuLuc.HasValue)
                    .ToListAsync();

                foreach (var item in listToUpdate)
                {
                    if (item.NgayHieuLuc.Value < today)
                    {
                        item.TrangThai = "Hết Hiệu Lực";
                    }
                    else
                    {
                        item.TrangThai = "Còn Hiệu Lực";
                    }
                }

                await _context.SaveChangesAsync();


            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật trạng thái: " + ex.Message);
            }
        }

        public async Task UpdateTrangThai_HetHieuLuc_BGLK()
        {
            try
            {
                _context.ChangeTracker.Clear();

                var today = DateTime.Today;

                var listToUpdate = await _context.Bieugialuukho
                    .Where(x => x.NgayHieuLuc.HasValue)
                    .ToListAsync();

                foreach (var item in listToUpdate)
                {
                    if (item.NgayHieuLuc.Value < today)
                    {
                        item.Trangthai = "Hết Hiệu Lực";
                    }
                    else
                    {
                        item.Trangthai = "Còn Hiệu Lực";
                    }
                }

                await _context.SaveChangesAsync();


            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật trạng thái: " + ex.Message);
            }
        }

        public async Task<string?> GetBGNo_ByID(Guid? id)
        {
            try
            {
                AuthUser user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                var rs = await _context.Bieugialuukho
                    .Where(x => x.BieuGiaLuuKhoID == id)
                    .OrderBy(x => x.Bieugialuukhono)
                    .Select(x => x.Bieugialuukhono) // chỉ lấy cột MaRFQ
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
