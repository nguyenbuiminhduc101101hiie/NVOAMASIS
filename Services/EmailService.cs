using MimeKit;

using MailKit.Net.Smtp;

using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

using MailKit.Security;

using Microsoft.EntityFrameworkCore;

using NVOAMASIS.Data;

using Microsoft.JSInterop;

using NVOAMASIS.Models;



namespace NVOAMASIS.Services

{

    public class EmailService(AppDbContext _context, AccountService asv, IJSRuntime JS)

    {

        private const string SmtpServer = "mail.logisticssoftware.vn";

        private const int SmtpPort = 587; // Cổng đã chọn là 25, nếu bị lỗi có thể thử lại với 465 hoặc 587

        private const string SenderEmail = "vms.noreply@logisticssoftware.vn";

        private const string SenderPassword = "Noreply@1";



        public async Task<bool> SendEmailAsync_1(string body, string[] recipients, List<(byte[] Content, string FileName)> attachments)

        {

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("vms.noreply", SenderEmail));



            foreach (var email in recipients)

            {

                message.To.Add(new MailboxAddress("", email));

            }



            message.Subject = "Thông báo";



            var bodyBuilder = new BodyBuilder { TextBody = body };



            foreach (var attachment in attachments)

            {

                bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content);

            }



            message.Body = bodyBuilder.ToMessageBody();



            var client = new SmtpClient();

            try

            {

                try

                {

                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;



                    await client.ConnectAsync(SmtpServer, SmtpPort, MailKit.Security.SecureSocketOptions.Auto);



                    //await client.ConnectAsync(SmtpServer, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);

                }

                catch (Exception ex)

                {

                    Console.WriteLine($"Lỗi SMTP: {ex.Message}");

                    await JS.InvokeVoidAsync("alert", $"Lỗi SMTP: {ex.Message}"); // Hiển thị lỗi mà không làm sập dialog

                    //StateHasChanged(); // Cập nhật UI

                }

                await client.AuthenticateAsync(SenderEmail, SenderPassword);

                await client.SendAsync(message);

                await client.DisconnectAsync(true);

                return true; // Gửi email thành công

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Lỗi khi gửi email: {ex.Message}");

                return false; // Gửi email thất bại

            }

        }



        public async Task<bool> SendEmailAttachment_Quotation(string toEmail, string subject, string body, byte[] attachment, string fileName)

        {

            try

            {

                var message = new MimeMessage();

                message.From.Add(new MailboxAddress("vms.noreply", SenderEmail));

                message.To.Add(new MailboxAddress("", toEmail));

                message.Subject = subject;



                var bodyBuilder = new BodyBuilder { HtmlBody = body };



                // Đính kèm file PDF

                if (attachment != null)

                {

                    bodyBuilder.Attachments.Add(fileName, attachment, ContentType.Parse("application/pdf"));

                }



                message.Body = bodyBuilder.ToMessageBody();



                using (var client = new SmtpClient())

                {

                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync(SmtpServer, SmtpPort, MailKit.Security.SecureSocketOptions.Auto);

                    await client.AuthenticateAsync(SenderEmail, SenderPassword);

                    await client.SendAsync(message);

                    await client.DisconnectAsync(true);

                }



                return true;

            }

            catch (Exception ex)

            {

                return false;

            }

        }



        /// <summary>

        /// Gửi email text thuần (không bật alert JS) — dùng cho thông báo nền sau khi lưu SI.

        /// </summary>

        public async Task<bool> SendPlainTextEmailAsync(IReadOnlyList<string> recipients, string subject, string textBody)

        {

            if (recipients == null || recipients.Count == 0)

                return true;



            var valid = recipients.Where(static e => !string.IsNullOrWhiteSpace(e) && e.Contains('@', StringComparison.Ordinal)).ToList();

            if (valid.Count == 0)

                return true;



            try

            {

                var message = new MimeMessage();

                message.From.Add(new MailboxAddress("vms.noreply", SenderEmail));

                foreach (var email in valid)

                    message.To.Add(new MailboxAddress("", email.Trim()));

                message.Subject = subject;

                message.Body = new TextPart("plain") { Text = textBody ?? string.Empty };



                using var client = new SmtpClient();

                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(SmtpServer, SmtpPort, SecureSocketOptions.Auto);

                await client.AuthenticateAsync(SenderEmail, SenderPassword);

                await client.SendAsync(message);

                await client.DisconnectAsync(true);

                return true;

            }

            catch (Exception ex)

            {

                Console.WriteLine($"SendPlainTextEmailAsync: {ex.Message}");

                return false;

            }

        }



        /// <summary>

        /// Thông báo đã lưu Shipment Instruction (SI) tới email cấu hình trong Company Information.

        /// </summary>

        /// <returns>true nếu không cần gửi hoặc gửi thành công; false nếu đã cố gửi nhưng SMTP lỗi.</returns>

        public async Task<bool> NotifyCompanyShipmentInstructionSavedAsync(M_SI si, bool isNew, string? savedByUser)

        {

            var company = await _context.CompanyInfomation.AsNoTracking().FirstOrDefaultAsync();

            var raw = company?.Email?.Trim();

            if (string.IsNullOrEmpty(raw))

                return true;



            var recipients = raw

                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)

                .Where(static e => e.Contains('@', StringComparison.Ordinal))

                .ToList();



            if (recipients.Count == 0)

                return true;



            var subject = isNew

                ? "[VMS] Shipment Instruction mới đã được tạo"

                : "[VMS] Shipment Instruction đã được cập nhật";



            var bkNoDisplay = string.IsNullOrWhiteSpace(si.bkno) ? "(chưa nhập)" : si.bkno.Trim();

            var sb = new StringBuilder();

            sb.AppendLine(isNew
                ? $"Đã tạo mới bản ghi Shipment Instruction (SI) trên hệ thống. Số BK: {bkNoDisplay}."
                : $"Đã cập nhật bản ghi Shipment Instruction (SI) trên hệ thống. Số BK: {bkNoDisplay}.");

            sb.AppendLine();

            sb.AppendLine($"Thời gian: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            if (!string.IsNullOrWhiteSpace(savedByUser))

                sb.AppendLine($"Người thực hiện: {savedByUser}");

            sb.AppendLine($"BK No.: {bkNoDisplay}");

            sb.AppendLine($"MBL: {si.mbl}");

            sb.AppendLine($"HBL: {si.hbl}");

            sb.AppendLine($"Vessel / Voy: {si.vessel} / {si.voy}");

            sb.AppendLine($"POL: {si.polname} ({si.polcode})");

            sb.AppendLine($"POD: {si.podname} ({si.podcode})");

            sb.AppendLine();

            sb.AppendLine("— Hệ thống VMS");



            return await SendPlainTextEmailAsync(recipients, subject, sb.ToString());

        }

    }

}

