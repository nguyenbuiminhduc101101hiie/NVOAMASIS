using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("GeneralLedgerEntries")]
    public class M_GeneralLedgerEntries
    {
        [Key]
        public Guid Id { get; set; }

        public Guid VoucherId { get; set; }
        public Guid? VoucherLineId { get; set; }

        public Guid CompanyId { get; set; }
        public int FiscalYear { get; set; }
        public int FiscalPeriod { get; set; }

        public DateTime PostingDate { get; set; }
        public DateTime VoucherDate { get; set; }
        public string VoucherNo { get; set; } = string.Empty;

        public string AccountCode { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        public string CurrencyCode { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; } = 1m;
        public decimal DebitFC { get; set; }
        public decimal CreditFC { get; set; }

        public Guid? CustomerId { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? ShipmentId { get; set; }
        public Guid? ContractId { get; set; }
        public string? BranchCode { get; set; }
        public string? Description { get; set; }
        public string? SourceModule { get; set; }
        public string? SourceId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public bool? IsTaxBook { get; set; }

        public bool? IsManagementBook { get; set; }

        public string? LedgerType { get; set; }

    }
}
