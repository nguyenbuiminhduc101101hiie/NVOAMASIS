using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_FormBooking
    {
        [Key]
        public Guid id { get; set; }
        public DateTime? Ngay { get; set; }
        public string? Sale { get; set; }
        public string? Line { get; set; }
        public Guid? Shipper { get; set; }
        public string? Volume { get; set; }
        public string? POL_POD { get; set; }
        public DateTime? ETD { get; set; }
        public string? FreeTime { get; set; }
        public string? PP_CC { get; set; }
        public string? Description { get; set; }
        public double? Buy_Rate { get; set; }
        public string? Remark { get; set; }
        public string? Acc { get; set; }
        public string? Type_Bill { get; set; }
        public Guid? BookingId { get; set; }

    }
}
