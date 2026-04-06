using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Theodoilohang
    {
        [Key]
        public Guid theodoilohangID { get; set; }
        public Guid outboundid { get; set; }
        public string? Items { get; set; }
        public string? Remarks { get; set; }
        public string? Timer { get; set; }
        public string? UserUpdate { get; set; }
        public string? dateUpdate { get; set; }
        public string? validUser { get; set; }
        public string? ngaychungtu { get; set; }
        public string? bangoc { get; set; }
        public bool? intheodoi { get; set; }
        public string? LOCATION { get; set; }
        public string? toadoMap { get; set; }
        public bool? approve { get; set; }
    }
}
