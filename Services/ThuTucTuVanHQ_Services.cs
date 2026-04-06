using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;

namespace NVOAMASIS.Services
{
    public class ThuTucTuVanHQ_Services(AppDbContext _context, IWebHostEnvironment _env, AccountService asv, HistoryLogService HistoryLogService)
    {
        public async Task<List<M_YeuCauTuVanTTHQ>> GetListYeuCauThuTucHQCO_request()
        {
            try
            {
                AuthUser user = new AuthUser();
                user = asv.GetUserDetail();

                _context.ChangeTracker.Clear();
                List<M_YeuCauTuVanTTHQ> rs = new();

                rs = _context.YeuCauTuVanThuTucHQCO.OrderBy(x => x.SoYeuCau).Distinct().ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_YeuCauTuVanTTHQ>();
            }
        }
        public async Task<BoolandMessReponse> CreateHQCOsvDetail(M_YeuCauTuVanTTHQ c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.YctvtthqcoID = Guid.NewGuid();
                _context.YeuCauTuVanThuTucHQCO.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteHQCODetail(M_YeuCauTuVanTTHQ c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.YctvtthqcoID == null || c?.YctvtthqcoID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.YeuCauTuVanThuTucHQCO.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteDetail_HQCO(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return new BoolandMessReponse(false, "ID is null");
                }

                var c = await _context.TraLoiYeuCauHQCO.FirstOrDefaultAsync(x => x.YctvtthqcoID == id);

                if (c == null)
                {
                    return new BoolandMessReponse(false, "Detail not found");
                }

                _context.TraLoiYeuCauHQCO.Remove(c);
                await _context.SaveChangesAsync();

                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot delete. Error: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateHQCODetail(M_YeuCauTuVanTTHQ c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.YeuCauTuVanThuTucHQCO.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateHQCO(M_YeuCauTuVanTTHQ IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.YctvtthqcoID == null || IV.YctvtthqcoID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Request Customs Procedures Consultation Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Request Customs Procedures Consultation Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Update Request Customs Procedures Consultation Fail", "0"];
            }
        }

        public async Task<List<M_YeuCauTuVanTTHQ>> GetListHQCO_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.YeuCauTuVanThuTucHQCO.Where(_ => _.YctvtthqcoID == id).ToListAsync();
            return Invoices;
        }

        public async Task<List<M_TraLoiYeuCauHQCO>> GetListTraLoiYeuCauHQCO(Guid id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.TraLoiYeuCauHQCO.Where(_ => _.YctvtthqcoID == id).ToListAsync();
            return Invoices;
        }

        public async Task<BoolandMessReponse> UpdateTraLoiYeuCauHQCO_Detail(M_TraLoiYeuCauHQCO c, M_TraLoiYeuCauHQCO c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.TraLoiYeuCauHQCO.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                string No = await GetNo_ByID_TL_YCHQ(c.YctvtthqcoID);
                await HistoryLogService.LogAsync(usr, "Update Tra Loi YC TTHQ", "YeuCauThuTucHQCO", c.TraLoiyeuCauHQCOID, No, new { OldData = c_old, NewData = c });

                return new BoolandMessReponse(true, "Update CO Customs Request Response Detail Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update CO Customs Request Response Detail with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> CreateTraLoiYeuCauHQCO_Detail(M_TraLoiYeuCauHQCO c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.TraLoiyeuCauHQCOID = Guid.NewGuid();
                _context.TraLoiYeuCauHQCO.Add(c!);
                await _context.SaveChangesAsync();

                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                string No = await GetNo_ByID_TL_YCHQ(c.YctvtthqcoID);
                await HistoryLogService.LogAsync(usr, "ADD Tra Loi YC TTHQ", "YeuCauThuTucHQCO", c.TraLoiyeuCauHQCOID, No, c);


                return new BoolandMessReponse(true, "Create CO Customs Request Response Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add CO Customs Request Response with error code: " + ex.Message);
            }
        }
        public async Task<string?> GetNo_ByID_TL_YCHQ(Guid? id)
        {
            try
            {
                AuthUser user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                var rs = await _context.YeuCauTuVanThuTucHQCO
                    .Where(x => x.YctvtthqcoID == id)
                    .OrderBy(x => x.SoYeuCau)
                    .Select(x => x.SoYeuCau) // chỉ lấy cột MaRFQ
                    .FirstOrDefaultAsync(); // lấy phần tử đầu tiên hoặc null nếu không có

                return rs;
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        public async Task<BoolandMessReponse> DeleteTraLoiYeuCauHQCO_Detail(M_TraLoiYeuCauHQCO c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.TraLoiyeuCauHQCOID == null || c?.TraLoiyeuCauHQCOID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.TraLoiYeuCauHQCO.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete CO Customs Request Response with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateOrCreateTraLoiYeuCauHQCO(M_TraLoiYeuCauHQCO IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.TraLoiyeuCauHQCOID == null || IV.TraLoiyeuCauHQCOID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Product Price Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update CO Customs Request Response Successfully", "1"];
                }
            }
            catch
            {
                return ["Save CO Customs Request Response Fail", "0"];
            }
        }
    }
}
