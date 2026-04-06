


using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Stimulsoft.Base.StiDbType;
using static Stimulsoft.Report.Func;
using static NVOAMASIS.Components.Report_RFQ.Pages.Index;

namespace NVOAMASIS.Services
{
    public class ProductPriceServices(AppDbContext _context, IWebHostEnvironment _env,AccountService asv, HistoryLogService HistoryLogService)
    {
        

        public async Task<BoolandMessReponse> CreateProductPriceDetail(M_Product_Price c,string loai)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.id = Guid.NewGuid();
                c.loai = loai;
                _context.Product_Price.Add(c!);
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
        

                await HistoryLogService.LogAsync(usr, "ADD", "Product_Price_Export", c.id, c.MaRFQ, c);
                await _context.SaveChangesAsync();

                return new BoolandMessReponse(true, "Create Product Price Enter Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Product Price Enter with error code: " + ex.Message);
            }
        }

    

        public async Task<BoolandMessReponse> DeleteProductPriceDetail(M_Product_Price c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.id == null || c?.id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Product_Price.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Product Price Enter with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateProductPriceDetail(M_Product_Price c, M_Product_Price OldData)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Product_Price.Update(c);
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
         

                await HistoryLogService.LogAsync(usr, "Update", "Product_Price_Export", c.id, c.MaRFQ, new { OldData = OldData, NewData = c });
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Project Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Product Price Enter with error code: " + ex.Message);
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

        public async Task<List<M_Customer>> GetList_Customer_Agent()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Customer.OrderBy(x => x.Customer_Code)
                    .Where(x => x.MainCode.Contains("Agent"))
                    .ToListAsync();
                rs.Insert(0, new M_Customer { Customer_Code = "" });
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Customer>();
            }

        }

        public async Task<List<string>> UpdateOrCreateProductPrice(M_Product_Price IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.id == null || IV.id == Guid.Empty)
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
                    return ["Update Product Price Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Booking Request Fail", "0"];
            }
        }
        public async Task<List<M_Product_Price_Details>> GetListProductPriceDetails_pricingrequest(Guid ID)
        {
            try
            {
                AuthUser user = new AuthUser();
                user = asv.GetUserDetail();

                _context.ChangeTracker.Clear();
                List<M_Product_Price_Details> producs = new();

                if (user.Department == "ADMIN")
                {
                    producs = await(from dt in _context.Product_Price_Details
                                   join pr in _context.Product_Price
                                   on dt.product_price_id equals pr.id
                                   where pr.pricingid == ID
                                   select dt).ToListAsync();
                }
                else
                {
                    producs = await (from dt in _context.Product_Price_Details
                                     join pr in _context.Product_Price
                                     on dt.product_price_id equals pr.id
                                     where pr.pricingid == ID && pr.UserUPdate == user.Name
                                     select dt).ToListAsync();

                    //producs = _context.Duan.OrderBy(x => x.Pricingno)
                    //   .Where(x => x.EmailNguoiPhuTrach.Contains(user.Name))
                    //    .Distinct().ToList();
                }


                ////var producs = await (from dt in _context.Product_Price_Details
                ////                     join pr in _context.Product_Price
                ////                     on dt.product_price_id equals pr.id
                ////                     where pr.pricingid == ID
                ////                     select dt).ToListAsync();

                return producs;
            }
            catch (Exception ex)
            {
                return new List<M_Product_Price_Details>();
            }
        }
        public async Task<List<M_Product_Price_Details>> GetListProductPriceDetails(Guid ID)
        {
            try
            {
                var producs = await _context.Product_Price_Details
                                    .Where(inv => inv.product_price_id == ID)
                                    .ToListAsync();
                return producs;
            }
            catch (Exception ex)
            {
                return new List<M_Product_Price_Details>();
            }
        }
        public async Task<BoolandMessReponse> UpdateProductPrice_details(M_Product_Price_Details c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Product_Price_Details.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Product Price Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Product Price with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> CreateProductPrice_details(M_Product_Price_Details c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.id = Guid.NewGuid();
              
                _context.Product_Price_Details.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Product Price Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Product Price with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeletepRODUCTdETAILS(M_Product_Price_Details c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.id == null || c?.id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Product_Price_Details.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Product Price with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateProduct(M_Product_Price IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.id == null || IV.id == Guid.Empty)
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
                    return ["Update Product Price Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Product Price Fail", "0"];
            }
        }

        //--------------------------------

        public async Task<List<M_ImportCostRequest>> GetProduct_Price_import()
        {
            try
            {

                AuthUser user = new AuthUser();
                user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                List<M_ImportCostRequest> rs = new();

               
                    rs = await _context.ImportCostRequest
                         .Where(x => x.Continued == true)
                        .OrderBy(x => x.MaRFQ).ToListAsync();
                

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_ImportCostRequest>();
            }
        }

        public async Task<string?> GetMaRFQ_ByID(Guid? id)
        {
            try
            {
                AuthUser user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                var rs = await _context.ImportCostRequest
                    .Where(x => x.Id == id)
                    .OrderBy(x => x.MaRFQ)
                    .Select(x => x.MaRFQ) // chỉ lấy cột MaRFQ
                    .FirstOrDefaultAsync(); // lấy phần tử đầu tiên hoặc null nếu không có

                return rs;
            }
            catch (Exception ex)
            {
                
                return null;
            }
        }

        public async Task<string?> GetMaRFQ_ByID_TRUCK(Guid? id)
        {
            try
            {
                AuthUser user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                var rs = await _context.TruckingCostRequest
                    .Where(x => x.Id == id)
                    .OrderBy(x => x.MaRFQ)
                    .Select(x => x.MaRFQ) // chỉ lấy cột MaRFQ
                    .FirstOrDefaultAsync(); // lấy phần tử đầu tiên hoặc null nếu không có

                return rs;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public async Task<string?> GetMaRFQ_ByID_KTCL(Guid? id)
        {
            try
            {
                AuthUser user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                var rs = await _context.KTCLCostRequest
                    .Where(x => x.Id == id)
                    .OrderBy(x => x.MaRFQ)
                    .Select(x => x.MaRFQ) // chỉ lấy cột MaRFQ
                    .FirstOrDefaultAsync(); // lấy phần tử đầu tiên hoặc null nếu không có

                return rs;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public async Task<string?> GetMaRFQ_ByID_Customs(Guid? id)
        {
            try
            {
                AuthUser user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                var rs = await _context.CustomCostRequest
                    .Where(x => x.Id == id)
                    .OrderBy(x => x.MaRFQ)
                    .Select(x => x.MaRFQ) // chỉ lấy cột MaRFQ
                    .FirstOrDefaultAsync(); // lấy phần tử đầu tiên hoặc null nếu không có

                return rs;
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        public async Task<string?> GetMaRFQ_ByID_Export(Guid? id)
        {
            try
            {
                AuthUser user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                var rs = await _context.ExportCostRequest
                    .Where(x => x.id == id)
                    .OrderBy(x => x.maRFQ)
                    .Select(x => x.maRFQ) // chỉ lấy cột MaRFQ
                    .FirstOrDefaultAsync(); // lấy phần tử đầu tiên hoặc null nếu không có

                return rs;
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        public async Task<BoolandMessReponse> DeleteProductPrice_import(M_ImportCostRequest c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.ImportCostRequest.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Import Pricing Enter with error code: " + ex.Message);
            }
        }


        public async Task<BoolandMessReponse> UpdateProductImport(M_ImportCostRequest c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.ImportCostRequest.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update  Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateProduct_import(M_ImportCostRequest IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Product Price Import Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Product Price Import Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Product Price Import Fail", "0"];
            }
        }

        public async Task<List<M_ImportCostRequest>> GetProduct_price_import_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.ImportCostRequest.Where(_ => _.Id == id).ToListAsync();
            return Invoices;
        }

        public async Task<List<string>> UpdateCreateProduct_Detail(M_Product_Price_Details IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.id == null || IV.id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Product Price Detail Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Product Price Detail Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Product Price Detail Fail", "0"];
            }
        }


        //----------------Import
        public async Task<List<string>> UpdateCreateProduct_Import(M_ImportCostRequest IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Product Price Import Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Product Price Import Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Product Price Import Fail", "0"];
            }
        }
        //----------------Truck
        public async Task<List<M_TruckingCostRequest>> GetProduct_Price_truck()
        {
            try
            {

                AuthUser user = new AuthUser();
                user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                List<M_TruckingCostRequest> rs = new();

                
                    rs = await _context.TruckingCostRequest
                         .Where(x => x.Continued == true)
                        .OrderBy(x => x.MaRFQ).ToListAsync();
               

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_TruckingCostRequest>();
            }
        }

        public async Task<BoolandMessReponse> DeleteProductPrice_truck(M_TruckingCostRequest c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.TruckingCostRequest.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Truck Pricing Enter with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateProduct_Truck(M_TruckingCostRequest IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Product Price Truck Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Product Price Truck Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Product Price Truck Fail", "0"];
            }
        }

        public async Task<List<M_TruckingCostRequest>> GetProduct_price_Truck_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.TruckingCostRequest.Where(_ => _.Id == id).ToListAsync();
            return Invoices;
        }
        public async Task<List<string>> UpdateCreateProduct_truck(M_TruckingCostRequest IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Product Price Truck Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Product Price Truck Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Product Price Truck Fail", "0"];
            }
        }
        public async Task<List<M_Product_Price_detail_ALL>> GetListProductPriceDetails_ALL(Guid ID)
        {
            try
            {
                var producs = await _context.Product_Price_Detail_ALL
                                    .Where(inv => inv.RFQ_Id == ID)
                                    .ToListAsync();
                return producs;
            }
            catch (Exception ex)
            {
                return new List<M_Product_Price_detail_ALL>();
            }
        }
        public async Task<BoolandMessReponse> UpdateProductPrice_details_Truck(M_Product_Price_detail_ALL c, M_Product_Price_detail_ALL OldData)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Product_Price_Detail_ALL.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                string RFQNo = "";
                string Form = "";
                if (!string.IsNullOrEmpty(await GetMaRFQ_ByID_TRUCK(c.RFQ_Id)))
                {
                    RFQNo = await GetMaRFQ_ByID_TRUCK(c.RFQ_Id);
                    Form = "Product_Price_Truck";
                }
                else if (!string.IsNullOrEmpty(await GetMaRFQ_ByID(c.RFQ_Id)))
                {
                    RFQNo = await GetMaRFQ_ByID(c.RFQ_Id);
                    Form = "Product_Price";
                }
                else if (!string.IsNullOrEmpty(await GetMaRFQ_ByID_KTCL(c.RFQ_Id)))
                {
                    RFQNo = await GetMaRFQ_ByID_KTCL(c.RFQ_Id);
                    Form = "Product_Price_KTCL";
                }
                else if (!string.IsNullOrEmpty(await GetMaRFQ_ByID_Customs(c.RFQ_Id)))
                {
                    RFQNo = await GetMaRFQ_ByID_Customs(c.RFQ_Id);
                    Form = "Product_Price_Custom";
                }
                else if (!string.IsNullOrEmpty(await GetMaRFQ_ByID_Export(c.RFQ_Id)))
                {
                    RFQNo = await GetMaRFQ_ByID_Export(c.RFQ_Id);
                    Form = "Product_Price_Export";
                }

                await HistoryLogService.LogAsync(usr, "UpdateDetails", Form, c.Id, RFQNo, new { OldData = OldData, NewData = c });
                return new BoolandMessReponse(true, "Update Product Price Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Product Price with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> CreateProductPrice_details_truck(M_Product_Price_detail_ALL c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();

                _context.Product_Price_Detail_ALL.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                string RFQNo = "";
                string Form = "";
                if (!string.IsNullOrEmpty(await GetMaRFQ_ByID_TRUCK(c.RFQ_Id)))
                {
                    RFQNo = await GetMaRFQ_ByID_TRUCK(c.RFQ_Id);
                    Form = "Product_Price_Truck";
                }
                else if (!string.IsNullOrEmpty(await GetMaRFQ_ByID(c.RFQ_Id)))
                {
                    RFQNo = await GetMaRFQ_ByID(c.RFQ_Id);
                    Form = "Product_Price";
                }
                else if (!string.IsNullOrEmpty(await GetMaRFQ_ByID_KTCL(c.RFQ_Id)))
                {
                    RFQNo = await GetMaRFQ_ByID_KTCL(c.RFQ_Id);
                    Form = "Product_Price_KTCL";
                }
                else if (!string.IsNullOrEmpty(await GetMaRFQ_ByID_Customs(c.RFQ_Id)))
                {
                    RFQNo = await GetMaRFQ_ByID_Customs(c.RFQ_Id);
                    Form = "Product_Price_Custom";
                }

                await HistoryLogService.LogAsync(usr, "ADDDetails",Form, c.Id, RFQNo, c);
                return new BoolandMessReponse(true, "Create Product Price Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Product Price with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeletepRODUCTdETAILS_Truck(M_Product_Price_detail_ALL c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Product_Price_Detail_ALL.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Product Price with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateProduct_Detail_truck(M_Product_Price_detail_ALL IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Product Price Detail Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Product Price Detail Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Product Price Detail Fail", "0"];
            }
        }

        //-----------------KTCL
        public async Task<List<M_KTCLCostRequest>> GetProduct_Price_KTCL()
        {
            try
            {

                AuthUser user = new AuthUser();
                user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                List<M_KTCLCostRequest> rs = new();

                
                    rs = await _context.KTCLCostRequest
                         .Where(x => x.Continued == true)
                        .OrderBy(x => x.MaRFQ).ToListAsync();
               

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_KTCLCostRequest>();
            }
        }

        public async Task<BoolandMessReponse> DeleteProductPrice_KTCL(M_KTCLCostRequest c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.KTCLCostRequest.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Cost Request Quality Control with error code: " + ex.Message);
            }
        }
        public async Task<List<string>> UpdateCreateProduct_KTCL(M_KTCLCostRequest IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Cost Request Quality Control Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Cost Request Quality Control Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Cost Request Quality Control Fail", "0"];
            }
        }

        public async Task<List<M_KTCLCostRequest>> GetProduct_price_KTCL_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.KTCLCostRequest.Where(_ => _.Id == id).ToListAsync();
            return Invoices;
        }

        //-------------------------------Customs
        public async Task<List<M_CustomCostRequest>> GetProduct_Price_Customs()
        {
            try
            {

                AuthUser user = new AuthUser();
                user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                List<M_CustomCostRequest> rs = new();

                    rs = await _context.CustomCostRequest
                         .Where(x => x.Continued == true)
                        .OrderBy(x => x.MaRFQ).ToListAsync();
               
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_CustomCostRequest>();
            }
        }
        public async Task<BoolandMessReponse> DeleteProductPrice_Customs(M_CustomCostRequest c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.CustomCostRequest.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Customs Procedure Cost Price with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateProduct_Customs(M_CustomCostRequest IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Customs Procedure Cost Price Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Customs Procedure Cost Price Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Customs Procedure Cost Price Fail", "0"];
            }
        }

        public async Task<List<M_CustomCostRequest>> GetProduct_price_Customs_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.CustomCostRequest.Where(_ => _.Id == id).ToListAsync();
            return Invoices;
        }

        public async Task<List<M_Product_Price_detail_ALL>> Get_ProductPrice_Detail_All(int fromthang, int fromnam, int tothang, int tonam, string type, Guid? itemid,string POL , string POD ,bool chkpol,bool chkpod)
        {
            var fromDate = new DateTime(fromnam, fromthang, 1);
            var toDate = new DateTime(tonam, tothang, 1).AddMonths(1).AddDays(-1);
            _context.ChangeTracker.Clear();
            List<M_Product_Price_detail_ALL> rs = new();

            //rs = await _context.Product_Price_Detail_ALL
            //        .Where(x => x.Ngaybaogia >= fromDate && x.Ngaybaogia <= toDate && x.Type == type && (!itemid.HasValue || x.Itemid == itemid))
            //        .ToListAsync();
            if(type == "Export")
            {
                rs = await (from p in _context.Product_Price_Detail_ALL
                            join ex in _context.ExportCostRequest
                                on p.RFQ_Id equals ex.id
                            where p.Ngaybaogia >= fromDate
                                  && p.Ngaybaogia <= toDate
                                  && p.Type == type
                                  && (!itemid.HasValue || itemid == Guid.Empty || p.Itemid == itemid)
                                    && (!chkpol || ex.cangdi == POL)
                                    && (!chkpod || ex.cangden == POD)
                            select p).ToListAsync();
            }
            else if (type == "Import")
            {
                rs = await (from p in _context.Product_Price_Detail_ALL
                            join ex in _context.ImportCostRequest
                                on p.RFQ_Id equals ex.Id
                            where p.Ngaybaogia >= fromDate
                                  && p.Ngaybaogia <= toDate
                                  && p.Type == type
                                  && (!itemid.HasValue || itemid == Guid.Empty || p.Itemid == itemid)
                                    && (!chkpol || ex.Cangxuat == POL)
                                    && (!chkpod || ex.Cangnhap == POD)
                            select p).ToListAsync();
            }
            else if (type == "Customs")
            {
                rs = await (from p in _context.Product_Price_Detail_ALL
                            join ex in _context.CustomCostRequest
                                on p.RFQ_Id equals ex.Id
                            where p.Ngaybaogia >= fromDate
                                  && p.Ngaybaogia <= toDate
                                  && p.Type == type
                                  && (!itemid.HasValue || itemid == Guid.Empty || p.Itemid == itemid)

                            select p).ToListAsync();
            }
            else if (type == "KTCL")
            {
                rs = await (from p in _context.Product_Price_Detail_ALL
                            join ex in _context.ImportCostRequest
                                on p.RFQ_Id equals ex.Id
                            where p.Ngaybaogia >= fromDate
                                  && p.Ngaybaogia <= toDate
                                  && p.Type == type
                                 && (!itemid.HasValue || itemid == Guid.Empty || p.Itemid == itemid)

                            select p).ToListAsync();
            }
            else if (type == "Trucking")
            {
                rs = await (from p in _context.Product_Price_Detail_ALL
                            join ex in _context.TruckingCostRequest
                                on p.RFQ_Id equals ex.Id
                            where p.Ngaybaogia >= fromDate
                                  && p.Ngaybaogia <= toDate
                                  && p.Type == type
                                 && (!itemid.HasValue || itemid == Guid.Empty || p.Itemid == itemid)
                                    && (!chkpol || ex.Noilayhang == POL)
                                    && (!chkpod || ex.Noigiaohang == POD)
                            select p).ToListAsync();
            }


            return rs;
        }

        //--------------------list all detail import

        public async Task<List<M_Product_Price_detail_ALL>> GetProduct_Price_Detail_ByType(string type)
        {
            try
            {
                AuthUser user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                List<M_Product_Price_detail_ALL> rs = new();

                
                    if (type == "Import")
                    {
                        rs = await (from detail in _context.Product_Price_Detail_ALL
                                    join cost in _context.ImportCostRequest
                                        on detail.RFQ_Id equals cost.Id
                                    where detail.Continued == true
                                    select detail)
                                    .ToListAsync();

                      

                        return rs;
                    }
                    else if (type == "Truck")
                    {
                        rs = await (from detail in _context.Product_Price_Detail_ALL
                                    join cost in _context.TruckingCostRequest
                                        on detail.RFQ_Id equals cost.Id
                                    where detail.Continued == true
                                    select detail)
                                    .ToListAsync();
                    }
                    else if (type == "KTCL")
                    {
                        rs = await (from detail in _context.Product_Price_Detail_ALL
                                    join cost in _context.KTCLCostRequest
                                        on detail.RFQ_Id equals cost.Id
                                    where detail.Continued == true
                                    select detail)
                                    .ToListAsync();
                    }
                    else if (type == "Custom")
                    {
                        rs = await (from detail in _context.Product_Price_Detail_ALL
                                    join cost in _context.CustomCostRequest
                                        on detail.RFQ_Id equals cost.Id
                                    where detail.Continued == true
                                    select detail)
                                    .ToListAsync();
                    }
                    else if (type == "Export")
                    {
                        rs = await (from detail in _context.Product_Price_Detail_ALL
                                    join cost in _context.ExportCostRequest
                                        on detail.RFQ_Id equals cost.id
                                    where detail.Continued == true
                                    select detail)
                                    .ToListAsync();
                    }
                
                

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_Product_Price_detail_ALL>();
            }
        }

        public async Task<List<string>> UpdateCreateProduct_export(M_ExportCostRequest IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.id == null || IV.id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Export Cost Request Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Export Cost Request  Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Export Cost Request  Fail", "0"];
            }
        }

        public async Task<List<M_ExportCostRequest>> GetProduct_Price_export()
        {
            try
            {

                AuthUser user = new AuthUser();
                user = asv.GetUserDetail();
                _context.ChangeTracker.Clear();

                List<M_ExportCostRequest> rs = new();

                
                    rs = await _context.ExportCostRequest
                         .Where(x => x.continued == true)
                        .OrderBy(x => x.maRFQ).ToListAsync();
               
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_ExportCostRequest>();
            }
        }

        public async Task<BoolandMessReponse> DeleteProductPrice_export(M_ExportCostRequest c)
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
                return new BoolandMessReponse(false, "Cannot Delete Export Cost Request with error code: " + ex.Message);
            }
        }
        
        public async Task<List<M_ExportCostRequest>> GetProduct_price_export_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.ExportCostRequest.Where(_ => _.id == id).ToListAsync();
            return Invoices;
        }
        public async Task<BoolandMessReponse> DeleteDetail_All(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return new BoolandMessReponse(false, "ID is null");
                }

                var c = await _context.Product_Price_Detail_ALL.FirstOrDefaultAsync(x => x.RFQ_Id == id);

                if (c == null)
                {
                    return new BoolandMessReponse(false, "Detail not found");
                }

                _context.Product_Price_Detail_ALL.Remove(c);
                await _context.SaveChangesAsync();

                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot delete. Error: " + ex.Message);
            }
        }
        public List<M_Container> GetListContainerAll()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.Container.Where(x => x.CONTINUED == true).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Container>();
            }
        }
    }
}
