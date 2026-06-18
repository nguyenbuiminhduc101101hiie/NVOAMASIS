using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_CompanyInfo
    {
        [Key]
        public Guid CompanyID { get; set; }
        public string? NameTiengViet { get; set; }
        public string? AddressTiengViet { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Tel { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? SwiftCode { get; set; }
        public string? AddDebit { get; set; }
        public string? Accno1Debit { get; set; }
        public string? Accno2Debit { get; set; }
        public string? TaxCode { get; set; }
        public string? IPAddress { get; set; }
        public int? hanmovecus { get; set; }
        public byte[]? Logo { get; set; }
        public byte[]? FormBillSea { get; set; }
        public string? AccountName { get; set; }
        public string? BankName { get; set; }
        public string? BankAddress { get; set; }
        public string? IBAN_CODE { get; set; }


    }
}
