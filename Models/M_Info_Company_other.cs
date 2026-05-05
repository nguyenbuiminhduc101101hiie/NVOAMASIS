using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Info_Company_other
    {
        [Key]
        public Guid id { get; set; }
        public string? Code { get; set; }
        public string? AccountName { get; set; }
        public string? BankName { get; set; }
        public string? AccountNo { get; set; }
        public string? Deposit { get; set; }
        public string? Transit { get; set; }
        public string? SDT_HotroKH { get; set; }

        public string? Company { get; set; }
        public string? Address { get; set; }
        public string? Tel { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }

        public string? BankAddress { get; set; }
        public string? SWIFT_CODE { get; set; }
        public string? IBAN_CODE { get; set; }
        public string? Branches { get; set; }

    }
}
