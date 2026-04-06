using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Stimulsoft.Blockly.Model;
using System.Globalization;
using System.Net.Http;
using System.Xml.Linq;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services
{
    public class SingletonSerivce(IDbContextFactory<AppDbContext> _ctxFactory, CustomAuthenticationStateProvider _cusAuth, HttpClient _httpClient)
    {
        private const string VcbUrl = "https://portal.vietcombank.com.vn/Usercontrols/TVPortal.TyGia/pXML.aspx?b=1";
        public AuthUser _user = new();
        public async Task<AuthenticationState> GetAuth()
        {
            return await _cusAuth.GetAuth();

        }
        public AuthUser GetUserDetail()
        {
            try
            {
                using var _context = _ctxFactory.CreateDbContext();
                _context.ChangeTracker.Clear();
                var user = GetAuth().Result.User.Identity!.Name!;
                var rs = _context.UserList.FirstOrDefault(x => x.Name == user);
                _user = rs ?? new();
                return rs!;
            }
            catch (Exception ex)
            {
                return new AuthUser();
            }
        }
        public async Task<int> GetUnreadEmailCountAsync()
        {
            await using var _context = _ctxFactory.CreateDbContext();
            return await _context.Notifications
                .Where(e => e.ReceiverUserId == _user.UsrId && !e.IsRead)
                .CountAsync();
        }
        public async Task<List<M_Currency>> GetExchangeRatesAsync()
        {
            var currencyList = new List<M_Currency>();

            try
            {
                _httpClient = new();
                var response = await _httpClient.GetAsync(VcbUrl);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var doc = XDocument.Parse(content);
                foreach (var exRate in doc.Descendants("Exrate"))
                {
                    var sellString = exRate.Attribute("Sell")?.Value;

                    // Parse đúng văn hóa số
                    double sellValue = double.Parse(
                        sellString,
                        NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                        new CultureInfo("en-US")
                    );

                    var currency = new M_Currency
                    {
                        Currency = exRate.Attribute("CurrencyCode")?.Value,
                        Exchange = sellValue,
                        ngay = DateTime.Now.ToString("dd-MMM-yyyy").ToUpper(),
                        UpdateTime = DateTime.Now,
                        Continued = true,
                        Editable = true,
                        Approve = false,
                        UserID = _user.Name
                    };

                    currencyList.Add(currency);
                }
                await using var _context = _ctxFactory.CreateDbContext();
                foreach (var currency in currencyList)
                {
                    _context.ChangeTracker.Clear();
                    var rs = await _context.Currency
                        .Where(x => x.ngay == currency.ngay && x.Currency == currency.Currency)
                        .FirstOrDefaultAsync();

                    if (rs != null)
                    {
                        rs.Exchange = currency.Exchange;
                        _context.Currency.Update(rs);
                    }
                    else
                    {
                        await _context.Currency.AddAsync(currency);
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy tỷ giá: " + ex.Message);
            }

            return currencyList;
        }
        public async Task<List<M_HBL>> GetListHBLALL_bysale()
        {
            try
            {
                await using var _context = _ctxFactory.CreateDbContext();
                _context.ChangeTracker.Clear();
                List<M_HBL> rs = new();
                if (_user.Department == "ADMIN")
                {
                    rs = await _context.HBL.ToListAsync();
                }
                else
                {

                    rs = await _context.HBL
                        .Where(x => x.SaleName == _user.Name)
                        .ToListAsync();
                }

                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_HBL>();
            }
        }
        public async Task<List<string>> GetCur()
        {
            await using var _context = _ctxFactory.CreateDbContext();
            var result = await _context.Currency
                .Select(x => x.Currency)
                .Distinct()
                .ToListAsync();

            return result;
        }
    }
}
