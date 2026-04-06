using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;


namespace NVOAMASIS.Services
{
    public class Danhmuctaikhoan_services(AppDbContext _context, IWebHostEnvironment _env, AccountService asv, HistoryLogService HistoryLogService)
    {
        public async Task<List<DanhMucTaiKhoan>> GetList_Danhmuctaikhoan()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<DanhMucTaiKhoan> rs = new();

                rs = _context.DanhMucTaiKhoan.OrderByDescending(x => x.Taikhoan).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<DanhMucTaiKhoan>();
            }
        }

        public async Task<List<string>> GetList_Danhmuctaikhoan_search()
        {
             
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.DanhMucTaiKhoan.Where(x => x.Taikhoan != null)
                    .Select(x => x.Taikhoan).Distinct().OrderBy(x => x).ToListAsync();
                rs.Insert(0, "");
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        
        }
        //------------------------------------------------------
        public async Task<BoolandMessReponse> CreateDanhMucTaiKhoan_Detail(DanhMucTaiKhoan c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();
                _context.DanhMucTaiKhoan.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Danh Muc Tai Khoan", "Danhmuctaikhoan", c.Id, c.Taikhoan, c);
                return new BoolandMessReponse(true, "Create Account List Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Account List with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateDanhMucTaiKhoan(DanhMucTaiKhoan IV, DanhMucTaiKhoan IV_old)
        {
            try
            {
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    await HistoryLogService.LogAsync(usr, "ADD Danh Muc Tai Khoan", "Danhmuctaikhoan", IV.Id, IV.Taikhoan, IV);
                    return ["Create new Account Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    await HistoryLogService.LogAsync(usr, "Update Danh Muc Tai Khoan", "Danhmuctaikhoan",IV.Id, IV.Taikhoan, new { OldData = IV_old, NewData = IV });
                    return ["Update Account Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Account Fail", "0"];
            }
        }


        public async Task<BoolandMessReponse> DeleteDanhMucTaiKhoan_Detail(DanhMucTaiKhoan c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.DanhMucTaiKhoan.Remove(c!);
                await _context?.SaveChangesAsync()!;
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete Danh Muc Tai Khoan", "Danhmuctaikhoan", c.Id, c.Taikhoan, new { OldData = c});
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateDanhMucTaiKhoan_Detail(DanhMucTaiKhoan c, DanhMucTaiKhoan c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.DanhMucTaiKhoan.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Danh Muc Tai Khoan", "Danhmuctaikhoan", c.Id, c.Taikhoan, new { OldData = c_old ,NewData =c });
                return new BoolandMessReponse(true, "Update Account  Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Account  with error code: " + ex.Message);
            }
        }

        public async Task<List<DanhMucTaiKhoan>> GetListDanhMucTaiKhoan_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.DanhMucTaiKhoan.Where(_ => _.Id == id).ToListAsync();
            return Invoices;
        }
    }
}
