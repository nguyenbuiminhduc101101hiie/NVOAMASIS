namespace NVOAMASIS.Models
{
    public class M_LocalCharge_pt
    {
        public Guid Id { get; set; }
        public int? Index_ { get; set; }
        public string? Carr { get; set; }

        /// <summary>COC = vỏ hãng tàu, SOC = vỏ khách hàng</summary>
        public string? ContainerType { get; set; }

        public double? Thc20dc { get; set; }
        public double? Thc40hc { get; set; }
        public double? Thc20rf { get; set; }
        public double? Thc40rf { get; set; }
        public double? EBS20dc { get; set; }
        public double? EBS40hc { get; set; }
        public double? EBS20rf { get; set; }
        public double? EBS40rf { get; set; }

        // COC-specific
        public double? DEM { get; set; }
        public double? DET { get; set; }
        public double? Deposit { get; set; }
        public int? FreeTimeDemDet { get; set; }
        public double? RepairFee { get; set; }

        // SOC-specific
        public double? SurveyFee { get; set; }
        public double? FumigationFee { get; set; }
        public double? SocHandlingFee { get; set; }

        public double? Seal { get; set; }
        public double? BL { get; set; }
        public double? Telex { get; set; }
        public double? ENS { get; set; }
        public double? LatePayMentFEE { get; set; }
        public double? CIC { get; set; }
        public double? LSS { get; set; }
        public double? ISPS { get; set; }
        public double? MTF { get; set; }
        public double? MAF { get; set; }
        public string? CURRENCY { get; set; }
        public string? Remarks { get; set; }
        public string? Userupdate { get; set; }
        public string? Dateupdate { get; set; }
        public DateTime? Ngaybatdau { get; set; }
        public DateTime? Ngayketthuc { get; set; }
    }
}
