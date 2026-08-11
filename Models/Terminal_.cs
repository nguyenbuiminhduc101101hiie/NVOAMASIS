using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class Terminal_
    {               
        [Key]
        public Guid TerminalID { get; set; }
        public string? Code { get; set; }
        public string? TermiNalName { get; set; }
        public string? Address { get; set; }
        public string? tel { get; set; }
        public int? Capacity { get; set; }
        public double? FreeStorage { get; set; }
        public int? ValidOrder { get; set; }
        public int? OrderReport { get; set; }
        public bool? Continued { get; set; }
        public bool? Editable { get; set; }
        public bool? Approve { get; set; }
        public string? UserID { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string? code_nvocc { get; set; }
        public string? Codeha { get; set; }
    }
}
