using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    /// <summary>Giao dịch tiền vào tài khoản ngân hàng nhận qua webhook (SePay).</summary>
    public class M_BankTransaction
    {
        [Key]
        public Guid Id { get; set; }
        public string Provider { get; set; } = "SePay";
        public string ProviderTxnId { get; set; } = "";
        public string? Gateway { get; set; }
        public string? AccountNumber { get; set; }
        public string? SubAccount { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? TransferType { get; set; }
        public decimal Amount { get; set; }
        public string? Content { get; set; }
        public string? ReferenceCode { get; set; }
        public string? RawJson { get; set; }
        public Guid? PhieuthuID { get; set; }
        public string MatchStatus { get; set; } = BankMatchStatus.Unmatched;
        public DateTime ReceivedAt { get; set; } = DateTime.Now;
    }

    public static class BankMatchStatus
    {
        public const string Matched = "Matched";
        public const string WrongAmount = "WrongAmount";
        public const string AlreadyPaid = "AlreadyPaid";
        public const string Unmatched = "Unmatched";
        public const string Manual = "Manual";
    }
}
