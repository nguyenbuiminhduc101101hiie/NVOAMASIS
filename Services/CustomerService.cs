using ExcelDataReader;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
using SixLabors.ImageSharp.ColorSpaces;
using System.Data;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using NVOAMASIS.Components.Charge.Pages;
using NVOAMASIS.Components.Quotation.Pages;
using NVOAMASIS.Data;
using NVOAMASIS.Interface;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using static MudBlazor.CategoryTypes;
using static MudBlazor.Icons;
using static NVOAMASIS.Components.Report.Pages.Cus_RPT;
namespace NVOAMASIS.Services;

public class CustomerService(AppDbContext _context, HistoryLogService HistoryLogService, AccountService asv)
{
    public async Task<List<M_Customer>> GetList(AuthUser user)
    {
        try
        {
            _context.ChangeTracker.Clear();

            //var rs = await _context.Customer.Where(x => x.SaleName != "NOMI").OrderByDescending(x => x.Customer_Code).AsNoTracking().ToListAsync();
            var rs = await _context.Customer
                .Where(x =>
                    (x.MainCode.Contains("Vendor")) ||
                    (!x.MainCode.Contains("Vendor") && x.SaleName != "NOMI"))
                .OrderByDescending(x => x.Customer_Code)
                .AsNoTracking()
                .ToListAsync();

            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }
        //return new List<M_Customer>();
    }
    public async Task<List<M_Customer>> GetList_allVendor(AuthUser user)
    {
        try
        {
            _context.ChangeTracker.Clear();
            //var rs = await _context.Customer.OrderByDescending(x => x.Customer_Code).AsNoTracking().ToListAsync();
            ////if (user.Department!.Contains("SALES"))
            ////    rs = rs.Where(x => !string.IsNullOrEmpty(x.SaleName) && (x.SaleName == "NOMI" || x.SaleName.ToUpper() == user.Usr.ToUpper())).OrderBy(x => x.SaleName).ToList();
            var rs = await _context.Customer
                .Where(x =>
                    (x.MainCode == "Vendor") ||
                    (x.MainCode != "Vendor" && x.SaleName != "NOMI"))
                .OrderByDescending(x => x.Customer_Code)
                .AsNoTracking()
                .ToListAsync();

            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }
        //return new List<M_Customer>();
    }

    public async Task<List<M_Customer>> GetList()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Customer.Where(x => x.SaleName != "NOMI").OrderByDescending(x => x.Customer_Code).AsNoTracking().ToListAsync();
            rs.Insert(0, new M_Customer { COMPANY = "" });
            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }
    }
    public async Task<List<M_Customer>> GetListBySale(AuthUser user)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Customer.OrderByDescending(x => x.Customer_Code).AsNoTracking().ToListAsync();
            if (user.Department!.Contains("ADMIN"))
                //rs = rs.Where(x => !string.IsNullOrEmpty(x.SaleName) && (x.SaleName == "NOMI" || x.SaleName.ToUpper() == user.Usr.ToUpper())).OrderBy(x => x.SaleName).ToList();

                rs = rs.Where(x =>
               !string.IsNullOrEmpty(x.MainCode) &&
               x.MainCode.Contains("Customer") &&
               x.SaleName != "NOMI")
             .OrderBy(x => x.SaleName)
             .ToList();

            else if (user.Department!.Contains("SALE"))
                //rs = rs.Where(x => x.SaleName != "NOMI").OrderBy(x => x.SaleName).ToList();
                rs = rs.Where(x => !string.IsNullOrEmpty(x.SaleName) && !string.IsNullOrEmpty(x.MainCode) && x.MainCode.Contains("Customer") && x.SaleName != "NOMI" && (x.SaleName.ToUpper() == user.Usr.ToUpper())).OrderBy(x => x.SaleName).ToList();
            else
                rs = rs.Where(x => !string.IsNullOrEmpty(x.SaleName) && !string.IsNullOrEmpty(x.MainCode) && x.MainCode.Contains("Customer") && x.SaleName != "NOMI").OrderBy(x => x.SaleName).ToList();

            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }
    }

    public async Task<List<M_Customer>> GetListBySale_vendor(AuthUser user)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Customer.OrderByDescending(x => x.Customer_Code).AsNoTracking().ToListAsync();
            if (user.Department!.Contains("ADMIN"))
                //rs = rs.Where(x => !string.IsNullOrEmpty(x.SaleName) && (x.SaleName == "NOMI" || x.SaleName.ToUpper() == user.Usr.ToUpper())).OrderBy(x => x.SaleName).ToList();

                rs = rs.Where(x => !string.IsNullOrEmpty(x.MainCode) && !x.MainCode.Contains("Customer")).OrderBy(x => x.SaleName).ToList();

            else if (user.Department!.Contains("SALE"))
                //rs = rs.Where(x => x.SaleName != "NOMI").OrderBy(x => x.SaleName).ToList();
                rs = rs.Where(x => !string.IsNullOrEmpty(x.SaleName) && !string.IsNullOrEmpty(x.MainCode) && !x.MainCode.Contains("Customer")).OrderBy(x => x.SaleName).ToList();
            else
                rs = rs.Where(x => !string.IsNullOrEmpty(x.SaleName) && !string.IsNullOrEmpty(x.MainCode) && !x.MainCode.Contains("Customer")).OrderBy(x => x.SaleName).ToList();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }
    }
   
    public async Task<List<M_Customer>> GetListNomi()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Customer.Where(x => x.SaleName == "NOMI" && !string.IsNullOrEmpty(x.MainCode) && x.MainCode.Contains("Customer")).OrderByDescending(x => x.Customer_Code).AsNoTracking().ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }
    }

    public async Task<List<M_Customer>> GetListNomi_by_tax(string conf)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Customer.Where(x => x.SaleName == "NOMI" && !string.IsNullOrEmpty(x.MainCode) && x.MainCode.Contains("Customer") && x.TaxCode.ToLower().Contains(conf.ToLower())).OrderByDescending(x => x.Customer_Code).AsNoTracking().ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }
    }

    public async Task<List<M_Customer>> GetListNomi_by_shortname(string conf)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Customer.Where(x => x.SaleName == "NOMI" && !string.IsNullOrEmpty(x.MainCode) && x.MainCode.Contains("Customer") && x.shortname.ToLower().Contains(conf.ToLower())).OrderByDescending(x => x.Customer_Code).AsNoTracking().ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }
    }

    public async Task<List<M_Customer>> GetListNomi_by_debitname(string conf)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Customer.Where(x => x.SaleName == "NOMI" && !string.IsNullOrEmpty(x.MainCode) && x.MainCode.Contains("Customer") && x.COMPANY.ToLower().Contains(conf.ToLower())).OrderByDescending(x => x.Customer_Code).AsNoTracking().ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }
    }

    public async Task<List<M_Customer>> GetListNomi_by_invoicename(string conf)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Customer.Where(x => x.SaleName == "NOMI" && !string.IsNullOrEmpty(x.MainCode) && x.MainCode.Contains("Customer") && x.EnglishName.ToLower().Contains(conf.ToLower())).OrderByDescending(x => x.Customer_Code).AsNoTracking().ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }
    }


    public async Task<List<M_Customer>> GetList_Bymaincus(string maincus)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Customer
                .Where(x => !string.IsNullOrEmpty(x.MainCode) && x.MainCode.Contains(maincus))
                .OrderByDescending(x => x.Customer_Code).AsNoTracking().ToListAsync();
            rs.Insert(0, new M_Customer { COMPANY = "" });
            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }
    }
    public List<M_Customer> GetList_cus_debit()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.Customer.Where(x => x.SaleName != "NOMI").OrderBy(x => x.COMPANY).Distinct().ToList();
            rs.Insert(0, new M_Customer { COMPANY = "" });
            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }

    }
    public List<M_Customer> GetList_cus_Shipping()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.Customer
                .Where(x => !string.IsNullOrEmpty(x.MainCode) && x.MainCode.Contains("Shipping") && x.SaleName != "NOMI")  // Thêm điều kiện lọc nếu cần
                .OrderBy(x => x.COMPANY)  // Sắp xếp theo COMPANY A-Z
                .GroupBy(x => x.COMPANY)  // Nhóm theo COMPANY để loại bỏ trùng lặp
                .Select(g => g.First())  // Chỉ lấy 1 bản ghi duy nhất cho mỗi COMPANY
                .ToList();

            rs.Insert(0, new M_Customer { COMPANY = "", shortname = "" });  // Thêm dòng rỗng

            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }

    }
    public List<M_Customer> GetList_cus_Coloader()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.Customer
                .Where(x => !string.IsNullOrEmpty(x.MainCode) && x.MainCode.Contains("Coloader") && x.SaleName != "NOMI")  // Thêm điều kiện lọc nếu cần
                .OrderBy(x => x.COMPANY)  // Sắp xếp theo COMPANY A-Z
                .GroupBy(x => x.COMPANY)  // Nhóm theo COMPANY để loại bỏ trùng lặp
                .Select(g => g.First())  // Chỉ lấy 1 bản ghi duy nhất cho mỗi COMPANY
                .ToList();

            rs.Insert(0, new M_Customer { COMPANY = "", shortname = "" });  // Thêm dòng rỗng

            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Customer>();
        }

    }
    public async Task<List<M_Sale>> GetList_sale()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Sale.OrderByDescending(x => x.SaleCode).ToListAsync();

            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Sale>();
        }
    }
    public async Task<List<M_Sale>> GetListSale()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Sale.OrderBy(x => x.SaleCode).ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Sale>();
        }
    }


    public async Task<List<CustomerMarket>> GetListCustomerMarket(Guid CustomerID)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.CustomerMarket.Where(x => x.Customer_ID == CustomerID).OrderByDescending(x => x.country_market).ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return null;
        }
    }



    public async Task<List<CustomerCommondity>> GetListCustomerCommondity(Guid CustomerID)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.CustomerCommondity.Where(x => x.Customer_ID == CustomerID && x.Continued == true).ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public string? GetCompanyFromID(Guid? Cus_ID)
    {
        try
        {
            if (Cus_ID == null)
                return null;

            _context.ChangeTracker.Clear();
            var rs = _context.Customer.Where(x => x.Customer_ID == Cus_ID && x.Continued == true).FirstOrDefault();
            return rs.PIC;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public string? GetCus_BankFromID(Guid? Cus_ID)
    {
        try
        {
            if (Cus_ID == null)
                return null;

            _context.ChangeTracker.Clear();
            var rs = _context.Customer.Where(x => x.Customer_ID == Cus_ID && x.Continued == true).FirstOrDefault();
            return rs.sotaikhoan;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public string? GetHSCodeCompanyFromID(Guid? Cus_ID)
    {
        try
        {
            if (Cus_ID == null)
                return "";

            _context.ChangeTracker.Clear();
            var rs = _context.Customer.Where(x => x.Customer_ID == Cus_ID && x.Continued == true).Select(x => x.MaHangKhaiBaoHSCode_gs).FirstOrDefault();
            return rs;
        }
        catch (Exception ex)
        {
            return "";
        }
    }
    public string? GetIdcusfromID(Guid? Cus_ID)
    {
        try
        {
            if (Cus_ID == null)
                return null;

            _context.ChangeTracker.Clear();
            var rs = _context.Customer.Where(x => x.Customer_ID == Cus_ID && x.Continued == true).FirstOrDefault();
            return rs.Email;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<string> GetMarketCodeFromID(Guid Market_ID)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.MARKET.Where(x => x.Market_ID == Market_ID && x.Continued == true).FirstOrDefaultAsync();
            return rs.MarketCode;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<List<M_PIC>> GetListPIC(Guid CustomerID)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.PIC.Where(x => x.Customer_ID == CustomerID && x.Continued == true).ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<List<M_Booking>> GetListSaleDetail(Guid CustomerID)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Booking.Where(x => x.Customer_ID == CustomerID && x.continued == true).ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<List<M_CustomerBr>> GetListAgentNetworkBranch(Guid CustomerID)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.CustomerBr.Where(x => x.customerid == CustomerID && x.continued == true).ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<List<M_DebitCreditCustomer>> GetListDebitCreditCustomer(Guid CustomerID)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.DebitCreditCustomer.Where(x => x.customerid == CustomerID && x.continued == true).ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<List<M_Buyer>> GetListBuyer(Guid CustomerID)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Buyer.Where(x => x.Customer_Id == CustomerID && x.Continued == "1").ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<List<CustomerReport>> GetListCustomerReport(Guid CustomerID)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.CUSTOMERREPORT.Where(x => x.Customer_ID == CustomerID && x.continued == true).ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<List<MARKET>> GetListMarket()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.MARKET.OrderByDescending(x => x.MarketCode).ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<List<M_DebitCreditCustomer>> GetListMatHang()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.DebitCreditCustomer.OrderBy(x => x.mathang).Distinct().ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<List<M_ListDept>> GetListDept()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.ListDept.OrderBy(x => x.Viewername).ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<List<M_CustomerBr>> GetListCustomerBr()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.CustomerBr.OrderBy(x => x.citybr).ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<BoolandMessReponse> UpdateOrCreate(M_Customer p, M_Customer P_Old, string loai)
    {
        try
        {
            p.Continued = true;
            p.Approve = p.Editable = false;
            string usr = asv.GetAuth().Result.User.Identity!.Name!;
            if (p.Customer_ID == null || p.Customer_ID == Guid.Empty)
            {
                _context.ChangeTracker.Clear();
                _context.Customer.Add(p);

                await _context.SaveChangesAsync();
                if (loai == "Duplicate")
                {
                    await HistoryLogService.LogAsync(usr, "Duplicate", "System-Customer", p.Customer_ID, p.Customer_Code, new { OldData = P_Old, NewData = p });
                }
                else
                {
                    await HistoryLogService.LogAsync(usr, "ADD", "System-Customer", p.Customer_ID, p.Customer_Code, p);
                }

                return new BoolandMessReponse(true, "Create Customer Success");
            }
            else
            {
                _context.ChangeTracker.Clear();
                _context.Customer.Update(p);
                await _context.SaveChangesAsync();
                await HistoryLogService.LogAsync(usr, "Update", "System-Customer", p.Customer_ID, p.Customer_Code, new { OldData = P_Old, NewData = p });
                return new BoolandMessReponse(true, "Update Customer Success");
            }
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Update or Add Customer with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> Delete(M_Customer p)
    {
        try
        {
            _context.ChangeTracker.Clear();
            if (p?.Customer_ID == null || p?.Customer_ID == Guid.Empty)
                return new BoolandMessReponse(false, "Nothing to Delete");

            _context?.Customer.Remove(p!);
            await _context?.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Delete successful");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Delete Customer with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> UpdateCustomerMarket(CustomerMarket c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            _context.CustomerMarket.Update(c);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Update CustomerMarket Success");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Update CustomerMarket with error code: " + ex.Message);
        }
    }
    public async Task<List<string>> queryCountry()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Customer.OrderBy(x => x.Country).Select(x => x.Country).Distinct().ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<string>();
        }
    }
    public string queryACountry(string text)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.Customer.OrderBy(x => x.Country).Select(x => x.Country).Distinct().FirstOrDefault();
            return rs;
        }
        catch (Exception ex)
        {
            return "";
        }
    }

    public async Task<List<string>> querySaleName()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Customer.OrderBy(x => x.SaleName).Select(x => x.SaleName).Distinct().ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<string>();
        }
    }
    public async Task<List<string>> querySaleName_fromUserlist()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.UserList.OrderBy(x => x.Name).Where(x=>x.Department== "SALES").Select(x => x.Name).Distinct().ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<string>();
        }
    }
    public async Task<BoolandMessReponse> CreateCustomerMarket(CustomerMarket c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            c.CustomerMarket_Id = Guid.NewGuid();
            _context.CustomerMarket.Add(c!);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Create CustomerMarket Success");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Add CustomerMarket with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> DeleteCustomerMarket(CustomerMarket c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            if (c?.CustomerMarket_Id == null || c?.CustomerMarket_Id == Guid.Empty)
                return new BoolandMessReponse(false, "Nothing to Delete");

            _context?.CustomerMarket.Remove(c!);
            await _context?.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Delete successful");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Delete CustomerMarket with error code: " + ex.Message);
        }
    }

    public async Task<BoolandMessReponse> UpdateCustomerCommondity(CustomerCommondity c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            _context.CustomerCommondity.Update(c);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Update Contract Success");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Update Contract with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> CreateCustomerCommondity(CustomerCommondity c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            c.CustomerCommondity_ID = Guid.NewGuid();
            _context.CustomerCommondity.Add(c!);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Create Contract Success");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Add Contract with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> DeleteCustomerCommondity(CustomerCommondity c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            if (c?.CustomerCommondity_ID == null || c?.CustomerCommondity_ID == Guid.Empty)
                return new BoolandMessReponse(false, "Nothing to Delete");

            _context?.CustomerCommondity.Remove(c!);
            await _context?.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Delete successful");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Delete Contract with error code: " + ex.Message);
        }
    }

    public async Task<List<(CustomerMarket cusmk, MARKET mk)>?> GetCustomerMarket(Guid CustomerID)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.CustomerMarket
                .Join(_context.MARKET,
                cusmk => new { cusmk.Market_ID },
                mk => new { mk.Market_ID },
                (cusmk, mk) => new { cusmk, mk })
                .Where(x => x.cusmk.Customer_ID == CustomerID)
                .ToListAsync();
            var list = new List<(CustomerMarket, MARKET)>();
            foreach (var item in rs)
                list.Add((item.cusmk, item.mk));
            return list;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public MARKET GetMarketfromID(Guid ID)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.MARKET.Where(x => x.Market_ID == ID).FirstOrDefault();
            return rs;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<(BoolandMessReponse, CustomerCode)> GetCustomerCode()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.CustomerCode.OrderBy(x => x.CustomerCode_Ref).Where(x => x.Used == false).FirstOrDefault();
            if (rs == null)
                return (new BoolandMessReponse(false, "Hết số Customer Code, vui lòng liên hệ admin hoặc nhập thủ công!"), null)!;
            await UpdateCustomerCode(rs!);
            return (new BoolandMessReponse(true, ""), rs)!;
        }
        catch (Exception ex)
        {
            return (new BoolandMessReponse(false, $"Xãy ra lỗi khi lấy customer code mã: {ex.Message}, vui lòng liên hệ admin hoặc nhập thủ công!"), null)!;
        }
    }

    public async Task<(BoolandMessReponse Response, M_Customer? Customer, bool IsCreated)> GetOrCreateCustomerForInvoiceAsync(
        string? buyerTaxCode,
        string? buyerName,
        string? buyerAddress,
        string userName)
    {
        try
        {
            var normalizedTaxCode = NormalizeTaxCodeForCompare(buyerTaxCode);
            if (string.IsNullOrWhiteSpace(normalizedTaxCode))
            {
                return (new BoolandMessReponse(false, "Buyer tax code is missing to find/create customer."), null, false);
            }

            _context.ChangeTracker.Clear();
            var existingCustomers = await _context.Customer
                .Where(x => x.Continued != false && !string.IsNullOrWhiteSpace(x.TaxCode))
                .AsNoTracking()
                .ToListAsync();

            var matchedCustomer = existingCustomers
                .FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(x.MainCode)
                    && x.MainCode.Contains("Customer", StringComparison.OrdinalIgnoreCase)
                    && NormalizeTaxCodeForCompare(x.TaxCode) == normalizedTaxCode);

            if (matchedCustomer != null)
            {
                return (new BoolandMessReponse(true, "Customer found by tax code."), matchedCustomer, false);
            }

            var customerCodeResult = await GetCustomerCode();
            if (!customerCodeResult.Item1.Flag || customerCodeResult.Item2 == null)
            {
                return (new BoolandMessReponse(false, "Cannot allocate a new customer code. Please contact admin or enter manually."), null, false);
            }

            var userDetail = asv.GetUserDetail();
            var saleNameForCustomer = string.IsNullOrWhiteSpace(userDetail?.Usr) ? userName : userDetail.Usr;

            var companyName = string.IsNullOrWhiteSpace(buyerName) ? $"CUSTOMER_{normalizedTaxCode}" : buyerName.Trim();
            var address = buyerAddress?.Trim() ?? string.Empty;
            var now = DateTime.Now;

            var customer = new M_Customer
            {
                Customer_ID = Guid.NewGuid(),
                Customer_Code = customerCodeResult.Item2.CustomerCode_Ref,
                MainCode = "Customer",
                SaleName = saleNameForCustomer,
                COMPANY = companyName,
                shortname = companyName,
                EnglishName = companyName,
                Address = address,
                addresstiengviet = address,
                TaxCode = normalizedTaxCode,
                Continued = true,
                Approve = false,
                Editable = false,
                UserID = userName,
                strUser = userName,
                Updatetime = now,
                ngaythem = now,
                Type = "HQ"
            };

            _context.ChangeTracker.Clear();
            _context.Customer.Add(customer);
            await _context.SaveChangesAsync();
            await HistoryLogService.LogAsync(userName, "ADD", "System-Customer", customer.Customer_ID, customer.Customer_Code, customer);

            return (new BoolandMessReponse(true, "New customer created from invoice successfully."), customer, true);
        }
        catch (Exception)
        {
            return (new BoolandMessReponse(false, "Error creating customer from invoice."), null, false);
        }
    }

    private static string NormalizeTaxCodeForCompare(string? taxCode)
    {
        if (string.IsNullOrWhiteSpace(taxCode))
            return string.Empty;

        var noSpace = Regex.Replace(taxCode, @"\s+", string.Empty);
        noSpace = noSpace.Replace(".", string.Empty);
        return noSpace.Trim().ToUpperInvariant();
    }

    public async Task<BoolandMessReponse> UpdateCustomerCode(CustomerCode c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            c.Used = true;
            _context?.CustomerCode.Update(c!);
            await _context?.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Update successful");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Update fail with error code: " + ex.Message);
        }
    }

    public async Task<BoolandMessReponse> UpdateCustomer(M_Customer c)
    {
        try
        {
            _context.ChangeTracker.Clear();

            _context?.Customer.Update(c!);
            await _context?.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Update successful");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Update fail with error code: " + ex.Message);
        }
    }

    public async Task<BoolandMessReponse> UpdatePIC(M_PIC c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            _context.PIC.Update(c);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Update PIC Success");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Update PIC with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> CreatePIC(M_PIC c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            c.PIC_ID = Guid.NewGuid();
            _context.PIC.Add(c!);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Create PIC Success");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Add PIC with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> DeletePIC(M_PIC c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            if (c?.PIC_ID == null || c?.PIC_ID == Guid.Empty)
                return new BoolandMessReponse(false, "Nothing to Delete");

            _context?.PIC.Remove(c!);
            await _context?.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Delete successful");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Delete PIC with error code: " + ex.Message);
        }
    }

    public async Task<BoolandMessReponse> UpdateSaleDetail(M_Booking c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            _context.Booking.Update(c);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Update SaleDetail Success");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Update SaleDetail with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> CreateSaleDetail(M_Booking c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            c.Booking_ID = Guid.NewGuid();
            _context.Booking.Add(c!);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Create SaleDetail Success");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Add SaleDetail with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> DeleteSaleDetail(M_Booking c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            if (c?.Booking_ID == null || c?.Booking_ID == Guid.Empty)
                return new BoolandMessReponse(false, "Nothing to Delete");

            _context?.Booking.Remove(c!);
            await _context?.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Delete successful");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Delete SaleDetail with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> UpdateCustomerReport(CustomerReport c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            _context.CUSTOMERREPORT.Update(c);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Update CustomerReport Success");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Update CustomerReport with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> CreateCustomerReport(CustomerReport c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            c.CusReport_ID = Guid.NewGuid();
            _context.CUSTOMERREPORT.Add(c!);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Create CustomerReport Success");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Add CustomerReport with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> DeleteCustomerReport(CustomerReport c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            if (c?.CusReport_ID == null || c?.CusReport_ID == Guid.Empty)
                return new BoolandMessReponse(false, "Nothing to Delete");

            _context?.CUSTOMERREPORT.Remove(c!);
            await _context?.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Delete successful");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Delete CustomerReport with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> UpdateBuyer(M_Buyer c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            _context.Buyer.Update(c);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Update Buyer Success");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Update Buyer with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> CreateBuyer(M_Buyer c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            c.BuyerID = Guid.NewGuid();
            _context.Buyer.Add(c!);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Create Buyer Success");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Add Buyer with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> DeleteBuyer(M_Buyer c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            if (c?.BuyerID == null || c?.BuyerID == Guid.Empty)
                return new BoolandMessReponse(false, "Nothing to Delete");

            _context?.Buyer.Remove(c!);
            await _context?.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Delete successful");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Delete Buyer with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> UpdateCreditDebitCus(M_DebitCreditCustomer c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            _context.DebitCreditCustomer.Update(c);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Update Credit/Debit Customer Success");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Update Credit/Debit Customer with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> CreateCreditDebitCus(M_DebitCreditCustomer c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            c.debitcreditcustomerid = Guid.NewGuid();
            _context.DebitCreditCustomer.Add(c!);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Create Credit/Debit Customer Success");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Add Credit/Debit Customer with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> DeleteCreditDebitCus(M_DebitCreditCustomer c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            if (c?.debitcreditcustomerid == null || c?.debitcreditcustomerid == Guid.Empty)
                return new BoolandMessReponse(false, "Nothing to Delete");

            _context?.DebitCreditCustomer.Remove(c!);
            await _context?.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Delete successful");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Delete Credit/Debit Customer with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> UpdateAgentNetworkBranch(M_CustomerBr c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            _context.CustomerBr.Update(c);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Update AgentNetworkBranch Customer Success");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Update AgentNetworkBranch Customer with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> CreateAgentNetworkBranch(M_CustomerBr c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            c.customerBrID = Guid.NewGuid();
            _context.CustomerBr.Add(c!);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Create AgentNetworkBranch Customer Success");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Add AgentNetworkBranch Customer with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> DeleteAgentNetworkBranch(M_CustomerBr c)
    {
        try
        {
            _context.ChangeTracker.Clear();
            if (c?.customerBrID == null || c?.customerBrID == Guid.Empty)
                return new BoolandMessReponse(false, "Nothing to Delete");

            _context?.CustomerBr.Remove(c!);
            await _context?.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Delete successful");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Delete AgentNetworkBranch Customer with error code: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> UploadfiletoFTP(IBrowserFile file, Guid? CustomerCommondityID, string UserUpload)
    {
        try
        {
            //ftp server details
            var ftpServer = "ftp://115.165.166.130";
            var ftpUsername = "hinh";
            var ftpPassword = "qweQWE123!@#";
            var fileName = file.Name;
            int dotIndex = fileName.IndexOf('.');

            string namePart = fileName.Substring(0, dotIndex);
            string extensionPart = fileName.Substring(dotIndex);

            CustomerCommondityFileUpload info = new CustomerCommondityFileUpload();
            info.FileID = System.Guid.NewGuid();
            info.FileName = fileName;
            info.Link = $"{ftpServer}/{namePart}-{info.FileID}{extensionPart}";
            info.CustomerCommondityID = CustomerCommondityID;
            info.UserUpdate = UserUpload;
            info.DateUpdate = System.DateTime.Now;

            // Create FTP request
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(info.Link);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

            // Upload file in chunks (streaming)
            try
            {
                using (Stream ftpStream = await request.GetRequestStreamAsync())
                {
                    using (Stream fileStream = file.OpenReadStream(maxAllowedSize: long.MaxValue))
                    {
                        byte[] buffer = new byte[8192]; // 8KB buffer
                        int bytesRead;

                        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await ftpStream.WriteAsync(buffer, 0, bytesRead);
                        }
                    }
                }

                // Nhận phản hồi từ server sau khi upload
                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode == FtpStatusCode.ClosingData)
                    {
                        Console.WriteLine("Upload thành công!");
                    }
                    else
                    {
                        Console.WriteLine($"Upload không thành công. Trạng thái: {response.StatusDescription}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi upload file: {ex.Message}");
                return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
            }

            // Thêm đối tượng vào DbSet và lưu vào cơ sở dữ liệu
            var rs = await InsertCustommerCommodityFileUpload(info);
            if (!rs.Flag)
                return new BoolandMessReponse(false, rs.Message);

            return new BoolandMessReponse(true, "Upload Successful!");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
        }
    }
    public async Task<BoolandMessReponse> InsertCustommerCommodityFileUpload(CustomerCommondityFileUpload infofile)
    {
        try
        {

            _context.ChangeTracker.Clear();
            _context.CustomerCommondityFileUpload.Add(infofile);
            await _context.SaveChangesAsync();
            return new BoolandMessReponse(true, "Add File Success");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Add File Fail with Error Code: " + ex.Message);

        }

    }
    public List<CustomerCommondityFileUpload> GetListCustomerCommondityFileUpload(Guid? CustomerCommondityID)
    {
        var rs = _context.CustomerCommondityFileUpload.Where(x => x.CustomerCommondityID == CustomerCommondityID).OrderBy(x => x.DateUpdate).ToList();
        return rs;
    }
    public async Task<BoolandMessReponse> DeleteLink(CustomerCommondityFileUpload item)
    {
        _context.ChangeTracker.Clear();
        try
        {
            if (item?.FileID == null || item?.FileID == Guid.Empty)
                return new BoolandMessReponse(false, "Nothing to Delete");

            _context!.CustomerCommondityFileUpload.Remove(item!);
            await _context!.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Deleted");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot delete with error code: " + ex.Message);

        }
    }
    public async Task<List<M_Sale>> ListSale()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Sale.ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_Sale>();
        }
    }
    public async Task<List<string>> InsertCSV(IBrowserFile file)
    {
        try
        {
            if (file != null)
            {
                string filePath = "";
                string fileName = "";
                using (var stream = file.OpenReadStream(maxAllowedSize: long.MaxValue))
                {
                    // Lấy đường dẫn tệp
                    fileName = Path.GetFileName(file.Name);
                    filePath = Path.Combine(Path.GetTempPath(), fileName);

                    // Lưu tệp vào thư mục tạm
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
                string fileType = file.ContentType;
                switch (fileType)
                {
                    case "application/vnd.ms-excel":
                        fileType = "xls";
                        break;
                    case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                        fileType = "xlsx";
                        break;
                }
                if (fileType == "xlsx")
                {
                    var SDB = await ReadDataFromExcelXLSX(filePath, fileName);
                    if (SDB == null) return ["Can't Read File", "0"];
                    _context.ChangeTracker.Clear();
                    _context.AddRange(SDB);
                    await _context.SaveChangesAsync();
                    return ["Import Data Successfully", "1"];
                }
                else if (fileType == "xls")
                {

                    var SDB = await ReadDataFromExcelXLS(filePath, fileName);
                    if (SDB == null) return ["Can't Read File", "0"];
                    _context.ChangeTracker.Clear();
                    _context.AddRange(SDB);
                    await _context.SaveChangesAsync();
                    return ["Import Data Successfully", "1"];

                }

                return ["Can't Read File", "0"];
            }
            else
            {
                return ["Import Data Fail", "0"];
            }
        }
        catch (Exception ex)
        {
            return ["Import Data Fail with error code: " + ex.Message, "0"];
        }
    }
    public async Task<List<M_Customer>> ReadDataFromExcelXLS(string filePath, string fileName)
    {
        List<M_Customer> ListsCSV = new List<M_Customer>();
        try
        {
            FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            DataSet dt = excelReader.AsDataSet();
            excelReader.Close();

            foreach (System.Data.DataTable table in dt.Tables)
            {

                for (int row = 1; row < table.Rows.Count; row++) // Bắt đầu từ dòng 2
                {
                    int column = 0;
                    if (table.Rows[row][0]?.ToString() == null || table.Rows[row][0]?.ToString() == "")
                        return ListsCSV;
                    M_Customer sbd = new M_Customer();

                    var rs = await GetCustomerCode();
                    if (!rs.Item1.Flag)
                    {
                        // hết ref
                        return ListsCSV;
                    }
                    var refno = rs.Item2.CustomerCode_Ref;
                    sbd.Customer_Code = refno;
                    column++;
                    sbd.MainCode = table.Rows[row][column++]?.ToString();
                    //sbd.Country = table.Rows[row][column++]?.ToString();

                    //sbd.shortname = table.Rows[row][column++]?.ToString();
                    sbd.SaleName = table.Rows[row][column++]?.ToString();

                    sbd.COMPANY = table.Rows[row][column++]?.ToString();
                    sbd.addresstiengviet = table.Rows[row][column++]?.ToString();
                    sbd.Tel = table.Rows[row][column++]?.ToString();
                    sbd.Fax = table.Rows[row][column++]?.ToString();

                    sbd.EnglishName = table.Rows[row][column++]?.ToString();
                    sbd.Address = table.Rows[row][column++]?.ToString();

                    sbd.TaxCode = table.Rows[row][column++]?.ToString();
                    sbd.Email = table.Rows[row][column++]?.ToString();

                    sbd.Remarks_Customer = table.Rows[row][column++]?.ToString();
                    sbd.hancongno = table.Rows[row][column++]?.ToString();

                    sbd.kpis = table.Rows[row][column++]?.ToString();
                    //sbd.MaHangKhaiBaoHSCode_gs = table.Rows[row][column++]?.ToString();
                    //sbd.commodity = table.Rows[row][column++]?.ToString();
                    //sbd.tenNuocXuatXu_gs = table.Rows[row][column++]?.ToString();

                    //sbd.PhuongTienVanChuyen_gs = table.Rows[row][column++]?.ToString();
                    //sbd.tendiadiemnhanhangcuoicung_gs = table.Rows[row][column++]?.ToString();
                    //sbd.tendiadiemxephang_gs = table.Rows[row][column++]?.ToString();

                    ListsCSV.Add(sbd);
                }
            }
            return ListsCSV;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<List<M_Customer>> ReadDataFromExcelXLSX(string filePath, string fileName)
    {
        try
        {
            List<M_Customer> ListsCSV = new List<M_Customer>();
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = package.Workbook.Worksheets[0];
                if (sheet.Dimension != null && sheet.Dimension.Rows > 0 && sheet.Dimension.Columns > 0)
                {
                    //row la dòng, column là cột
                    for (int row = 2; row <= sheet.Dimension.End.Row; row++) // Bắt đầu từ dòng 2
                    {
                        M_Customer sbd = new M_Customer();
                        if (sheet.Cells[row, 4].Value?.ToString() == null || sheet.Cells[row, 4].Value?.ToString() == "")
                            return ListsCSV;
                        int column = 2;
                        var rs = await GetCustomerCode();
                        if (!rs.Item1.Flag)
                        {
                            // hết ref
                            return ListsCSV;
                        }

                        var refno = rs.Item2.CustomerCode_Ref;
                        try
                        {
                            sbd.Customer_Code = refno;
                        }
                        catch
                        {
                            sbd.Customer_Code = "0";
                        }
                        try
                        {
                            sbd.tilecom = 0;
                        }
                        catch
                        {
                            sbd.tilecom = 0;
                        }
                        try
                        {
                            //sbd.MainCode = sheet.Cells[row, column++].Value?.ToString();
                            sbd.MainCode = "Customer";
                        }
                        catch
                        {
                            sbd.MainCode = "Customer";
                        }
                        try
                        {
                            sbd.SaleName = "NOMI";
                        }
                        catch
                        {
                            sbd.SaleName = "NOMI";
                        }
                        //sbd.Country = sheet.Cells[row, column++].Value?.ToString();                        
                        try
                        {
                            column++;
                            sbd.shortname = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.shortname = "";
                        }

                        try
                        {
                            column++;
                            sbd.EnglishName = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.EnglishName = "";
                        }

                        try
                        {
                            column++;
                            sbd.addresstiengviet = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.addresstiengviet = "";
                        }

                        try
                        {
                            column++;
                            if (sheet.Cells[row, column].Value is null)
                                sbd.Tel = "";
                            else
                            {
                                if (sheet.Cells[row, column].Value.ToString().Length == 9 || sheet.Cells[row, column].Value.ToString().Length == 10)
                                    sbd.Tel = "0" + sheet.Cells[row, column].Value.ToString();
                                else if (sheet.Cells[row, column].Value.ToString().Length >= 50)
                                    sbd.Tel = "";
                                else
                                    sbd.Tel = sheet.Cells[row, column].Value?.ToString();
                            }

                        }
                        catch
                        {
                            sbd.Tel = "";
                        }

                        try
                        {
                            column++;
                            if (sheet.Cells[row, column].Value is null)
                                sbd.Fax = "";
                            else
                            {
                                if (sheet.Cells[row, column].Value.ToString().Length == 9 || sheet.Cells[row, column].Value.ToString().Length == 10)
                                    sbd.Fax = "0" + sheet.Cells[row, column].Value.ToString();
                                else if (sheet.Cells[row, column].Value.ToString().Length >= 50)
                                    sbd.Fax = "";
                                else
                                    sbd.Fax = sheet.Cells[row, column].Value?.ToString();
                            }
                        }
                        catch
                        {
                            sbd.Fax = "";
                        }
                        try
                        {
                            column++;
                            sbd.COMPANY = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.COMPANY = "";
                        }
                        try
                        {
                            column++;
                            sbd.Address = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.Address = "";
                        }
                        try
                        {
                            column++;
                            if (sheet.Cells[row, column].Value?.ToString().Length == 9 || sheet.Cells[row, column].Value?.ToString().Length == 12)
                                sbd.TaxCode = "0" + sheet.Cells[row, column].Value?.ToString();
                            else
                                sbd.TaxCode = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.TaxCode = "";
                        }
                        try
                        {
                            column++;
                            sbd.Email = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.Email = "";
                        }
                        try
                        {
                            column++;
                            sbd.Remarks_Customer = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.Remarks_Customer = "";
                        }
                        try
                        {
                            column++;
                            sbd.hancongno = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.hancongno = "";
                        }
                        try
                        {
                            column++;
                            sbd.DonViDoiTac_gs = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.DonViDoiTac_gs = "";
                        }
                        try
                        {
                            column++;
                            sbd.TenNoiMoToKhai_gs = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.TenNoiMoToKhai_gs = "";
                        }
                        try
                        {
                            column++;
                            sbd.MaHangKhaiBaoHSCode_gs = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.MaHangKhaiBaoHSCode_gs = "";
                        }
                        try
                        {
                            column++;
                            sbd.TenHang_gs = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.TenHang_gs = "";
                        }
                        try
                        {
                            column++;
                            sbd.tenNuocXuatXu_gs = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.tenNuocXuatXu_gs = "";
                        }
                        try
                        {
                            column++;
                            sbd.dieuKienGiaoHang_gs = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.dieuKienGiaoHang_gs = "";
                        }
                        try
                        {
                            column++;
                            sbd.PhuongTienVanChuyen_gs = sheet.Cells[row, column].Value?.ToString();
                        }
                        catch
                        {
                            sbd.PhuongTienVanChuyen_gs = "";
                        }


                        ListsCSV.Add(sbd);
                    }
                }
            }
            return ListsCSV;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<BoolandMessReponse> MoveCustomer(MoveCustomer movecus, M_Customer item)
    {
        _context.ChangeTracker.Clear();
        try
        {
            if (item?.Customer_ID == null || item?.Customer_ID == Guid.Empty)
                return new BoolandMessReponse(false, "Nothing to move");
            _context.MoveCustomer.Add(movecus);
            _context.Customer.Update(item!);
            await _context!.SaveChangesAsync()!;
            return new BoolandMessReponse(true, "Move customer successfully!");

        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Move customer with error code: " + ex.Message);

        }
    }

    public async Task<(BoolandMessReponse, int)> CheckAndRefundMoveCustomer()
    {
        try
        {
            _context.ChangeTracker.Clear();

            // danh sách quá 30 ngày
            var listmovecus = await _context.MoveCustomer.Join(_context.Customer,
                move => move.CustomerID,
                cus => cus.Customer_ID,
                (move, cus) => new { move, cus })
            .Where(x => x.move.MoveDate.HasValue && EF.Functions.DateDiffDay(x.move.MoveDate.Value, DateTime.Now) > (x.cus.thoihanmove ?? 30)) // nếu là null thì lấy 30 ngày
            .Select(x => x.move).ToListAsync();
            //var listmovecus = await _context.MoveCustomer.Where(x => EF.Functions.DateDiffDay(x.MoveDate!.Value, DateTime.Now) > 30).ToListAsync();

            if (listmovecus.Count == 0)
                return (new BoolandMessReponse(true, $"There are no overdue customers in the sales list."), 0);

            var listcustomer = new List<M_Customer>();
            // kiểm tra trong HBL có cusid và salename này không
            foreach (var movecus in listmovecus)
            {
                var exist = await _context.HBL.AnyAsync(x => x.CustomerID == movecus.ID && x.SaleName == movecus.SaleName);
                if (!exist) // không phát sinh dịch vụ
                {
                    // cập nhật sale của customer về nomi
                    var cus = await _context.Customer.FirstOrDefaultAsync(x => x.Customer_ID == movecus.CustomerID);
                    if (cus != null)
                    {
                        cus.SaleName = "NOMI";
                        cus.Updatetime = DateTime.Now;
                        listcustomer.Add(cus);
                    }
                }
                else //nếu có phát sinh dịch vụ thì xóa khỏi list sắp xóa
                {
                    listmovecus.Remove(movecus);
                }
            }
            _context.MoveCustomer.RemoveRange(listmovecus);
            _context.Customer.UpdateRange(listcustomer);
            var effect = await _context.SaveChangesAsync();

            return (new BoolandMessReponse(true, $"{listcustomer.Count} overdue customers updated to customer public list "), effect);
        }
        catch (Exception ex)
        {
            return (new BoolandMessReponse(false, "Cannot update customer to customer public list with error code: " + ex.Message), 0);
        }
    }

    public async Task<List<MoveCustomer>> GetMoveCustomer()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.MoveCustomer.ToListAsync();
            return rs;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new List<MoveCustomer>();
        }
    }
    public async Task<BoolandMessReponse> Delete_cus_in_movecus(Guid cusid)
    {
        try
        {
            _context.ChangeTracker.Clear();

            var customer = await _context.MoveCustomer.FirstOrDefaultAsync(x => x.CustomerID == cusid);
            if (customer == null)
            {
                return new BoolandMessReponse(false, "Nothing to Delete");
            }

            _context.MoveCustomer.Remove(customer);
            await _context.SaveChangesAsync();

            return new BoolandMessReponse(true, "Delete successful");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Cannot Delete Customer with error code: " + ex.Message);
        }
    }

    public async Task<BoolandMessReponse> CheckMoveCusOver100(string salename)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.MoveCustomer.Where(x => x.SaleName!.ToUpper() == salename.ToUpper()).ToListAsync();
            if (rs.Count > 100)
                return new BoolandMessReponse(false, salename + "Has exceeded 100 customers");
            return new BoolandMessReponse(true, "OK");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, ex.Message);
        }
    }
    public string queryShipperFromMST(string mst_Shipper)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.Customer
                .Where(x => x.TaxCode.Contains(mst_Shipper) || x.EnglishName.Contains(mst_Shipper) && x.MainCode.Contains("Shipping"))
                .Select(x => x.EnglishName + "\n" + x.addresstiengviet + "\n" + x.Tel + "\n" + x.Fax).Distinct()
                .FirstOrDefault();
            return rs;
        }
        catch (Exception ex)
        {
            return "";
        }
    }
    public List<string> queryListShipperFromMST()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.Customer
                .Select(x => x.EnglishName + "\n" + x.addresstiengviet + "\n" + x.Tel + "\n" + x.Fax).Distinct()
                .ToList();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<string>();
        }
    }
    public List<string> queryListShippers()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.Customer
                .Select(x => x.EnglishName + "\n" + x.addresstiengviet + "\n" + x.Tel + "\n" + x.Fax).Distinct()
                .ToList();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<string>();
        }
    }
    public List<string> queryListPorts()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.Port
                .OrderByDescending(x => x.UPDATETIME)
                .Select(x => x.PORT).Distinct()
                .ToList();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<string>();
        }
    }

    /// <summary>Khớp đúng tên cảng (PORT) để lấy PORT_CODE khi import bill / autocomplete.</summary>
    public PortModel? GetPortByExactPortName(string? portName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(portName))
                return null;
            _context.ChangeTracker.Clear();
            var n = portName.Trim();
            return _context.Port
                .AsNoTracking()
                .FirstOrDefault(x => x.PORT != null && x.PORT == n);
        }
        catch
        {
            return null;
        }
    }
    public List<string> queryListVessels()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.MBL
                .Select(x => x.Vessel).Distinct()
                .ToList();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<string>();
        }
    }
    public List<string> queryListVoys()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.MBL
                .Select(x => x.Voy).Distinct()
                .ToList();
            return rs;
        }
        catch (Exception ex)
        {
            return new List<string>();
        }
    }
    public string queryConsigneeFromMST(string mst_Shipper)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.Customer
                .Where(x => x.TaxCode.Contains(mst_Shipper) || x.EnglishName.Contains(mst_Shipper) && x.MainCode.Contains("Shipping"))
                .Select(x => x.EnglishName + "\n" + x.addresstiengviet + "\n" + x.Tel + "\n" + x.Fax).Distinct()
                .FirstOrDefault();
            return rs;
        }
        catch (Exception ex)
        {
            return "";
        }
    }
    public string queryNotifyFromMST(string mst_Shipper)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = _context.Customer
                .Where(x => x.TaxCode.Contains(mst_Shipper) || x.EnglishName.Contains(mst_Shipper))
                .Select(x => x.EnglishName + "\n" + x.addresstiengviet).Distinct()
                .FirstOrDefault();
            return rs;
        }
        catch (Exception ex)
        {
            return "";
        }
    }
    public async Task<List<M_SaleCustomerCount>> GetCustomerCountBySale(DateTime? fromDate, DateTime? toDate, string salename)
    {
        try
        {
            _context.ChangeTracker.Clear();

            var rs = await (from movecus in _context.MoveCustomer
                            join cus in _context.Customer on movecus.CustomerID equals cus.Customer_ID
                            where cus != null && cus.MainCode.Contains("Customer") &&
                            ((!fromDate.HasValue || movecus.MoveDate >= fromDate.Value) && (!toDate.HasValue || movecus.MoveDate <= toDate.Value)) &&
                            movecus.SaleName == salename

                            group movecus by movecus.SaleName into g
                            select new M_SaleCustomerCount
                            {
                                SaleName = g.Key,
                                CustomerCount = g.Count()
                            }).ToListAsync();



            return rs;
        }
        catch (Exception ex)
        {
            return new List<M_SaleCustomerCount>();
        }
    }
    public async Task<List<M_SaleCustomerCount>> GetCustomerCount_ByMonth(DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            AuthUser userdetail;
            userdetail = asv.GetUserDetail();
            _context.ChangeTracker.Clear();
            if (userdetail.Department!.Contains("SALES"))
            {
                var rs = await (from movecus in _context.MoveCustomer
                                join cus in _context.Customer on movecus.CustomerID equals cus.Customer_ID
                                where cus != null &&
                                      cus.MainCode.Contains("Customer") &&
                                      movecus.MoveDate != null && // đảm bảo MoveDate không null
                                      (movecus.SaleName.ToUpper() == userdetail.Usr.ToUpper()) &&
                                      (!fromDate.HasValue || movecus.MoveDate >= fromDate.Value) &&
                                      (!toDate.HasValue || movecus.MoveDate <= toDate.Value)
                                group movecus by new
                                {
                                    movecus.SaleName,
                                    Year = movecus.MoveDate.Value.Year,
                                    Month = movecus.MoveDate.Value.Month
                                } into g
                                select new M_SaleCustomerCount
                                {
                                    SaleName = g.Key.SaleName,
                                    Year = g.Key.Year,
                                    Month = g.Key.Month,
                                    CustomerCount = g.Count()
                                }).ToListAsync();


                return rs;
            }
            else
            {
                var rs = await (from movecus in _context.MoveCustomer
                                join cus in _context.Customer on movecus.CustomerID equals cus.Customer_ID
                                where cus != null &&
                                      cus.MainCode.Contains("Customer") &&
                                      movecus.MoveDate != null && // đảm bảo MoveDate không null
                                      (!fromDate.HasValue || movecus.MoveDate >= fromDate.Value) &&
                                      (!toDate.HasValue || movecus.MoveDate <= toDate.Value)
                                group movecus by new
                                {
                                    movecus.SaleName,
                                    Year = movecus.MoveDate.Value.Year,
                                    Month = movecus.MoveDate.Value.Month
                                } into g
                                select new M_SaleCustomerCount
                                {
                                    SaleName = g.Key.SaleName,
                                    Year = g.Key.Year,
                                    Month = g.Key.Month,
                                    CustomerCount = g.Count()
                                }).ToListAsync();


                return rs;
            }
        }
        catch (Exception ex)
        {
            // Có thể log lỗi tại đây
            return new List<M_SaleCustomerCount>();
        }
    }

    public async Task<List<M_SaleCustomerCount>> GetCustomerCount_ByMonth_bysale(DateTime? fromDate, DateTime? toDate, string? salename)
    {
        try
        {
            AuthUser userdetail;
            userdetail = asv.GetUserDetail();
            _context.ChangeTracker.Clear();
            if (userdetail.Department!.Contains("SALES"))
            {
                var rs = await (from movecus in _context.MoveCustomer
                                join cus in _context.Customer on movecus.CustomerID equals cus.Customer_ID
                                where cus != null &&
                                      cus.MainCode.Contains("Customer") &&
                                      movecus.MoveDate != null && // đảm bảo MoveDate không null
                                       (movecus.SaleName.ToUpper() == userdetail.Usr.ToUpper()) &&
                                      (!fromDate.HasValue || movecus.MoveDate >= fromDate.Value) &&
                                      (!toDate.HasValue || movecus.MoveDate <= toDate.Value) && movecus.SaleName == salename
                                group movecus by new
                                {
                                    movecus.SaleName,
                                    Year = movecus.MoveDate.Value.Year,
                                    Month = movecus.MoveDate.Value.Month
                                } into g
                                select new M_SaleCustomerCount
                                {
                                    SaleName = g.Key.SaleName,
                                    Year = g.Key.Year,
                                    Month = g.Key.Month,
                                    CustomerCount = g.Count()
                                }).ToListAsync();


                return rs;
            }
            else
            {
                var rs = await (from movecus in _context.MoveCustomer
                                join cus in _context.Customer on movecus.CustomerID equals cus.Customer_ID
                                where cus != null &&
                                      cus.MainCode.Contains("Customer") &&
                                      movecus.MoveDate != null && // đảm bảo MoveDate không null
                                      (!fromDate.HasValue || movecus.MoveDate >= fromDate.Value) &&
                                      (!toDate.HasValue || movecus.MoveDate <= toDate.Value) && movecus.SaleName == salename
                                group movecus by new
                                {
                                    movecus.SaleName,
                                    Year = movecus.MoveDate.Value.Year,
                                    Month = movecus.MoveDate.Value.Month
                                } into g
                                select new M_SaleCustomerCount
                                {
                                    SaleName = g.Key.SaleName,
                                    Year = g.Key.Year,
                                    Month = g.Key.Month,
                                    CustomerCount = g.Count()
                                }).ToListAsync();


                return rs;
            }
        }
        catch (Exception ex)
        {
            // Có thể log lỗi tại đây
            return new List<M_SaleCustomerCount>();
        }
    }
    public async Task<List<MoveCustomer>> GetMoveCustomersbytime_allsale(DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            AuthUser userdetail;
            userdetail = asv.GetUserDetail();
            _context.ChangeTracker.Clear();
            if (userdetail.Department!.Contains("SALES"))
            {
                var rs = await (from movecus in _context.MoveCustomer
                                join cus in _context.Customer on movecus.CustomerID equals cus.Customer_ID
                                where cus != null && cus.MainCode.Contains("Customer") &&
                                (movecus.SaleName.ToUpper() == userdetail.Usr.ToUpper()) &&
                                ((!fromDate.HasValue || movecus.MoveDate >= fromDate.Value) && (!toDate.HasValue || movecus.MoveDate <= toDate.Value))
                                select movecus
               ).ToListAsync();

                return rs;
            }
            else
            {
                var rs = await (from movecus in _context.MoveCustomer
                                join cus in _context.Customer on movecus.CustomerID equals cus.Customer_ID
                                where cus != null && cus.MainCode.Contains("Customer") &&
                                ((!fromDate.HasValue || movecus.MoveDate >= fromDate.Value) && (!toDate.HasValue || movecus.MoveDate <= toDate.Value))
                                select movecus
               ).ToListAsync();

                return rs;
            }
        }
        catch (Exception ex)
        {
            // Log lỗi nếu cần
            return new List<MoveCustomer>();
        }

    }
    public async Task<List<MoveCustomer>> GetMoveCustomersbytime_By_sale(DateTime? fromDate, DateTime? toDate, string? salename)
    {
        try
        {
            AuthUser userdetail;
            userdetail = asv.GetUserDetail();
            _context.ChangeTracker.Clear();
            if (userdetail.Department!.Contains("SALES"))
            {
                var rs = await (from movecus in _context.MoveCustomer
                                join cus in _context.Customer on movecus.CustomerID equals cus.Customer_ID
                                where cus != null && cus.MainCode.Contains("Customer") &&
                                (movecus.SaleName.ToUpper() == userdetail.Usr.ToUpper()) &&
                                ((!fromDate.HasValue || movecus.MoveDate >= fromDate.Value) && (!toDate.HasValue || movecus.MoveDate <= toDate.Value)) && movecus.SaleName == salename
                                select movecus
               ).ToListAsync();
                return rs;
            }
            else
            {
                var rs = await (from movecus in _context.MoveCustomer
                                join cus in _context.Customer on movecus.CustomerID equals cus.Customer_ID
                                where cus != null && cus.MainCode.Contains("Customer") &&
                                ((!fromDate.HasValue || movecus.MoveDate >= fromDate.Value) && (!toDate.HasValue || movecus.MoveDate <= toDate.Value)) && movecus.SaleName == salename
                                select movecus
              ).ToListAsync();
                return rs;
            }



        }
        catch (Exception ex)
        {
            // Log lỗi nếu cần
            return new List<MoveCustomer>();
        }

    }
    public async Task<List<Components.CUSTOMER.Pages.Index.CustomerPreviewModel>> PreviewCustomerFromExcelAsync(Stream fileStream)
    {
        var result = new List<Components.CUSTOMER.Pages.Index.CustomerPreviewModel>();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets[0];

        int row = 2;
        while (true)
        {


            var Shortname = worksheet.Cells[row, 4].Text?.Trim();
            if (string.IsNullOrEmpty(Shortname)) break;

            var preview = new Components.CUSTOMER.Pages.Index.CustomerPreviewModel
            {
                STT = worksheet.Cells[row, 1].Text?.Trim(),
                MainCode = worksheet.Cells[row, 2].Text?.Trim(),
                SaleName = worksheet.Cells[row, 3].Text?.Trim(),
                ShortName = worksheet.Cells[row, 4].Text?.Trim(),
                EnglishName = worksheet.Cells[row, 5].Text?.Trim(),
                AddressTV = worksheet.Cells[row, 6].Text?.Trim(),
                Tel = worksheet.Cells[row, 7].Text?.Trim(),
                Fax = worksheet.Cells[row, 8].Text?.Trim(),
                Company = worksheet.Cells[row, 9].Text?.Trim(),
                Address = worksheet.Cells[row, 10].Text?.Trim(),
                Taxcode = worksheet.Cells[row, 11].Text?.Trim(),
                Email = worksheet.Cells[row, 12].Text?.Trim(),
                Remark = worksheet.Cells[row, 13].Text?.Trim(),
                HanCongNo = worksheet.Cells[row, 14].Text?.Trim(),
                DonViDoiTac = worksheet.Cells[row, 15].Text?.Trim(),
                TenNoiMoToKhai_gs = worksheet.Cells[row, 16].Text?.Trim(),
                MaHangKhaiBaoHSCode_gs = worksheet.Cells[row, 17].Text?.Trim(),
                TenHang_gs = worksheet.Cells[row, 18].Text?.Trim(),
                tenNuocXuatXu_gs = worksheet.Cells[row, 19].Text?.Trim(),
                dieuKienGiaoHang_gs = worksheet.Cells[row, 20].Text?.Trim(),
                PhuongTienVanChuyen_gs = worksheet.Cells[row, 21].Text?.Trim()

            };

            result.Add(preview);
            row++;
        }

        return result;
    }

    public async Task<string?> GetHanCongNoCus(Guid CustomerID)
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rs = await _context.Customer.Where(x => x.Customer_ID == CustomerID && x.Continued == true)
                .Select(x => x.hancongno).FirstOrDefaultAsync();
            return rs?.ToString();
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public string? GetCompanyNameFromID(Guid? Cus_ID)
    {
        try
        {
            if (Cus_ID == null)
                return null;

            _context.ChangeTracker.Clear();
            var rs = _context.Customer.Where(x => x.Customer_ID == Cus_ID && x.Continued == true).FirstOrDefault();
            return rs.COMPANY;
        }
        catch (Exception ex)
        {
            return null;
        }
    }


}
