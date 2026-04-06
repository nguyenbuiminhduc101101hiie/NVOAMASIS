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

        [Required]
        [MaxLength(10)]
        [RegularExpression("^(DEBIT|CREDIT)$")]
        public string LineType { get; set; } = "DEBIT"; // DEBIT / CREDIT

        [Required]
        public Guid DanhMucTaiKhoanID { get; set; }
        
        public int SortOrder { get; set; } =1;
        public bool IsActive { get; set; } = true;
    }
}

