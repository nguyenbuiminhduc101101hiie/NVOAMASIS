using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class PortModel
    {
        [Key]
        public Guid PORT_ID { get; set; }
        public string? PORT_CODE { get; set; }
        public string? PORT { get; set; }
        public string? MARKETCODETS { get; set; }
        public string? MARKETCODESALE { get; set; }
        public string? TEL { get; set; }
        public string? FAX { get; set; }
        public string? ADDRESS { get; set; }
        public string? COUNTRY { get; set; }
        public DateTime? DATEEXP { get; set; }
        public string? IG { get; set; }
        public string? TRADECODE { get; set; }
        public string? OverW20 { get; set; }
        public string? OverW40 { get; set; }
        public bool? APPROVE { get; set; }
        public bool? CONTINUED { get; set; }
        public bool? EDITABLE { get; set; }
        public string? USERID { get; set; }
        public DateTime? UPDATETIME { get; set; }
        public bool? show { get; set; } = true;
        public string? dept { get; set; }
    }
}
