using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_BalanceSheetItemAccounts
    {
        [Key]
        public Guid Id { get; set; }
        public string ItemCode { get; set; }
        public string AccountCode { get; set; }
        public string? AccountName { get; set; }
        public int Sign { get; set; }
        public string? BalanceSide { get; set; }
        public bool IsPrefixMatch { get; set; }
        public bool IsActive { get; set; }
        public string? Note { get; set; }

    }
}
