using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;

namespace NVOAMASIS.Services
{
    public class TaxServices(AppDbContext _context, IWebHostEnvironment _env, IJSRuntime JSRuntime, AccountService asv, SupportServices supsv)
    {
        public async Task<List<M_TaxInvoice>> GetListTaxInvoice()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.TaxInvoice.Where(x => x.Continued == true).OrderByDescending(x => x.Updatetime).ToListAsync();
            return rs;
        }
        public async Task<M_TaxInvoice?> GetTaxInvoiceByID(Guid TaxInvoiceID)
        {
            _context.ChangeTracker.Clear();
            return await _context.TaxInvoice.AsNoTracking().FirstOrDefaultAsync(x => x.TaxInvoiceID == TaxInvoiceID);
        }
        public async Task<List<M_TaxDetail>> GetListTaxDetail(Guid TaxInvoiceID)
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.TaxDetail.Where(x => x.taxInvoiceID == TaxInvoiceID).OrderBy(x => x.STT).ToListAsync();
            return rs;
        }
        public async Task<List<M_TaxDetail>> GetListTaxDetailALL()
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.TaxDetail.OrderBy(x => x.STT).ToListAsync();
            return rs;
        }
        public async Task<BoolandMessReponse> UpdateTaxInvoice(M_TaxInvoice c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.Updatetime = DateTime.Now;
                _context.TaxInvoice.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Tax Invoice Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Tax Invoice with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateTaxInvoice(M_TaxInvoice c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.TaxInvoice.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Tax Invoice Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Tax Invoice with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteTaxInvoice(M_TaxInvoice c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.TaxInvoiceID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.TaxInvoice.Remove(c!);
                var details = await _context!.TaxDetail.Where(x => x.taxInvoiceID == c.TaxInvoiceID).ToListAsync();
                _context?.TaxDetail.RemoveRange(details);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Tax Invoice with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateTaxDetail(M_TaxDetail c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.UpdateTime = DateTime.Now;
                _context.TaxDetail.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Tax Detail Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Tax Detail with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateTotalInvoiceByTaxInvoiceID(Guid taxInvoiceID)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var _Inv = await _context.TaxInvoice.Where(x => x.TaxInvoiceID == taxInvoiceID).FirstOrDefaultAsync();
                if (_Inv == null) 
                    return new BoolandMessReponse(true, "Not found Invoice!");
                var Details = await _context.TaxDetail.Where(x => x.taxInvoiceID == taxInvoiceID).ToListAsync();
                _Inv.tongTruocThue = Details.Sum(x => x.thanhtientruocthueVND ?? 0); // hết dòng detail thì về 0
                var VAT = (double.TryParse(_Inv.VAT, out double VATparse) ? VATparse : 0) / 100;
                _Inv.tongThue = _Inv.tongTruocThue * VAT;
                _Inv.tongSauThue = _Inv.tongTruocThue + _Inv.tongThue;
                _context.TaxInvoice.Update(_Inv);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Tax Detail Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Tax Detail with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateTaxDetail(M_TaxDetail c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.taxDetailID = Guid.NewGuid();
                _context.TaxDetail.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Tax Detail Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Tax Detail with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateTaxDetail(List<M_TaxDetail> c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.ForEach(x=>x.taxDetailID = Guid.NewGuid());
                _context.TaxDetail.AddRange(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Tax Detail Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Tax Detail with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteTaxDetail(M_TaxDetail c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.taxDetailID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.TaxDetail.Remove(c!);
                await _context?.SaveChangesAsync()!;

                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Tax Detail with error code: " + ex.Message);
            }
        }
        public async Task<List<string>> GetListType()
        {
            _context.ChangeTracker.Clear();
            var rsdebit = await _context.Debit.Select(x => x.type).Distinct().ToListAsync();
            var rscredit = await _context.Credit.Select(x => x.type).Distinct().ToListAsync();
            var rsTaxDetail = await _context.TaxDetail.Select(x => x.Container_Type).Distinct().ToListAsync();
            var rs = rsdebit
                .Union(rscredit)
                .Union(rsTaxDetail)
                .Distinct()
                .ToList();
            return rs!;
        }
    }
}
