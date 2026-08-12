using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("AccountingVoucherLines")]
    public class M_AccountingVoucherLines
    {
        [Key]
        public Guid Id { get; set; }

        public Guid VoucherId { get; set; }

        public string LineNo_ { get; set; } = string.Empty;

        public Guid DanhMucTaiKhoanID { get; set; }
        public string AccountCode { get; set; } = string.Empty;

        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal DebitAmountFC { get; set; }
        public decimal CreditAmountFC { get; set; }

        public string? LineDescription { get; set; }

        public Guid? CustomerId { get; set; }
        public Guid? ShipmentId { get; set; }
        public string? ContractId { get; set; }
        public string? BranchId { get; set; }
        public Guid? DepartmentId { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsTaxBook { get; set; }
        public bool IsManagementBook { get; set; }
        public string? HBLNo { get; set; }
        public string? BookingNo { get; set; }
        public string? POLName { get; set; }
        public string? PODName { get; set; }
        public string? ContainerNo { get; set; }
        public string? ContainerSizeType { get; set; }
        public string? SaleName { get; set; }
        public string? DELName { get; set; }
        public string? PORName { get; set; }
        public string? TransportMode { get; set; }


        /// <summary>FINANCIAL / TAX / MANAGEMENT (NVARCHAR(20)).</summary>
        [StringLength(20)]
        public string? LedgerType { get; set; }
        public string? TaxCode { get; set; }
        public decimal? VatRate { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public int? SortKey { get; set; }
        public bool thuho { get; set; } = false;

    }
}

