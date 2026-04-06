using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;

namespace NVOAMASIS.Services
{
    public class LocalChargesServices(AppDbContext _context, IWebHostEnvironment _env, AccountService asv,HistoryLogService HistoryLogService)
    {
        public async Task<List<M_LocalCharge_pt>> GetListLocalcharges_pt()
        {
            try
            {
                //AuthUser user = new AuthUser();
                //user = asv.GetUserDetail();

                _context.ChangeTracker.Clear();
                List<M_LocalCharge_pt> rs = new();

                rs = _context.localcharges_pt.OrderByDescending(x => x.Index_).ToList();
             
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_LocalCharge_pt>();
            }
        }

        public async Task<BoolandMessReponse> CreateLocalCharges_Pt_Detail(M_LocalCharge_pt c)
        {
            try
            {
                
                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();
                _context.localcharges_pt.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Local Charge", "LocalCharges_PT", c.Id, c.Carr, c);
                return new BoolandMessReponse(true, "Create Local Charges Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Local Charges with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateLCC(M_LocalCharge_pt IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new LCC Shipping Line Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update LCC Shipping Line Successfully", "1"];
                }
            }
            catch
            {
                return ["Save LCC Shipping Line Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> DeleteLocalCharges_Pt_Detail(M_LocalCharge_pt c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.localcharges_pt.Remove(c!);
                await _context?.SaveChangesAsync()!;

                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete Local Charge", "LocalCharges_PT", c.Id,c.Carr, new { OldData = c });

                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateLocalCharges_Pt_Detail(M_LocalCharge_pt c, M_LocalCharge_pt c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.localcharges_pt.Update(c);
                await _context.SaveChangesAsync();

                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Local Charge", "LocalCharges_PT", c.Id, c.Carr, new { OldData = c_old, NewData = c });

                return new BoolandMessReponse(true, "Update Local Charges Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Local Charges with error code: " + ex.Message);
            }
        }

        public async Task<List<M_Customer>> GetList_Customer_line()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Customer.OrderBy(x => x.Customer_Code)
                    .Where(x => x.MainCode.Contains("Shipping"))
                    .ToListAsync();
                rs.Insert(0, new M_Customer { Customer_Code = "" });
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Customer>();
            }

        }

        public async Task<int?> Get_index()
        {
            try
            {
                _context.ChangeTracker.Clear();

                // Lấy giá trị Index_ lớn nhất và cộng thêm 1
                int maxIndex = await _context.localcharges_pt.MaxAsync(x => (int?)x.Index_) ?? 0;

                return maxIndex + 1;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }
        public async Task<List<M_LocalCharge_pt>> GetListLCC_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.localcharges_pt.Where(_ => _.Id == id).ToListAsync();
            return Invoices;
        }

    }
}
