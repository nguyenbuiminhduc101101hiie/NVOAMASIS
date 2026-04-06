using System.Net;
using System.Net.Mail;

namespace NVOAMASIS.Services
{
    public interface IForgotPasswordEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class ForgotPasswordEmailService : IForgotPasswordEmailService
    {
        private readonly string _smtpHost = "mail.logisticssoftware.vn";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "vms.noreply@logisticssoftware.vn";       // Thay bằng email gửi mail
        private readonly string _smtpPass = "Noreply@1";          // Thay bằng mật khẩu ứng dụng hoặc mật khẩu email
        private readonly string _fromEmail = "vms.noreply@logisticssoftware.vn";      // Email gửi đi

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                using var smtpClient = new System.Net.Mail.SmtpClient(_smtpHost)
                {
                    Port = _smtpPort,
                    Credentials = new System.Net.NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = true,
                };

                var mailMessage = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(_fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
            }
            finally
            {
                // Reset callback về mặc định để không ảnh hưởng tới request khác
                ServicePointManager.ServerCertificateValidationCallback = null;
            }

        }
    }

}
