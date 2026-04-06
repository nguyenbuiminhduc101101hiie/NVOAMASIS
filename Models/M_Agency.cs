using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Agency
    {
        [Key]
        public Guid? Agency_ID { get; set; }
        public string? AgencyName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Tel { get; set; }
        public string? Fax { get; set; }
        public double? HangdingFee { get; set; }
        public string? AgencyRemarks { get; set; }
        public string? email { get; set; }
        public string? website { get; set; }
        public string? pic_ { get; set; }
        public string? Ass { get; set; }
        public string? skype { get; set; }
        public string? yahoo { get; set; }
        public bool? Approve { get; set; }
        public bool? Continued { get; set; }
        public bool? Editable { get; set; }
        public string? UserID { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
