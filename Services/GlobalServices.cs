using Microsoft.EntityFrameworkCore;
using MimeKit;
using RazorLight;
using Stimulsoft.System.Windows.Forms;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using static Stimulsoft.Report.Images.StiReportImages;

namespace NVOAMASIS.Services
{
    public class GlobalServices(AppDbContext _context, IWebHostEnvironment _env ,  AccountService asv, HistoryLogService HistoryLogService)
    {
        private readonly IWebHostEnvironment _env;
        private readonly RazorLightEngine _razorEngine;

        public async Task<string?> GetEmailAsync(string? user)
        {
            try
            {
                _context.ChangeTracker.Clear();

                var rs = await _context.UserList
                                       .Where(x => x.Name == user)
                                       .Select(x => x.Email) // Chỉ lấy cột Email
                                       .FirstOrDefaultAsync(); // Lấy giá trị đầu tiên hoặc null

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                return null;
            }
        }
        public async Task<Guid?> GetIdfromUser(string? user)
        {
            try
            {
                _context.ChangeTracker.Clear();

                var rs = await _context.UserList
                                       .Where(x => x.Name == user)
                                       .Select(x => x.UsrId) // Chỉ lấy cột Email
                                       .FirstOrDefaultAsync(); // Lấy giá trị đầu tiên hoặc null

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Terminal_>> GetList_Terminal()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Terminal
                    .Where(x => !string.IsNullOrEmpty(x.TermiNalName))
                    .OrderBy(x => x.TermiNalName).Distinct()

                    .ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                return new List<Terminal_>();
            }

        }
        public async Task<List<M_CompanyInfo>> Get_Company_info()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.CompanyInfomation

                    .ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_CompanyInfo>();
            }

        }

        public async Task<BoolandMessReponse> UpdateCompany_info(M_CompanyInfo c)
        {
            try
            {
                _context.ChangeTracker.Clear();
         
                _context.CompanyInfomation.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Company Information Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Company Information with error code: " + ex.Message);
            }
        }

        // TiepDauNgu CRUD
        public async Task<List<M_TiepDauNgu>> GetTiepDauNguList()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.TiepDauNgu.OrderBy(x => x.Loai).ThenBy(x => x.Hangso).ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_TiepDauNgu>();
            }
        }

        public async Task<BoolandMessReponse> AddTiepDauNgu(M_TiepDauNgu item)
        {
            try
            {
                _context.ChangeTracker.Clear();
                item.id = Guid.NewGuid();
                _context.TiepDauNgu.Add(item);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Thêm thành công");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Lỗi: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> UpdateTiepDauNgu(M_TiepDauNgu item)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.TiepDauNgu.Update(item);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Cập nhật thành công");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Lỗi: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteTiepDauNgu(M_TiepDauNgu item)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.TiepDauNgu.Remove(item);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Xóa thành công");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Lỗi: " + ex.Message);
            }
        }

        // Company Other CRUD
        public async Task<List<M_Info_Company_other>> GetCompanyOtherList()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Information_Comapny_Other
                    .OrderBy(x => x.Code)
                    .ThenBy(x => x.AccountName)
                    .ToListAsync();
                return rs;
            }
            catch
            {
                return new List<M_Info_Company_other>();
            }
        }

        public async Task<BoolandMessReponse> AddCompanyOther(M_Info_Company_other item)
        {
            try
            {
                _context.ChangeTracker.Clear();
                item.id = Guid.NewGuid();
                _context.Information_Comapny_Other.Add(item);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Thêm thành công");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Lỗi: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> UpdateCompanyOther(M_Info_Company_other item)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Information_Comapny_Other.Update(item);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Cập nhật thành công");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Lỗi: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteCompanyOther(M_Info_Company_other item)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Information_Comapny_Other.Remove(item);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Xóa thành công");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Lỗi: " + ex.Message);
            }
        }

        public async Task<List<Terminal_>> GetList_Terminal_add()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Terminal
                    .Where(x=>!string.IsNullOrEmpty(x.TermiNalName))
                    .OrderBy(x => x.TermiNalName).Distinct()

                    .ToListAsync();

                return rs;
            }
            catch (Exception ex)
            {
                return new List<Terminal_>();
            }

        }

        public async Task<BoolandMessReponse> SaveOrUpdateTerminal(Terminal_ item)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var usr = asv.GetAuth().Result.User.Identity?.Name;
                item.UserID = usr;
                item.UpdateTime = DateTime.Now;
                if (item.TerminalID == Guid.Empty)
                {
                    item.TerminalID = Guid.NewGuid();
                    _context.Terminal.Add(item);
                }
                else
                {
                    _context.Terminal.Update(item);
                }
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Save Terminal Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Save Terminal: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeleteTerminal(Terminal_ item)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (item?.TerminalID == null || item.TerminalID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");
                _context.Terminal.Remove(item);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Terminal: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> GuiThongBaoProject(M_Duan item, string[] recipients)
        {
            try
            {
                string subject = $@"Thông Báo Export Cost Pricing &lt;{item.TenDuan}&gt; - Pricing No. &lt;{item.Pricingno}&gt; - Type. &lt;{item.loai}&gt;";
                string decodedSubject = System.Net.WebUtility.HtmlDecode(subject);
                var body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 100%;
                                        padding: 10px;
                                    }}
                                    .section {{
                                        margin-bottom: 20px;
                                        padding: 15px;
                                        border-radius: 5px;
                                    }}
                                    .header {{
                                        background-color: #007bff;
                                        color: white;
                                        font-size: 18px;
                                        font-weight: bold;
                                    }}
                                    .content {{
                                        background-color: #f8f9fa;
                                    }}
                                    .task {{
                                        background-color: #e9ecef;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='section header'>📢 {subject}</div>
                                    <div class='section content'>
                                        <strong>Nội dung:</strong><br>
                                        {item.MaDuan}
                                    </div>
                                    <div class='section task'>
                                        <strong>Chi tiết nội dung:</strong><br>
                                        {item.NoiDung}
                                    </div>
                                </div>
                            </body>
                            </html>";


                var sendrs = await SendMail(recipients.ToList(), new List<string>(), decodedSubject, body);
                return sendrs;
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Send mail failed! with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> GuiThongBao_DuyetDNTT(M_Duyet_DNTT item, string[] recipients,string DNTTNO, string noidungapprove)
        {
            try
            {
                string subject = $@"Thông Báo Duyệt Đê Nghị Thanh Toán : &lt;{DNTTNO}&gt; - Duyệt Lần :{item.SoLanGuiEmail}";
                string decodedSubject = System.Net.WebUtility.HtmlDecode(subject);
                var body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 100%;
                                        padding: 10px;
                                    }}
                                    .section {{
                                        margin-bottom: 20px;
                                        padding: 15px;
                                        border-radius: 5px;
                                    }}
                                    .header {{
                                        background-color: #007bff;
                                        color: white;
                                        font-size: 18px;
                                        font-weight: bold;
                                    }}
                                    .content {{
                                        background-color: #f8f9fa;
                                    }}
                                    .task {{
                                        background-color: #e9ecef;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='section header'>📢 {subject}</div>
                                    <div class='section content'>
                                        <strong>Nội dung:{noidungapprove}</strong><br>
                                            Kính gửi: {item.Kinhgui}<br>
                                            Bộ phận: {item.Bophan}<br>
                                            Người đề nghị: {item.Userduyet}<br>
                                    </div>
                                    
                                </div>
                            </body>
                            </html>";


                var sendrs = await SendMail(recipients.ToList(), new List<string>(), decodedSubject, body);
                return sendrs;
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Send mail failed! with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> GuiThongBao_YCDuyetDNTT(M_DNTT_Logistics item, string[] recipients)
        {
     
            try
            {
                string subject = $@"Thông Báo Yêu Cầu Duyệt Đê Nghị Thanh Toán : &lt;{item.So}&gt; - Yêu Cầu Duyệt Lần :{item.SoLanGuiEmail}";
                string decodedSubject = System.Net.WebUtility.HtmlDecode(subject);
                var body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 100%;
                                        padding: 10px;
                                    }}
                                    .section {{
                                        margin-bottom: 20px;
                                        padding: 15px;
                                        border-radius: 5px;
                                    }}
                                    .header {{
                                        background-color: #007bff;
                                        color: white;
                                        font-size: 18px;
                                        font-weight: bold;
                                    }}
                                    .content {{
                                        background-color: #f8f9fa;
                                    }}
                                    .task {{
                                        background-color: #e9ecef;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='section header'>📢 {subject}</div>
                                    <div class='section content'>
                                        <strong>Nội dung:</strong><br>
                                            Kính gửi: {item.Kinhgui}<br>
                                            Người đề nghị: {item.Tennguoidenghi}<br>
                                            Bộ phận: {item.Bophan}<br>
                                            Nội dung đề nghị: {item.Noidung}<br>
                                           Thành tiền: {(item.Thanhtien.HasValue ? item.Thanhtien.Value.ToString("#,##0.##") : "")} {item.Loaitiente}<br>
                                          
                                    </div>
                                    
                                </div>
                            </body>
                            </html>";


                var sendrs = await SendMail(recipients.ToList(), new List<string>(), decodedSubject, body);
                return sendrs;
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Send mail failed! with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> GuiThongBaoYeuCauTuVanHQ(M_YeuCauTuVanTTHQ item, string[] recipients)
        {
            try
            {
                string subject = $@"Thông Báo Request Customs Advice CO &lt;{item.TieuDeYeuCau}&gt; - Number of Requests . &lt;{item.SoYeuCau}&gt;";
                string decodedSubject = System.Net.WebUtility.HtmlDecode(subject);
                var body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 100%;
                                        padding: 10px;
                                    }}
                                    .section {{
                                        margin-bottom: 20px;
                                        padding: 15px;
                                        border-radius: 5px;
                                    }}
                                    .header {{
                                        background-color: #007bff;
                                        color: white;
                                        font-size: 18px;
                                        font-weight: bold;
                                    }}
                                    .content {{
                                        background-color: #f8f9fa;
                                    }}
                                    .task {{
                                        background-color: #e9ecef;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='section header'>📢 {subject}</div>
                                    <div class='section content'>
                                        <strong>Nội dung:</strong><br>
                                        {item.NoiDung}
                                    </div>
                                  
                                </div>
                            </body>
                            </html>";


                var sendrs = await SendMail(recipients.ToList(), new List<string>(), decodedSubject, body);
                return sendrs;
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Send mail failed! with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> GuiThongBao_YeuCauTrucking(M_YeuCauTrucking item, string[] recipients)
        {
            try
            {
                string subject = $@"Thông Báo Yêu Cầu Trucking &lt;{item.TieuDeEmail}&gt; - Yêu Cầu Trucking No. &lt;{item.YeuCauTruckingNo}&gt;";
                string decodedSubject = System.Net.WebUtility.HtmlDecode(subject);
                var body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 100%;
                                        padding: 10px;
                                    }}
                                    .section {{
                                        margin-bottom: 20px;
                                        padding: 15px;
                                        border-radius: 5px;
                                    }}
                                    .header {{
                                        background-color: #007bff;
                                        color: white;
                                        font-size: 18px;
                                        font-weight: bold;
                                    }}
                                    .content {{
                                        background-color: #f8f9fa;
                                    }}
                                    .task {{
                                        background-color: #e9ecef;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='section header'>📢 {subject}</div>
                                    <div class='section content'>
                                        <strong>Nội dung:</strong><br>
                                        {item.NoidungEmail}
                                    </div>
                                    <div class='section task'>
                                        <strong>Chi tiết nội dung:</strong><br>
                                        {item.NoidungChitietEmail}
                                    </div>
                                </div>
                            </body>
                            </html>";


                var sendrs = await SendMail(recipients.ToList(), new List<string>(), decodedSubject, body);
                return sendrs;
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Send mail failed! with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> GuiThongBao_TKHQ(M_TKHQ_Thongtinchunglohang item, string[] recipients)
        {
            try
            {
                string subject = $@"Thông Báo Yêu Cầu TKHQ &lt;{item.TieuDeEmail}&gt; - Số Tờ Khai &lt;{item.Sotokhai}&gt;";
                string decodedSubject = System.Net.WebUtility.HtmlDecode(subject);
                var body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 100%;
                                        padding: 10px;
                                    }}
                                    .section {{
                                        margin-bottom: 20px;
                                        padding: 15px;
                                        border-radius: 5px;
                                    }}
                                    .header {{
                                        background-color: #007bff;
                                        color: white;
                                        font-size: 18px;
                                        font-weight: bold;
                                    }}
                                    .content {{
                                        background-color: #f8f9fa;
                                    }}
                                    .task {{
                                        background-color: #e9ecef;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='section header'>📢 {subject}</div>
                                    <div class='section content'>
                                        <strong>Nội dung:</strong><br>
                                        {item.NoidungEmail}
                                    </div>
                                    <div class='section task'>
                                        <strong>Chi tiết nội dung:</strong><br>
                                        {item.NoidungChitietEmail}
                                    </div>
                                </div>
                            </body>
                            </html>";


                var sendrs = await SendMail(recipients.ToList(), new List<string>(), decodedSubject, body);
                return sendrs;
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Send mail failed! with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> GuiThongBaoProject_product_price(M_Product_Price item, string[] recipients)
        {
            try
            {
                string subject = $@"Thông Báo Reply Product Price Enter &lt;{item.tieude}&gt; - Pricing No. &lt;{item.Productpriceno}&gt;";
                string decodedSubject = System.Net.WebUtility.HtmlDecode(subject);
                var body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 100%;
                                        padding: 10px;
                                    }}
                                    .section {{
                                        margin-bottom: 20px;
                                        padding: 15px;
                                        border-radius: 5px;
                                    }}
                                    .header {{
                                        background-color: #007bff;
                                        color: white;
                                        font-size: 18px;
                                        font-weight: bold;
                                    }}
                                    .content {{
                                        background-color: #f8f9fa;
                                    }}
                                    .task {{
                                        background-color: #e9ecef;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='section header'>📢 {subject}</div>
                                    <div class='section content'>
                                        <strong>Nội dung:</strong><br>
                                        {item.noidung}
                                    </div>
                                    <div class='section task'>
                                        <strong>Chi tiết nội dung:</strong><br>
                                        {item.noidungcongviec}
                                    </div>
                                </div>
                            </body>
                            </html>";


                var sendrs = await SendMail(recipients.ToList(), new List<string>(), decodedSubject, body);
                return sendrs;
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Send mail failed! with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> SendMail(List<string> to, List<string> toCc, string subject, string body)
        {
            try
            {
                // Thiết lập thông tin SMTP
                using (var smtpClient = new SmtpClient("mail.logisticssoftware.vn", 587))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential("vms.noreply@logisticssoftware.vn", "Noreply@1");

                    // Tạo đối tượng MailMessage
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress("vms.noreply@logisticssoftware.vn");
                    //add mail
                    foreach (var mail in to)
                    {
                        mailMessage.To.Add(mail);
                    }
                    foreach (var mail in toCc)
                    {
                        mailMessage.CC.Add(mail);
                    }
                    mailMessage.Subject = subject;
                    mailMessage.IsBodyHtml = true;

                    mailMessage.Body = body;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    // Gửi email
                    await smtpClient.SendMailAsync(mailMessage);
                }
                return new BoolandMessReponse(true, "Send mail successfully!"); ;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new BoolandMessReponse(false, "Send mail failed! with error code:" + ex.Message);
            }
        }
        public async Task<List<M_Duan>> GetListDuAn_request()
        {
            try
            {
                AuthUser user = new AuthUser();
                user = asv.GetUserDetail();

                _context.ChangeTracker.Clear();
                List<M_Duan> rs = new();

                if (user.Department == "ADMIN")
                {
                    rs = _context.Duan.Where(x=>x.Continued == true)
                        .OrderBy(x => x.Pricingno).Distinct().ToList();
                }
                else
                {
                    rs = _context.Duan.OrderBy(x => x.Pricingno)
                        .Where(x => x.Continued == true && x.EmailNguoiPhuTrach.Contains(user.Name))
                        .Distinct().ToList();
                }


                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_Duan>();
            }
        }

        public List<M_Duan> GetListDuanAll()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.Duan.OrderBy(x => x.Pricingno).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<M_Duan>();
            }
        }
        public List<AuthUser> GetListuser()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.UserList.OrderBy(x => x.Name).ToList();
                return rs;
            }
            catch (Exception ex)
            {
                return new List<AuthUser>();
            }
        }


        public async Task<List<M_Duan>> GetListDuAn_Create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.Duan.Where(_ => _.Id == id).ToListAsync();
            return Invoices;
        }

        public async Task<List<M_Duan>> GetListDuAn(string loai)
        {
            try
            {
                AuthUser user = new AuthUser();
                user = asv.GetUserDetail();

                _context.ChangeTracker.Clear();
                List<M_Duan> rs = new();
                if(user.Department == "ADMIN")
                {
                    rs = _context.Duan.OrderBy(x => x.Pricingno)
                         .Distinct().ToList();
                }
                else
                {
                    if (loai == "Import")
                    {

                        rs = _context.Duan.OrderBy(x => x.Pricingno)
                            .Where(x => (x.loai == "SI" || x.loai == "AI") && x.EmailNguoiPhuTrach.Contains(user.Name))
                            .Distinct().ToList();


                    }
                    else if (loai == "Export")
                    {

                        rs = _context.Duan.OrderBy(x => x.Pricingno)
                               .Where(x => (x.loai == "SE" || x.loai == "AE") && x.EmailNguoiPhuTrach.Contains(user.Name))
                               .Distinct().ToList();

                    }
                    else if (loai == "Truck")
                    {

                        rs = _context.Duan.OrderBy(x => x.Pricingno)
                           .Where(x => (x.loai == "Truck") && x.EmailNguoiPhuTrach.Contains(user.Name))
                           .Distinct().ToList();


                    }
                    else if (loai == "KTCL")
                    {

                        rs = _context.Duan.OrderBy(x => x.Pricingno)
                             .Where(x => (x.loai == "KTCL") && x.EmailNguoiPhuTrach.Contains(user.Name))
                             .Distinct().ToList();

                    }
                    else if (loai == "Customs")
                    {

                        rs = _context.Duan.OrderBy(x => x.Pricingno)
                            .Where(x => (x.loai == "Customs") && x.EmailNguoiPhuTrach.Contains(user.Name))
                            .Distinct().ToList();


                    }
                }

                
                return rs;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_Duan>();
            }
        }

        public async Task<List<string>> UpdateCreateDuan(M_Duan IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Request Pricing Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Request Pricing Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Request Pricing Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> CreateDuAnDetail(M_Duan c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();
                _context.Duan.Add(c!);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Create Project Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeleteDuAnDetail(M_Duan c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Duan.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateDuAnDetail(M_Duan c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Duan.Update(c);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Project Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Project with error code: " + ex.Message);
            }
        }
        public async Task<List<string>> UpdateCreatePricing(M_Duan IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Request Pricing Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Request Pricing Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Request Pricing Fail", "0"];
            }
        }
        public async Task<BoolandMessReponse> UpdateYeuCauTuVanHQCODetail(M_YeuCauTuVanTTHQ c)
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

        public async Task<BoolandMessReponse> SendMessageAsync(string userId, string tieude , string noidung ,string chitietnoidung)
        {
            string ZaloApiUrl = "https://openapi.zalo.me/v3.0/oa/message/cs";
            var token = await GetTokenAsync();
            var _accessToken = token.Accesstoken;
            var requestBody = new
            {
                recipient = new { user_id = userId },
                message = new { text = $"{tieude}\n{noidung}\n{chitietnoidung}" }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
            using (var _httpClient = new HttpClient())
            {
              
                _httpClient.DefaultRequestHeaders.Add("access_token", _accessToken);

                HttpResponseMessage response = await _httpClient.PostAsync(ZaloApiUrl, jsonContent);

                string responseContent = await response.Content.ReadAsStringAsync();

                try
                {
                    using JsonDocument doc = JsonDocument.Parse(responseContent);
                    JsonElement root = doc.RootElement;

                    // Kiểm tra nếu có "error"
                    if (root.TryGetProperty("error", out JsonElement errorElement) && errorElement.GetInt32() == 0)
                    {
                        return new BoolandMessReponse(true, $"✅ Gửi tin nhắn thành công!");
                    }
                    else
                    {
                        string errorMessage = root.GetProperty("message").GetString();
                        return new BoolandMessReponse(false, $"❌ Gửi tin nhắn thất bại: {errorMessage}");
                    }
                }
                catch (JsonException)
                {
                    return new BoolandMessReponse(false, "❌ Lỗi không xác định từ API Zalo! JSON không hợp lệ.");
                }
            }
        }

        public async Task<ZaloAccessToken> GetTokenAsync()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.ZaloAccesstoken.FirstOrDefaultAsync();
                return rs == null ? new ZaloAccessToken() : rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new ZaloAccessToken();
            }
        }
        public async Task<List<PermissionTemplate>> GetListPermissionTemplate()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.PermissionTemplate.ToListAsync();
                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<PermissionTemplate>();
            }
        }
        public async Task<BoolandMessReponse> CreatePerTemplate(PermissionTemplate c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();
                _context.PermissionTemplate.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Permission Template", "Permission", c.Id, c.Dept, c);
                return new BoolandMessReponse(true, "Create Template Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Template with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeletePerTemplate(PermissionTemplate c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.PermissionTemplate.Remove(c!);
                await _context?.SaveChangesAsync()!;
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete Permission Template", "Permission", c.Id, c.Dept, new { OldData = c});
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Template with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdatePerTemplate(PermissionTemplate c, PermissionTemplate c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.PermissionTemplate.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Permission Template", "Permission", c.Id, c.Dept, new { OldData = c_old, NewData = c });
                return new BoolandMessReponse(true, "Update Template Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Template with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> GuiThongBaoProject_product_price_import(M_ImportCostRequest item, string[] recipients)
        {
            try
            {
                string subject = $@"Thông Báo Reply Product Price Import &lt;{item.MaRFQ}&gt;";
                string decodedSubject = System.Net.WebUtility.HtmlDecode(subject);
                var body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 100%;
                                        padding: 10px;
                                    }}
                                    .section {{
                                        margin-bottom: 20px;
                                        padding: 15px;
                                        border-radius: 5px;
                                    }}
                                    .header {{
                                        background-color: #007bff;
                                        color: white;
                                        font-size: 18px;
                                        font-weight: bold;
                                    }}
                                    .content {{
                                        background-color: #f8f9fa;
                                    }}
                                    .task {{
                                        background-color: #e9ecef;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='section header'>📢 {subject}</div>
                                    <div class='section content'>
                                        <strong>Nội dung:</strong><br>
                                    
                                        {item.Noidung}
                                      
                                    </div>
                                </div>
                            </body>
                            </html>";


                var sendrs = await SendMail(recipients.ToList(), new List<string>(), decodedSubject, body);
                return sendrs;
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Send mail failed! with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> GuiThongBao_IssueReport(M_IssueReports item, string[] recipients)
        {
            try
            {
                string subject = $@"Thông Báo Issue Report &lt;{item.No}&gt - &lt;{item.Title}&gt;";
                string decodedSubject = System.Net.WebUtility.HtmlDecode(subject);
                var body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 100%;
                                        padding: 10px;
                                    }}
                                    .section {{
                                        margin-bottom: 20px;
                                        padding: 15px;
                                        border-radius: 5px;
                                    }}
                                    .header {{
                                        background-color: #007bff;
                                        color: white;
                                        font-size: 18px;
                                        font-weight: bold;
                                    }}
                                    .content {{
                                        background-color: #f8f9fa;
                                    }}
                                    .task {{
                                        background-color: #e9ecef;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='section header'>📢 {subject}</div>
                                    <div class='section content'>
                                        <p><strong>Mô tả:</strong> {item.Description}</p>
                                        <p><strong>Các bước để tái hiện lỗi:</strong> {item.StepsToReproduce}</p>
                                        <p><strong>Kết quả mong đợi:</strong> {item.ExpectedResult}</p>
                                        <p><strong>Kết quả thực tế:</strong> {item.ActualResult}</p>
                                        <p><strong>Mức độ nghiêm trọng:</strong> {item.Severity}</p>
                                        <p><strong>Trạng thái:</strong> {item.Status}</p>
                                        <p><strong>Người báo cáo:</strong> {item.Reporter}</p>
                                        <p><strong>Giao cho:</strong> {item.AssignedTo}</p>
                                        <p><strong>Ngày báo cáo:</strong> {item.ReportDate}</p>
                                        <p><strong>Ngày xử lý:</strong> {item.ResolveDate}</p>
                                    </div>
                                </div>
                            </body>

                            </html>";


                var sendrs = await SendMail(recipients.ToList(), new List<string>(), decodedSubject, body);
                return sendrs;
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Send mail failed! with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> GuiThongBaoProject_product_price_export(M_ExportCostRequest item, string[] recipients)
        {
            try
            {
                string subject = $@"Thông Báo Reply Product Price Export &lt;{item.maRFQ}&gt;";
                string decodedSubject = System.Net.WebUtility.HtmlDecode(subject);
                var body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 100%;
                                        padding: 10px;
                                    }}
                                    .section {{
                                        margin-bottom: 20px;
                                        padding: 15px;
                                        border-radius: 5px;
                                    }}
                                    .header {{
                                        background-color: #007bff;
                                        color: white;
                                        font-size: 18px;
                                        font-weight: bold;
                                    }}
                                    .content {{
                                        background-color: #f8f9fa;
                                    }}
                                    .task {{
                                        background-color: #e9ecef;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='section header'>📢 {subject}</div>
                                    <div class='section content'>
                                        <strong>Nội dung:</strong><br>
                                        {item.noidung}
                                    </div>
                                </div>
                            </body>
                            </html>";


                var sendrs = await SendMail(recipients.ToList(), new List<string>(), decodedSubject, body);
                return sendrs;
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Send mail failed! with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> GuiThongBaoProject_product_price_Truck(M_TruckingCostRequest item, string[] recipients)
        {
            try
            {
                string subject = $@"Thông Báo Reply Product Price Trucking &lt;{item.MaRFQ}&gt;";
                string decodedSubject = System.Net.WebUtility.HtmlDecode(subject);
                var body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 100%;
                                        padding: 10px;
                                    }}
                                    .section {{
                                        margin-bottom: 20px;
                                        padding: 15px;
                                        border-radius: 5px;
                                    }}
                                    .header {{
                                        background-color: #007bff;
                                        color: white;
                                        font-size: 18px;
                                        font-weight: bold;
                                    }}
                                    .content {{
                                        background-color: #f8f9fa;
                                    }}
                                    .task {{
                                        background-color: #e9ecef;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='section header'>📢 {subject}</div>
                                    <div class='section content'>
                                        <strong>Nội dung:</strong><br>
                                        {item.Ghichu}
                                    </div>
                                </div>
                            </body>
                            </html>";


                var sendrs = await SendMail(recipients.ToList(), new List<string>(), decodedSubject, body);
                return sendrs;
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Send mail failed! with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> GuiThongBaoProject_product_price_KTCL(M_KTCLCostRequest item, string[] recipients)
        {
            try
            {
                string subject = $@"Thông Báo Reply Cost Request Quality Control &lt;{item.MaRFQ}&gt;";
                string decodedSubject = System.Net.WebUtility.HtmlDecode(subject);
                var body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 100%;
                                        padding: 10px;
                                    }}
                                    .section {{
                                        margin-bottom: 20px;
                                        padding: 15px;
                                        border-radius: 5px;
                                    }}
                                    .header {{
                                        background-color: #007bff;
                                        color: white;
                                        font-size: 18px;
                                        font-weight: bold;
                                    }}
                                    .content {{
                                        background-color: #f8f9fa;
                                    }}
                                    .task {{
                                        background-color: #e9ecef;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='section header'>📢 {subject}</div>
                                    <div class='section content'>
                                        <strong>Nội dung:</strong><br>
                                        {item.Noidung}
                                    </div>
                                </div>
                            </body>
                            </html>";


                var sendrs = await SendMail(recipients.ToList(), new List<string>(), decodedSubject, body);
                return sendrs;
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Send mail failed! with error code: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> GuiThongBaoProject_product_price_Customs(M_CustomCostRequest item, string[] recipients)
        {
            try
            {
                string subject = $@"Thông Báo Reply Customs Procedure Cost Price &lt;{item.MaRFQ}&gt;";
                string decodedSubject = System.Net.WebUtility.HtmlDecode(subject);
                var body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                    }}
                                    .container {{
                                        width: 100%;
                                        padding: 10px;
                                    }}
                                    .section {{
                                        margin-bottom: 20px;
                                        padding: 15px;
                                        border-radius: 5px;
                                    }}
                                    .header {{
                                        background-color: #007bff;
                                        color: white;
                                        font-size: 18px;
                                        font-weight: bold;
                                    }}
                                    .content {{
                                        background-color: #f8f9fa;
                                    }}
                                    .task {{
                                        background-color: #e9ecef;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='container'>
                                    <div class='section header'>📢 {subject}</div>
                                    <div class='section content'>
                                        <strong>Nội dung:</strong><br>
                                        {item.Noidung}
                                    </div>
                                </div>
                            </body>
                            </html>";


                var sendrs = await SendMail(recipients.ToList(), new List<string>(), decodedSubject, body);
                return sendrs;
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Send mail failed! with error code: " + ex.Message);
            }
        }

  

    }
}
