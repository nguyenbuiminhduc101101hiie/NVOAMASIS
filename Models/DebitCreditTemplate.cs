using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class DebitCreditTemplate
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public Guid CustomerId { get; set; }
        public string? DebitCredit { get; set; }
        public string? UserCreate { get; set; }
        public string? ShipmentType { get; set; }
        /// <summary>Loại phí (cùng nguồn danh sách với cột Type trên Debit/Credit).</summary>
        public string? type { get; set; }
        public bool? Continued { get; set; }
        public double Amount { get; set; } = 0;
        public string Cur { get; set; } = "VND";
    }
}
