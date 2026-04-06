using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;

namespace NVOAMASIS.Services
{
    public class ExportCostPriceServices(AppDbContext _context, IWebHostEnvironment _env, AccountService asv)
    {
        public async Task<List<M_ExportCostRequest>> GetExportCostRequest()
        {
            try
            {

                AuthUser user = new AuthUser();
                user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                List<M_ExportCostRequest> rs = new();

                if (user.Department == "ADMIN")
                {
                    rs = await _context.ExportCostRequest
                    .Where(x => x.continued == true)
                    .OrderBy(x => x.maRFQ).ToListAsync();
                }
                else
                {
                    rs = await _context.ExportCostRequest
                      .Where(x => x.continued == true && x.userupdate == user.Name)
                      .OrderBy(x => x.maRFQ).ToListAsync();
                }

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_ExportCostRequest>();
            }

            
    }
        public async Task<BoolandMessReponse> UpdateExportCostDetail(M_ExportCostDetail c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.ExportCostDetail.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Project Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Product Price Enter with error code: " + ex.Message);
            }


        }

        public async Task<BoolandMessReponse> UpdateExportCostRequest(M_ExportCostRequest c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.ExportCostRequest.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Project Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Product Price Enter with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteExportCostDetail(M_ExportCostDetail c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.id == null || c?.id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.ExportCostDetail.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Product Price Enter with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteExportCostRequest(M_ExportCostRequest c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.id == null || c?.id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.ExportCostRequest.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Product Price Enter with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateExportCostRequest(M_ExportCostRequest c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.id = Guid.NewGuid();
                _context.ExportCostRequest.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Product Price Enter Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Product Price Enter with error code: " + ex.Message);
            }
        }
        public async Task<List<M_ExportCostDetail>> GetListExportCostDetails(Guid ID)
        {
            try
            {
                var producs = await _context.ExportCostDetail
                                    .Where(inv => inv.exportcostrequestid==ID)
                                    .ToListAsync();
                return producs;
            }
            catch (Exception ex)
            {
                return new List<M_ExportCostDetail>();
            }
        }

        public async Task<BoolandMessReponse> CreateExportCost_details(M_ExportCostDetail c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.id = Guid.NewGuid();

                _context.ExportCostDetail.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Product Price Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Product Price with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeletepRODUCTdETAILS(M_ExportCostDetail c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.id == null || c?.id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.ExportCostDetail.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Product Price with error code: " + ex.Message);
            }
        }
    }
}
