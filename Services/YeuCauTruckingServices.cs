using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;

namespace NVOAMASIS.Services
{
    public class YeuCauTruckingServices(AppDbContext _context, IWebHostEnvironment _env, IJSRuntime JSRuntime, AccountService asv)
    {
        public async Task<List<M_YeuCauTrucking>> GetListYCTrucking() 
        { 
            try
            {
                _context.ChangeTracker.Clear();
                List<M_YeuCauTrucking> rs = new();

            rs = _context.YeuCauTrucking.OrderByDescending(x => x.YeuCauTruckingNo).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_YeuCauTrucking>();
            }
        }

        public async Task<BoolandMessReponse> CreateYCTruckingDetail(M_YeuCauTrucking c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.YeuCauTruckingID = Guid.NewGuid();
                _context.YeuCauTrucking.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Project Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteYCTruckingDetail(M_YeuCauTrucking c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.YeuCauTruckingID == null || c?.YeuCauTruckingID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.YeuCauTrucking.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateYCTruckingDetail(M_YeuCauTrucking c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.YeuCauTrucking.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Project Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Project with error code: " + ex.Message);
            }
        }
        public async Task<List<string>> UpdateCreateYCttruck(M_YeuCauTrucking IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.YeuCauTruckingID == null || IV.YeuCauTruckingID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create New Trucking Requirements Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Trucking Requirements Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Trucking Requirements Fail", "0"];
            }
        }
        public async Task<List<M_YeuCauTrucking>> GetListYCTrucking_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.YeuCauTrucking.Where(_ => _.YeuCauTruckingID == id).ToListAsync();
            return Invoices;
        }

        public async Task<List<string>> GetListYCTruckNo()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.YeuCauTrucking.Select(x => x.YeuCauTruckingNo).ToListAsync();
            return rs;
        }
        public M_YeuCauTrucking GetDetailByNo(string? no)
        {
            _context.ChangeTracker.Clear();
            var rs = _context.YeuCauTrucking.FirstOrDefault(x => x.YeuCauTruckingNo == no);
            return rs;
        }
    }
}
