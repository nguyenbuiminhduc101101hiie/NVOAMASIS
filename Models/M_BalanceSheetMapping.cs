using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_BalanceSheetMapping
    {
        [Key]
        public Guid Id { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Section { get; set; }
        public string AccountCode { get; set; }
        public int Sign { get; set; }
        public int SortOrder { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
