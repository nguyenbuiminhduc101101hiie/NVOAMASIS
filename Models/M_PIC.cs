using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_PIC
    {
        [Key]
        public Guid PIC_ID { get; set; }
        public Guid? Customer_ID { get; set; }
        public string? PIC { get; set; }
        public string? Pos { get; set; }
        public string? DirectLine { get; set; }
        public string? Fax { get; set; }
        public string? hp { get; set; }
        public string? EMail { get; set; }
        public bool? Editable { get; set; }
        public bool? Approve { get; set; }
        public bool? Continued { get; set; }
        public string? UserId { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
