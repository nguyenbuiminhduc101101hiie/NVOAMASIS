using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;

namespace NVOAMASIS.Services
{
    public class TKHQ_Services(AppDbContext _context, IWebHostEnvironment _env, IJSRuntime JSRuntime, AccountService asv, HistoryLogService HistoryLogService)
    {
        public async Task<List<M_TKHQ_Thongtinchunglohang>> GetListTKHQ()
        {
            try
            {

                _context.ChangeTracker.Clear();
                List<M_TKHQ_Thongtinchunglohang> rs = new();

                rs = _context.TKHQ_Thongtinchunglohang.OrderByDescending(x => x.Sotokhai).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_TKHQ_Thongtinchunglohang>();
            }
        }

        public async Task<List<M_TKHQ_Thongtinchunglohang>> GetListTKHQ_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.TKHQ_Thongtinchunglohang.Where(_ => _.Id == id).ToListAsync();
            return Invoices;
        }

        public async Task<BoolandMessReponse> CreateTKHQ_Detail(M_TKHQ_Thongtinchunglohang c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();
                _context.TKHQ_Thongtinchunglohang.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Local Charges Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Local Charges with error code: " + ex.Message);
            }

        }

          public async Task<BoolandMessReponse> CreateTKHQChitiet_Detail(M_TKHQ_Thongtinchitiet c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();
                _context.TKHQ_Thongtinchitiet.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                string No = await GetNo_ByID_TL_TKHQ(c.Id_Tkhq);
                await HistoryLogService.LogAsync(usr, "ADD Detail TKHQ", "TKHQ", c.Id, No, c);
                return new BoolandMessReponse(true, "Create Local Charges Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Local Charges with error code: " + ex.Message);
            }
        }
        public async Task<List<M_Customer>> GetList_Cusl()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Customer.OrderBy(x => x.Customer_Code)

                    .ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Customer>();
            }
        }
     
        public async Task<List<string>> UpdateCreateTKHQ(M_TKHQ_Thongtinchunglohang IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new TKHQ Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update LCCTKHQ Successfully", "1"];
                }
            }
            catch
            {
                return ["Save TKHQ Fail", "0"];
            }
        }

        public async Task<List<string>> UpdateCreateTKHQ_Detail(M_TKHQ_Thongtinchitiet IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new TKHQ Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update LCCTKHQ Successfully", "1"];
                }
            }
            catch
            {
                return ["Save TKHQ Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> DeleteTKHQ_Detail(M_TKHQ_Thongtinchunglohang c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.TKHQ_Thongtinchunglohang.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteTKHQChiTiet_Detail(M_TKHQ_Thongtinchitiet c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.TKHQ_Thongtinchitiet.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateTKHQ_Detail(M_TKHQ_Thongtinchunglohang c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.TKHQ_Thongtinchunglohang.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update TKHQ Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update TKHQ with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateTKHQChitiet_Detail(M_TKHQ_Thongtinchitiet c, M_TKHQ_Thongtinchitiet c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.TKHQ_Thongtinchitiet.Update(c);
                await _context.SaveChangesAsync();
                string No = await GetNo_ByID_TL_TKHQ(c.Id_Tkhq);
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Detail TKHQ ", "TKHQ", c.Id,No, new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update TKHQ Detail Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update TKHQ Detail with error code: " + ex.Message);
            }
        }
        public async Task<List<M_TKHQ_Thongtinchitiet>> GetListTKHQ_Detail_BYID_TKHQ(Guid id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.TKHQ_Thongtinchitiet.Where(_ => _.Id_Tkhq == id).ToListAsync();
            return Invoices;
        }

        public async Task<BoolandMessReponse> UpdateTKHQDetail(M_TKHQ_Thongtinchunglohang c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.TKHQ_Thongtinchunglohang.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update with error code: " + ex.Message);
            }
        }
        public List<string?> GetListSoToKhai()
        {
            _context.ChangeTracker.Clear();
            var rs = _context.TKHQ_Thongtinchunglohang.Select(x => x.Sotokhai).Distinct().ToList();
            return rs;
        }

        public M_TKHQ_Thongtinchunglohang GetDetailByNo(string? no)
        {
            _context.ChangeTracker.Clear();
            var rs = _context.TKHQ_Thongtinchunglohang.FirstOrDefault(x => x.Sotokhai == no);
            return rs;
        }

        public async Task<string?> GetNo_ByID_TL_TKHQ(Guid? id)
        {
            try
            {
                AuthUser user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                var rs = await _context.TKHQ_Thongtinchunglohang
                    .Where(x => x.Id == id)
                    .OrderBy(x => x.Sotokhai)
                    .Select(x => x.Sotokhai) // chỉ lấy cột MaRFQ
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
