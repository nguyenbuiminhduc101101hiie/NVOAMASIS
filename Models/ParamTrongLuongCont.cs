using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class ParamTrongLuongCont
    {
        [Key]
        public Guid Id { get; set; }
        public string? LoaiCont { get; set; } // 20 / 40 / 45 (feet)
        public string? TrongLuongToiDa { get; set; } // e.g. 28MT
    }
}

