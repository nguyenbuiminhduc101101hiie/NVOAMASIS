using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("AccountingVouchers")]
    public class M_AccountingVouchers
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }

        public string VoucherNo { get; set; } = string.Empty;
        public DateTime VoucherDate { get; set; }
        public DateTime PostingDate { get; set; }

        public int FiscalYear { get; set; }
        public int FiscalPeriod { get; set; }

        public string TransactionTypeCode { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string CurrencyCode { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }

        public int Status { get; set; } // 1 Draft, 2 Posted, 3 Cancelled (UI label: 0/1/2)

        public string? ReferenceNo { get; set; }
        public DateTime? ReferenceDate { get; set; }

        public string? BookScope { get; set; }
        public string? SourceModule { get; set; }
        public Guid? SourceId { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string? PostedBy { get; set; }
        public DateTime? PostedDate { get; set; }

        public string? CancelledBy { get; set; }
        public DateTime? CancelledDate { get; set; }
        public bool? Ghiso { get; set; } = false; // 0: Không, 1: Có
        public bool? Approve { get; set; } = false;
    }
}

