using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_GateIn
    {
        [Key]
        public Guid GateinID { get; set; }
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
        public string? BillOfLading { get; set; }
        public string? Consignee { get; set; }
        public string? Truck { get; set; }
        public string? YOM { get; set; }
        public string? SealNo { get; set; }
        public string? SealNo1 { get; set; }
        public string? SealNo2 { get; set; }
        public double? VGM { get; set; }
        public string? Clean { get; set; }
        public double? MaxGross { get; set; }
        public double? TareWeight { get; set; }
        public string? Remark { get; set; }
        public string? Status { get; set; }
        public string? Grade { get; set; }
        public string? Note { get; set; }
        public DateTime? InsDate { get; set; }

    }
}
