using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_GateOut
    {
        [Key]
        public Guid GateOutID { get; set; }
        public Guid Containerid { get; set; }
        public int? Seq { get; set; }
        public string? EIR { get; set; }
        public DateTime? DateIn { get; set; }
        public TimeSpan? TimeIn { get; set; }
        public string? Container { get; set; }
        public string? Type { get; set; }
        public string? ISO_SZTP { get; set; }
        public string? Opt { get; set; }
        public string? FE { get; set; }
        public string? Move { get; set; }
        public string? Location { get; set; }
        public string? Booking { get; set; }
        public string? Vessel { get; set; }
        public string? Voyage { get; set; }
        public string? POL { get; set; }
        public string? POD { get; set; }
        public string? Shipper { get; set; }
        public string? YOM { get; set; }
        public string? SealNo { get; set; }
        public string? Note1 { get; set; }
        public string? RequiredGrade { get; set; }
        public double? VGM { get; set; }
        public string? TruckNo { get; set; }
        public string? TruckerPhone { get; set; }
        public string? Grade { get; set; }
        public DateTime? DateOut { get; set; }
        public int? Days { get; set; }
        public string? Remark { get; set; }

    }
}
