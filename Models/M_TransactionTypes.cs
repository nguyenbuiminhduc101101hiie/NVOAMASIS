using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NVOAMASIS.Models
{
    [Table("TransactionTypes")]
    [Index(nameof(Code), IsUnique = true, Name = "UQ_TransactionTypes_Code")]
    [Index(nameof(Name), IsUnique = true, Name = "UQ_TransactionTypes_Name")]
    public class M_TransactionTypes
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

