using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Stock
    {
        [Key]
        public Guid StockID { get; set; }

        public string? Container { get; set; }
        public string? Type { get; set; }
        public string? IsoType { get; set; }
        public string? Opr { get; set; }
        public string? FE { get; set; }
        public string? Move { get; set; }
        public DateTime? DateIn { get; set; }
        public TimeSpan? TimeIn { get; set; }
        public string? Consignee { get; set; }
        public string? Position { get; set; }
        public int? Days { get; set; }
        public string? Location { get; set; }
        public string? YOM { get; set; }
        public double? TareWeight { get; set; }
        public string? SealNo { get; set; }
        public string? Note2 { get; set; }
        public string? Note3 { get; set; }
        public double? VGM { get; set; }
        public double? MaxGross { get; set; }
        public string? Remark { get; set; }
        public string? Status { get; set; }
        public string? Grade { get; set; }
        public string? CleanMethod { get; set; }
        public string? CleanStatus { get; set; }
        public DateTime? PtiDate { get; set; }
        public string? PtiSetting { get; set; }
        public string? PtiStatus { get; set; }
        public DateTime? EstimatedDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? RejectDate { get; set; }
        public DateTime? RepairedDate { get; set; }
        public DateTime? RegisteredDate { get; set; }
        public string? Shipper { get; set; }
        public string? Booking { get; set; }

        public DateTime? DateImport { get; set; }
        public string? UserImport { get; set; }
    }
}
