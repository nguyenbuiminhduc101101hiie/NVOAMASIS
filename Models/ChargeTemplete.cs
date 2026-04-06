using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class ChargeTemplete
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CHARGEID { get; set; }
        public double? Cont20 { get; set; }
        public double? Cont40 { get; set; }
        public double? Incvat { get; set; }
        public string? Cur { get; set; }
        public string? Ncc { get; set; }
        public string? Remarks { get; set; }
        public bool? APPROVE { get; set; }
        public bool? CONTINUED { get; set; }
        public bool? EDITABLE { get; set; }
        public string? USERupdate { get; set; }
        public DateTime? Dateupdate { get; set; }
    }
}
