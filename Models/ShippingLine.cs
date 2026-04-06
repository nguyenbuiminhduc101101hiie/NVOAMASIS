using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class ShippingLine
    {
        [Key]
        public Guid SHIPPINGLINEID { get; set; }
        public string? SHIPPINGLINE { get; set; }
        public string? ADDRESS { get; set; }
        public string? TEL { get; set; }
        public string? FAX { get; set; }
        public string? PIC { get; set; }
        public string? POS { get; set; }
        public string? Email { get; set; }
        public string? PIC1 { get; set; }
        public string? POS1 { get; set; }
        public string? Email1 { get; set; }
        public string? PIC2 { get; set; }
        public string? POS2 { get; set; }
        public string? Email2 { get; set; }
        public string? PIC3 { get; set; }
        public string? POS3 { get; set; }
        public string? Email3 { get; set; }
        public string? Remarks { get; set; }
        public bool? Continued { get; set; }
        public string? Noidi { get; set; }
        public string? Noiden { get; set; }
    }
}
