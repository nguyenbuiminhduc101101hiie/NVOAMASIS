using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NVOAMASIS.Models
{
    public class M_HBL
    {
        [Key]
        [Display(Name = "")]
        public Guid hblID { get; set; }
        [Display(Name = "")]
        public Guid mblid { get; set; }

        [ForeignKey(nameof(mblid))]
        public M_MBL? MBL { get; set; }

        [Display(Name = "BK No")]
        public string? bkno { get; set; }
        [Display(Name = "HBL No")]
        public string? hbl { get; set; }
        [Display(Name = "")]
        public string? statusHBL { get; set; }
        [Display(Name = "Shipper")]
        public string? shipper { get; set; }
        [Display(Name = "Consignee")]
        public string? consignee { get; set; }
        [Display(Name = "")]
        public string? notify1 { get; set; } = "Same as Consignee";
        [Display(Name = "")]
        public string? notify2 { get; set; }
        [Display(Name = "")]
        public string? vessel { get; set; }
        [Display(Name = "")]
        public string? voy { get; set; }
        [Display(Name = "")]
        public string? porname { get; set; }
        [Display(Name = "")]
        public string? porcode { get; set; }
        [Display(Name = "")]
        public string? polname { get; set; }
        [Display(Name = "")]
        public string? polcode { get; set; }
        [Display(Name = "")]
        public string? podname { get; set; }
        [Display(Name = "")]
        public string? podcode { get; set; }
        [Display(Name = "")]
        public string? delcode { get; set; }
        [Display(Name = "")]
        public string? delName { get; set; }
        [Display(Name = "")]
        public string? markAndNumbers { get; set; }
        [Display(Name = "")]
        public string? NoOfPackages { get; set; }
        [Display(Name = "")]
        public string? gross { get; set; }
        [Display(Name = "")]
        public string? cbm { get; set; }
        [Display(Name = "")]
        public string? freightPayableAt { get; set; }
        [Display(Name = "")]
        public string? numberOfOriginal { get; set; }
        [Display(Name = "")]
        public string? placeAndDate { get; set; }
        [Display(Name = "")]

        public string? collectat { get; set; }
        [Display(Name = "")]
        public string? freightAmount { get; set; }
        [Display(Name = "")]
        public string? dateLaden { get; set; }
        [Display(Name = "")]
        public string? forDelivery { get; set; }
        [Display(Name = "")]
        public string? description { get; set; }
        [Display(Name = "")]
        public string? shippingMark { get; set; }
        [Display(Name = "")]
        public string? statusMBL { get; set; }
        [Display(Name = "")]
        public string? say { get; set; }
        [Display(Name = "")]
        public string? loadStowCount { get; set; }
        [Display(Name = "")]
        public string? shipOnboard { get; set; }
        [Display(Name = "Customer")]
        public Guid CustomerID { get; set; }
        [Display(Name = "")]
        public Guid AgentID { get; set; }
        [Display(Name = "")]
        public bool? Continued { get; set; } = true;
        [Display(Name = "")]
        public bool? editable { get; set; } = true;
        [Display(Name = "")]
        public bool? approve { get; set; } = false;
        [Display(Name = "")]
        public string? userupdate { get; set; }
        [Display(Name = "")]
        public string? dateupdate { get; set; }

        [Display(Name = "")]
        public string? Air_ShipperAccountNumber { get; set; }
        [Display(Name = "")]
        public string? Air_ConsigneeAccountNumber { get; set; }
        [Display(Name = "")]
        public string? Air_IssuingCarrier { get; set; }
        [Display(Name = "")]
        public string? Air_AccountingInfomation { get; set; }
        [Display(Name = "")]
        public string? Air_AgentIATACode { get; set; }
        [Display(Name = "")]
        public string? Air_AccountNo { get; set; }
        [Display(Name = "")]
        public string? Air_AirportofDeparture { get; set; }
        [Display(Name = "")]
        public string? Air_to { get; set; }
        [Display(Name = "")]
        public string? Air_ByfirstCarrier { get; set; }
        [Display(Name = "")]
        public string? Air_to2 { get; set; }
        [Display(Name = "")]
        public string? Air_by2 { get; set; }
        [Display(Name = "")]
        public string? Air_to3 { get; set; }
        [Display(Name = "")]
        public string? Air_by3 { get; set; }
        [Display(Name = "")]
        public string? Air_Currency { get; set; }
        [Display(Name = "")]
        public string? Air_CHGSCode { get; set; }
        [Display(Name = "")]
        public string? Air_PPD { get; set; }
        [Display(Name = "")]
        public string? Air_COLL { get; set; }
        [Display(Name = "")]
        public string? Air_otherPPD { get; set; }
        [Display(Name = "")]
        public string? Air_otherCOLL { get; set; }
        [Display(Name = "")]
        public string? Air_DeclaredValuedForCarriage { get; set; }
        [Display(Name = "")]
        public string? Air_DeclaredValueForCustoms { get; set; }
        [Display(Name = "")]
        public string? Air_AirportOfDestination { get; set; }
        [Display(Name = "")]
        public string? Air_FlightDate1 { get; set; }
        [Display(Name = "")]
        public string? Air_FlightDate2 { get; set; }
        [Display(Name = "")]
        public string? Air_AmountOfInsurance { get; set; }
        [Display(Name = "")]
        public string? Air_HandlingInfomation { get; set; }
        [Display(Name = "")]
        public string? Air_NoOfPiecesRCP { get; set; }
        [Display(Name = "")]
        public string? Air_GrossWieght { get; set; }
        [Display(Name = "")]
        public string? Air_kglb { get; set; }
        [Display(Name = "")]
        public string? Air_RateClass { get; set; }
        [Display(Name = "")]
        public string? Air_CommodityItemNo { get; set; }
        [Display(Name = "")]
        public string? Air_ChargeableWeight { get; set; }
        [Display(Name = "")]
        public string? Air_RateCharge { get; set; }
        [Display(Name = "")]
        public string? Air_Total { get; set; }
        [Display(Name = "")]
        public string? Air_NatureAndquantityOfgoods { get; set; }
        [Display(Name = "")]
        public string? Air_WeightCharge_Prepaid { get; set; }
        [Display(Name = "")]
        public string? Air_WeightCharge_Collect { get; set; }
        [Display(Name = "")]
        public string? Air_Valuation_Prepaid { get; set; }
        [Display(Name = "")]
        public string? Air_Valuation_Collect { get; set; }
        [Display(Name = "")]
        public string? Air_Tax_Prepaid { get; set; }
        [Display(Name = "")]
        public string? Air_Tax_Collect { get; set; }
        [Display(Name = "")]
        public string? Air_TotalOtherChargesDueAgent_Prepaid { get; set; }
        [Display(Name = "")]
        public string? Air_TotalOtherChargesDueAgent_Collect { get; set; }
        [Display(Name = "")]
        public string? Air_TotalOtherChargesDueCarrier_Prepaid { get; set; }
        [Display(Name = "")]
        public string? Air_TotalOtherChargesDueCarrier_Collect { get; set; }
        [Display(Name = "")]
        public string? Air_TotalPrepaid { get; set; }
        [Display(Name = "")]
        public string? Air_TotalCollect { get; set; }
        [Display(Name = "")]
        public string? Air_CurrencyConversionRates { get; set; }
        [Display(Name = "")]
        public string? Air_CCChargesInDestCurrenct { get; set; }
        [Display(Name = "")]
        public string? Air_OtherCharges { get; set; }
        [Display(Name = "")]
        public string? Air_ChargesatDestination { get; set; }
        [Display(Name = "")]
        public string? Air_TotalCollectCharges { get; set; }
        [Display(Name = "")]
        public string? Air_PAM { get; set; }
        [Display(Name = "")]
        public string? Air_HisAgent { get; set; }
        [Display(Name = "")]
        public string? Air_wareHouse { get; set; }
        [Display(Name = "")]
        public string? Air_type { get; set; }
        [Display(Name = "")]
        public string? SaleName { get; set; }
        [Display(Name = "")]

        public string? OPS { get; set; }
        [Display(Name = "")]

        public string? DOC { get; set; }
        [Display(Name = "")]
        public DateTime? ETA { get; set; }
        [Display(Name = "")]
        public DateTime? ETD { get; set; }
        [Display(Name = "")]
        public bool? issueinvoice { get; set; } = false;
        public bool? dathudebit { get; set; }
        [Display(Name = "")]
        public bool? dachicredit { get; set; }
        [Display(Name = "")]
        public bool? dathuchidaily { get; set; }
        [Display(Name = "")]
        public string? Truck_LenhDieuXeNo { get; set; }

        //[JsonIgnore]
        //[ForeignKey(nameof(Truck_LenhDieuXeNo))]
        //public M_LenhDieuXe? LenhDieuXe { get; set; }
        [Display(Name = "")]
        public string? Truck_YeucauTruckingNo { get; set; }
        [Display(Name = "")]
        public Guid? Jobid { get; set; }
        [Display(Name = "")]
        public string? TKHQ_MaHaiQuan { get; set; }
        [Display(Name = "")]
        public string? TKHQ_TenHaiQuan { get; set; }
        [Display(Name = "")]
        public string? TKHQ_MaLoaiHinh { get; set; }
        [Display(Name = "")]
        public string? TKHQ_TenLoaiHinh { get; set; }
        [Display(Name = "")]
        public string? TKHQ_NamDangKy { get; set; }
        [Display(Name = "")]
        public string? TKHQ_SoToKhai { get; set; }
        [Display(Name = "")]
        public DateTime? TKHQ_NgayDangKy { get; set; }
        [Display(Name = "")]
        public string? TKHQ_MaDonVi { get; set; }
        [Display(Name = "")]
        public DateTime? TKHQ_NgayThongQuan { get; set; }
        [Display(Name = "")]
        public DateTime? TKHQ_NgayQuaKhuVucGiamSat { get; set; }
        [Display(Name = "")]
        public string? TKHQ_TenLuong { get; set; }

        [Display(Name = "")]
        public DateTime? ActualDelDate { get; set; }
        [Display(Name = "")]
        public DateTime? datereport { get; set; } = DateTime.Now;

        [Display(Name = "")]
        public bool? release { get; set; }
        public string? releaseNote { get; set; }
        public string? TKHQ_SoTKdangky { get; set; }
        public bool? Required { get; set; } = false;
        public bool? Surrendered { get; set; } = false;
        public bool? TelexRelease { get; set; } = false;
        public bool? Prepaid { get; set; } = false;
        public bool? Collect { get; set; } = false;
        public bool? freight { get; set; } = false;
        public string? remarks { get; set; }
        public double? ex_rate { get; set; }
        public string? IssueAt { get; set; }

        public M_HBL DeepCopy()
        {
            string json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<M_HBL>(json);
        }
    }
    //public class M_HBL
    //{
    //    [Key]
    //    [Display(Name = "")]
    //    public Guid hblID { get; set; }
    //    [Display(Name = "")]
    //    public Guid mblid { get; set; }
    //    [Display(Name = "BK No")]
    //    public string? bkno { get; set; }
    //    [Display(Name = "HBL No")]
    //    public string? hbl { get; set; }
    //    [Display(Name = "")]
    //    public string? statusHBL { get; set; }
    //    [Display(Name = "Shipper")]
    //    public string? shipper { get; set; }
    //    [Display(Name = "Consignee")]
    //    public string? consignee { get; set; }
    //    [Display(Name = "")]
    //    public string? notify1 { get; set; }
    //    [Display(Name = "")]
    //    public string? notify2 { get; set; }
    //    [Display(Name = "")]
    //    public string? vessel { get; set; }
    //    [Display(Name = "")]
    //    public string? voy { get; set; }
    //    [Display(Name = "")]
    //    public string? porname { get; set; }
    //    [Display(Name = "")]
    //    public string? porcode { get; set; }
    //    [Display(Name = "")]
    //    public string? polname { get; set; }
    //    [Display(Name = "")]
    //    public string? polcode { get; set; }
    //    [Display(Name = "")]
    //    public string? podname { get; set; }
    //    [Display(Name = "")]
    //    public string? podcode { get; set; }
    //    [Display(Name = "")]
    //    public string? delcode { get; set; }
    //    [Display(Name = "")]
    //    public string? delName { get; set; }
    //    [Display(Name = "")]
    //    public string? markAndNumbers { get; set; }
    //    [Display(Name = "")]
    //    public string? NoOfPackages { get; set; }
    //    [Display(Name = "")]
    //    public string? gross { get; set; }
    //    [Display(Name = "")]
    //    public string? cbm { get; set; }
    //    [Display(Name = "")]
    //    public string? freightPayableAt { get; set; }
    //    [Display(Name = "")]
    //    public string? numberOfOriginal { get; set; }
    //    [Display(Name = "")]
    //    public string? placeAndDate { get; set; }
    //    [Display(Name = "")]
    //    public string? freightAmount { get; set; }
    //    [Display(Name = "")]
    //    public string? dateLaden { get; set; }
    //    [Display(Name = "")]
    //    public string? forDelivery { get; set; }
    //    [Display(Name = "")]
    //    public string? description { get; set; }
    //    [Display(Name = "")]
    //    public string? shippingMark { get; set; }
    //    [Display(Name = "")]
    //    public string? statusMBL { get; set; }
    //    [Display(Name = "")]
    //    public string? say { get; set; }
    //    [Display(Name = "")]
    //    public string? loadStowCount { get; set; }
    //    [Display(Name = "")]
    //    public string? shipOnboard { get; set; }
    //    [Display(Name = "Customer")]
    //    public Guid CustomerID { get; set; }
    //    [Display(Name = "")]
    //    public Guid AgentID { get; set; }
    //    [Display(Name = "")]
    //    public bool? Continued { get; set; } = true;
    //    [Display(Name = "")]
    //    public bool? editable { get; set; } = true;
    //    [Display(Name = "")]
    //    public bool? approve { get; set; } = false;
    //    [Display(Name = "")]
    //    public string? userupdate { get; set; }
    //    [Display(Name = "")]
    //    public string? dateupdate { get; set; }

    //    [Display(Name = "")]
    //    public string? Air_ShipperAccountNumber { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_ConsigneeAccountNumber { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_IssuingCarrier { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_AccountingInfomation { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_AgentIATACode { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_AccountNo { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_AirportofDeparture { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_to { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_ByfirstCarrier { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_to2 { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_by2 { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_to3 { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_by3 { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_Currency { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_CHGSCode { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_PPD { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_COLL { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_otherPPD { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_otherCOLL { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_DeclaredValuedForCarriage { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_DeclaredValueForCustoms { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_AirportOfDestination { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_FlightDate1 { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_FlightDate2 { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_AmountOfInsurance { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_HandlingInfomation { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_NoOfPiecesRCP { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_GrossWieght { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_kglb { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_RateClass { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_CommodityItemNo { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_ChargeableWeight { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_RateCharge { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_Total { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_NatureAndquantityOfgoods { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_WeightCharge_Prepaid { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_WeightCharge_Collect { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_Valuation_Prepaid { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_Valuation_Collect { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_Tax_Prepaid { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_Tax_Collect { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_TotalOtherChargesDueAgent_Prepaid { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_TotalOtherChargesDueAgent_Collect { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_TotalOtherChargesDueCarrier_Prepaid { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_TotalOtherChargesDueCarrier_Collect { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_TotalPrepaid { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_TotalCollect { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_CurrencyConversionRates { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_CCChargesInDestCurrenct { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_OtherCharges { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_ChargesatDestination { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_TotalCollectCharges { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_PAM { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_HisAgent { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_wareHouse { get; set; }
    //    [Display(Name = "")]
    //    public string? Air_type { get; set; }
    //    [Display(Name = "")]
    //    public string? SaleName { get; set; }
    //    [Display(Name = "")]
    //    public DateTime? ETA { get; set; }
    //    [Display(Name = "")]
    //    public DateTime? ETD { get; set; }
    //    [Display(Name = "")]
    //    public bool? issueinvoice { get; set; }
    //    public bool? dathudebit { get; set; }
    //    [Display(Name = "")]
    //    public bool? dachicredit { get; set; }
    //    [Display(Name = "")]
    //    public bool? dathuchidaily { get; set; }
    //    [Display(Name = "")]
    //    public string? Truck_LenhDieuXeNo { get; set; }

    //    //[JsonIgnore]
    //    //[ForeignKey(nameof(Truck_LenhDieuXeNo))]
    //    //public M_LenhDieuXe? LenhDieuXe { get; set; }

    //    [Display(Name = "")]
    //    public string? Truck_YeucauTruckingNo { get; set; }
    //    [Display(Name = "")]
    //    public Guid? Jobid { get; set; }
    //    [Display(Name = "")]
    //    public string? TKHQ_MaHaiQuan { get; set; }
    //    [Display(Name = "")]
    //    public string? TKHQ_TenHaiQuan { get; set; }
    //    [Display(Name = "")]
    //    public string? TKHQ_MaLoaiHinh { get; set; }
    //    [Display(Name = "")]
    //    public string? TKHQ_TenLoaiHinh { get; set; }
    //    [Display(Name = "")]
    //    public string? TKHQ_NamDangKy { get; set; }
    //    [Display(Name = "")]
    //    public string? TKHQ_SoToKhai { get; set; }
    //    [Display(Name = "")]
    //    public DateTime? TKHQ_NgayDangKy { get; set; }
    //    [Display(Name = "")]
    //    public string? TKHQ_MaDonVi { get; set; }
    //    [Display(Name = "")]
    //    public DateTime? TKHQ_NgayThongQuan { get; set; }
    //    [Display(Name = "")]
    //    public DateTime? TKHQ_NgayQuaKhuVucGiamSat { get; set; }
    //    [Display(Name = "")]
    //    public string? TKHQ_TenLuong { get; set; }

    //    [Display(Name = "")]
    //    public DateTime? ActualDelDate { get; set; }
    //    [Display(Name = "")]
    //    public DateTime? datereport { get; set; }


    //    [Display(Name = "")]
    //    public bool? release { get; set; }
    //    public string? releaseNote { get; set; }
    //    public M_HBL DeepCopy()
    //    {
    //        string json = JsonSerializer.Serialize(this);
    //        return JsonSerializer.Deserialize<M_HBL>(json);
    //    }
    //}
}