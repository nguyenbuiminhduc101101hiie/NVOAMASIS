using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Commondity
    {
        [Key]
        public Guid Commondity_ID { get; set; }
        public string? Commondity { get; set; }
        public string? EDI { get; set; }
        public string? Remarks { get; set; }
        public bool? Continued { get; set; }
        public bool? Editable { get; set; }
        public string? UserID { get; set; }
        public DateTime? Updatetime { get; set; }
        public bool? Approve { get; set; }
    }
}
