using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Buyer
    {
        [Key]
        public Guid BuyerID { get; set; }
        public Guid Customer_Id { get; set; }
        public string? Buyer { get; set; }
        public string? Address { get; set; }
        public string? Tel { get; set; }
        public string? Fax { get; set; }
        public string? email { get; set; }
        public string? website { get; set; }
        public string? Country { get; set; }
        public string? PIC1Name { get; set; }
        public string? PIC1Pos { get; set; }
        public string? PIC1Tel { get; set; }
        public string? PIC1Email { get; set; }
        public string? Commodity { get; set; }
        public bool? editable { get; set; }
        public bool? approve { get; set; }
        public string? Continued { get; set; }
        public string? userid { get; set; }
        public DateTime? updatetime { get; set; }
        public bool? consigneeshipper { get; set; }
    }
}
