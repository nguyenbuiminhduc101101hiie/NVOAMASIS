using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_VesselSpace
    {
        [Key]
        public Guid? VesselSpaceID { get; set; }
        public string? PODCode { get; set; }
        public string? Vessel { get; set; }
        public string? Voy { get; set; }
        public string? ETD { get; set; }
        public string? Fre { get; set; }
        public string? GATEINLADEN { get; set; }
        public string? Carrier { get; set; }
        public int? Confirm_ { get; set; }
        public string? Ghichu { get; set; }
        public bool? Editable { get; set; }
        public bool? Approve { get; set; }
        public bool? Continued { get; set; }
        public string? Userupdate { get; set; }
        public string? Dateupdate { get; set; }
    }
}
