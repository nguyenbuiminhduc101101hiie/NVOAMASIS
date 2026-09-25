namespace NVOAMASIS.Models
{
    public class SePaySettings
    {
        /// <summary>API key SePay gửi kèm header "Authorization: Apikey {key}". Để trống = tắt webhook.</summary>
        public string? WebhookApiKey { get; set; }

        /// <summary>Số tài khoản nhận tiền của công ty. Nếu có, chỉ nhận giao dịch của tài khoản này.</summary>
        public string? AccountNumber { get; set; }
    }
}
