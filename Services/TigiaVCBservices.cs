using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Xml.Linq;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using static Stimulsoft.Report.StiOptions.Export;

namespace NVOAMASIS.Services
{
    public class tigiaVCBservices
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly AccountService _asv;
        private const string VcbUrl = "https://portal.vietcombank.com.vn/Usercontrols/TVPortal.TyGia/pXML.aspx?b=1";
        public List<string> currs = new List<string>();

        public async Task<IEnumerable<string>> SearchCurr(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return this.currs;

            return this.currs.Where(x => !string.IsNullOrEmpty(x) && x.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        // Constructor có inject cả AppDbContext và AccountService
        public tigiaVCBservices(AppDbContext context, AccountService asv)
        {
            _context = context;
            _asv = asv;
            _httpClient = new HttpClient();
        }
        public async Task<List<M_Currency>> GetExchangeRatesAsync()
        {
            var currencyList = new List<M_Currency>();

            try
            {
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
                        new CultureInfo("en-US"));

        

                    var currency = new M_Currency
                    {
                        Currency = exRate.Attribute("CurrencyCode")?.Value,
                        Exchange = sellValue,
                        ngay = DateTime.Now.ToString("dd-MMM-yyyy").ToUpper(),
                        UpdateTime = DateTime.Now,
                        Continued = true,
                        Editable = true,
                        Approve = false,
                        UserID = _asv.GetAuth().Result.User.Identity!.Name!
                    };

                    currencyList.Add(currency);
                }

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


        public async Task<double?> Getexchange_cur(string cur)
        {
            if (string.IsNullOrEmpty(cur))
            {
                return 1;
            }
            string ngayht = DateTime.Now.ToString("dd-MMM-yyyy").ToUpper();
            string currency = cur.ToUpper();

            try
            {
                if (cur == "VND")
                {
                    return 1;
                }
                else
                {
                    double? rs = await _context.Currency
                                            .Where(x => x.ngay == ngayht && x.Currency == currency)
                                            .Select(x => x.Exchange)
                                            .FirstOrDefaultAsync();

                    return rs;
                }
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        public async Task<List<string?>> GetCur()
        {
            var result = await _context.Currency
                .Select(x => x.Currency)
                .Distinct()
                .ToListAsync();

            return result;
        }
    }
}
