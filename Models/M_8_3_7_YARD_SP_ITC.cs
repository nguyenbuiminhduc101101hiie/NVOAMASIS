using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("M_8_3_7_YARD_SP_ITC")]
    public class M_8_3_7_YARD_SP_ITC
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? ImportBatchNo { get; set; }
        public string? ImportFileName { get; set; }
        public string? SourceSheet { get; set; }
        public string? ReportType { get; set; }
        public string? Depot { get; set; }
        public string? Line { get; set; }
        public int? RowNo { get; set; }
        public int? STT { get; set; }

        public string? ContrNo { get; set; }
        public string? ContainerType { get; set; }
        public string? Size { get; set; }
        public string? Category { get; set; }
        public string? Movement { get; set; }
        public string? Status { get; set; }
        public string? FE { get; set; }
        public string? IO { get; set; }

        public DateTime? DateIn { get; set; }
        public DateTime? DateOut { get; set; }
        public DateTime? DateInStuff { get; set; }
        public DateTime? StuffingDate { get; set; }
        public DateTime? UnStuffingDate { get; set; }
        public decimal? Days { get; set; }
        public string? Dwell { get; set; }
        public decimal? ImpDays { get; set; }
        public decimal? GW { get; set; }
        public decimal? VGM { get; set; }

        public string? Booking { get; set; }
        public string? BKBLNo { get; set; }
        public string? Seal { get; set; }
        public string? Vessel { get; set; }
        public string? VesselVoyage { get; set; }
        public string? VoyIn { get; set; }
        public string? VoyOut { get; set; }
        public string? PortCD { get; set; }
        public string? POD { get; set; }
        public string? FPOD { get; set; }

        public string? Shipper { get; set; }
        public string? Oper { get; set; }
        public string? TruckBarge { get; set; }
        public string? Truck { get; set; }
        public string? SoXe { get; set; }
        public string? Move { get; set; }
        public string? Vent { get; set; }
        public string? Location { get; set; }
        public string? TTSC { get; set; }
        public string? CustomsClear { get; set; }
        public string? AVDM { get; set; }
        public string? Remark { get; set; }

        public string? RowHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
