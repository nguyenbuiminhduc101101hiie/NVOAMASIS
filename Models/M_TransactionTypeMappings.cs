using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("TransactionTypeMappings")]
    public class M_TransactionTypeMappings
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid TransactionID { get; set; }

        /// <summary>DEBIT / CREDIT. Nullable to tolerate legacy rows with NULL in SQL.</summary>
        [MaxLength(10)]
        public string? LineType { get; set; } = "DEBIT";

        /// <summary>Nullable to tolerate legacy rows with NULL in SQL.</summary>
        public Guid? DanhMucTaiKhoanID { get; set; }

        public int? SortOrder { get; set; } = 1;

        public bool? IsActive { get; set; } = true;
    }
}

