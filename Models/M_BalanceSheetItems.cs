using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_BalanceSheetItems
    {
        [Key]
        public Guid Id { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string? ParentItemCode { get; set; }
        public string Section { get; set; }
        public int LevelNo { get; set; }
        public int SortOrder { get; set; }
        public bool IsBold { get; set; }
        public bool IsTotal { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
