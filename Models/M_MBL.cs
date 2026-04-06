using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NVOAMASIS.Models
{
    public class M_MBL
    {
        [Key]
        public Guid MblID { get; set; }
        public Guid? Jobid { get; set; }

        [ForeignKey(nameof(Jobid))]
        [JsonIgnore]
        public M_Job? Job { get; set; }

        [JsonIgnore]
        public List<M_HBL>? HBLs { get; set; }
        public string? SaleName { get; set; }
        public string? Bkno { get; set; }
        public string? Mbl { get; set; }
        public string? Shipper { get; set; }
        public string? Consignee { get; set; }
        public string? Notify1 { get; set; } = "Same as Consignee";
        public string? Notify2 { get; set; }
        public string? Vessel { get; set; }
        public string? Voy { get; set; }
        public string? Porname { get; set; }
        public string? Porcode { get; set; }
        public string? Polname { get; set; }
        public string? Polcode { get; set; }
        public string? Podname { get; set; }
        public string? Podcode { get; set; }
        public string? Delname { get; set; }
        public string? Delcode { get; set; }
        public string? FreightPayableAt { get; set; }
        public string? NumberOfOriginal { get; set; }
        public string? PlaceAndDate { get; set; }
        public string? FreightAmount { get; set; }
        public string? DateLaden { get; set; }
        public string? ForDelivery { get; set; }
        public string? Description { get; set; }
        public string? Say { get; set; }
        public string? LoadStowCount { get; set; }
        public string? ShipOnboard { get; set; }
        public Guid CustomerID { get; set; }
        public Guid AgentID { get; set; }
        public bool? Continued { get; set; } = true;
        public bool? Editable { get; set; } = true;
        public bool? Approve { get; set; } = false;
        public string? UserUpdate { get; set; }
        public string? DateUpdate { get; set; }
        public string? statusMBL { get; set; }
        public string? markAndNumbers { get; set; }
        public string? NoOfPackages { get; set; }
        public string? Gross { get; set; }
        public string? CBM { get; set; }

        public string? Air_ShipperAccountNumber { get; set; }
        public string? Air_ConsigneeAccountNumber { get; set; }
        public string? Air_IssuingCarrier { get; set; }
        public string? Air_AccountingInfomation { get; set; }
        public string? Air_AgentIATACode { get; set; }
        public string? Air_AccountNo { get; set; }
        public string? Air_AirportofDeparture { get; set; }
        public string? Air_to { get; set; }
        public string? Air_ByfirstCarrier { get; set; }
        public string? Air_to2 { get; set; }
        public string? Air_by2 { get; set; }
        public string? Air_to3 { get; set; }
        public string? Air_by3 { get; set; }
        public string? Air_Currency { get; set; }
        public string? Air_CHGSCode { get; set; }
        public string? Air_PPD { get; set; }
        public string? Air_COLL { get; set; }
        public string? Air_otherPPD { get; set; }
        public string? Air_otherCOLL { get; set; }
        public string? Air_DeclaredValuedForCarriage { get; set; }
        public string? Air_DeclaredValueForCustoms { get; set; }
        public string? Air_AirportOfDestination { get; set; }
        public string? Air_FlightDate1 { get; set; }
        public string? Air_FlightDate2 { get; set; }
        public string? Air_AmountOfInsurance { get; set; }
        public string? Air_HandlingInfomation { get; set; }
        public string? Air_NoOfPiecesRCP { get; set; }
        public string? Air_GrossWieght { get; set; }
        public string? Air_kglb { get; set; }
        public string? Air_RateClass { get; set; }
        public string? Air_CommodityItemNo { get; set; }
        public string? Air_ChargeableWeight { get; set; }
        public string? Air_RateCharge { get; set; }
        public string? Air_Total { get; set; }
        public string? Air_NatureAndquantityOfgoods { get; set; }
        public string? Air_WeightCharge_Prepaid { get; set; }
        public string? Air_WeightCharge_Collect { get; set; }
        public string? Air_Valuation_Prepaid { get; set; }
        public string? Air_Valuation_Collect { get; set; }
        public string? Air_Tax_Prepaid { get; set; }
        public string? Air_Tax_Collect { get; set; }
        public string? Air_TotalOtherChargesDueAgent_Prepaid { get; set; }
        public string? Air_TotalOtherChargesDueAgent_Collect { get; set; }
        public string? Air_TotalOtherChargesDueCarrier_Prepaid { get; set; }
        public string? Air_TotalOtherChargesDueCarrier_Collect { get; set; }
        public string? Air_TotalPrepaid { get; set; }
        public string? Air_TotalCollect { get; set; }
        public string? Air_CurrencyConversionRates { get; set; }
        public string? Air_CCChargesInDestCurrenct { get; set; }
        public string? Air_ChargesatDestination { get; set; }
        public string? Air_TotalCollectCharges { get; set; }
        public string? Air_PAM { get; set; }
        public string? Air_OtherCharges { get; set; }
        public string? Air_HisAgent { get; set; }
        public DateTime? ETA { get; set; }
        public DateTime? ETD { get; set; }
        public bool? dathudebit { get; set; } = false;
        public bool? dachicredit { get; set; } = false;
        public bool? dathuchidaily { get; set; } = false;
        public string? mbl_Truck_LenhDieuXeNo { get; set; }
        public string? mbl_Truck_YeucauTruckingNo { get; set; }
        public DateTime? ActualDelDate { get; set; }
        public M_MBL DeepCopy()
        {
            string json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<M_MBL>(json);
        }
    }
    //public class M_MBL
    //{
    //    [Key]
    //    public Guid MblID { get; set; }
    //    public Guid? Jobid { get; set; }

    //    [JsonIgnore]
    //    public List<M_HBL>? HBLs { get; set; }
    //    public string? Bkno { get; set; }
    //    public string? Mbl { get; set; }
    //    public string? Shipper { get; set; }
    //    public string? Consignee { get; set; }
    //    public string? Notify1 { get; set; }
    //    public string? Notify2 { get; set; }
    //    public string? Vessel { get; set; }
    //    public string? Voy { get; set; }
    //    public string? Porname { get; set; }
    //    public string? Porcode { get; set; }
    //    public string? Polname { get; set; }
    //    public string? Polcode { get; set; }
    //    public string? Podname { get; set; }
    //    public string? Podcode { get; set; }
    //    public string? Delname { get; set; }
    //    public string? Delcode { get; set; }
    //    public string? FreightPayableAt { get; set; }
    //    public string? NumberOfOriginal { get; set; }
    //    public string? PlaceAndDate { get; set; }
    //    public string? FreightAmount { get; set; }
    //    public string? DateLaden { get; set; }
    //    public string? ForDelivery { get; set; }
    //    public string? Description { get; set; }
    //    public string? Say { get; set; }
    //    public string? LoadStowCount { get; set; }
    //    public string? ShipOnboard { get; set; }
    //    public Guid CustomerID { get; set; }
    //    public Guid AgentID { get; set; }
    //    public bool? Continued { get; set; } = true;
    //    public bool? Editable { get; set; } = true;
    //    public bool? Approve { get; set; } = false;
    //    public string? UserUpdate { get; set; }
    //    public string? DateUpdate { get; set; }
    //    public string? statusMBL { get; set; }
    //    public string? markAndNumbers { get; set; }
    //    public string? NoOfPackages { get; set; }
    //    public string? Gross { get; set; }
    //    public string? CBM { get; set; }

    //    public string? Air_ShipperAccountNumber { get; set; }
    //    public string? Air_ConsigneeAccountNumber { get; set; }
    //    public string? Air_IssuingCarrier { get; set; }
    //    public string? Air_AccountingInfomation { get; set; }
    //    public string? Air_AgentIATACode { get; set; }
    //    public string? Air_AccountNo { get; set; }
    //    public string? Air_AirportofDeparture { get; set; }
    //    public string? Air_to { get; set; }
    //    public string? Air_ByfirstCarrier { get; set; }
    //    public string? Air_to2 { get; set; }
    //    public string? Air_by2 { get; set; }
    //    public string? Air_to3 { get; set; }
    //    public string? Air_by3 { get; set; }
    //    public string? Air_Currency { get; set; }
    //    public string? Air_CHGSCode { get; set; }
    //    public string? Air_PPD { get; set; }
    //    public string? Air_COLL { get; set; }
    //    public string? Air_otherPPD { get; set; }
    //    public string? Air_otherCOLL { get; set; }
    //    public string? Air_DeclaredValuedForCarriage { get; set; }
    //    public string? Air_DeclaredValueForCustoms { get; set; }
    //    public string? Air_AirportOfDestination { get; set; }
    //    public string? Air_FlightDate1 { get; set; }
    //    public string? Air_FlightDate2 { get; set; }
    //    public string? Air_AmountOfInsurance { get; set; }
    //    public string? Air_HandlingInfomation { get; set; }
    //    public string? Air_NoOfPiecesRCP { get; set; }
    //    public string? Air_GrossWieght { get; set; }
    //    public string? Air_kglb { get; set; }
    //    public string? Air_RateClass { get; set; }
    //    public string? Air_CommodityItemNo { get; set; }
    //    public string? Air_ChargeableWeight { get; set; }
    //    public string? Air_RateCharge { get; set; }
    //    public string? Air_Total { get; set; }
    //    public string? Air_NatureAndquantityOfgoods { get; set; }
    //    public string? Air_WeightCharge_Prepaid { get; set; }
    //    public string? Air_WeightCharge_Collect { get; set; }
    //    public string? Air_Valuation_Prepaid { get; set; }
    //    public string? Air_Valuation_Collect { get; set; }
    //    public string? Air_Tax_Prepaid { get; set; }
    //    public string? Air_Tax_Collect { get; set; }
    //    public string? Air_TotalOtherChargesDueAgent_Prepaid { get; set; }
    //    public string? Air_TotalOtherChargesDueAgent_Collect { get; set; }
    //    public string? Air_TotalOtherChargesDueCarrier_Prepaid { get; set; }
    //    public string? Air_TotalOtherChargesDueCarrier_Collect { get; set; }
    //    public string? Air_TotalPrepaid { get; set; }
    //    public string? Air_TotalCollect { get; set; }
    //    public string? Air_CurrencyConversionRates { get; set; }
    //    public string? Air_CCChargesInDestCurrenct { get; set; }
    //    public string? Air_ChargesatDestination { get; set; }
    //    public string? Air_TotalCollectCharges { get; set; }
    //    public string? Air_PAM { get; set; }
    //    public string? Air_OtherCharges { get; set; }
    //    public string? Air_HisAgent { get; set; }
    //    public DateTime? ETA { get; set; }
    //    public DateTime? ETD { get; set; }
    //    public bool? dathudebit { get; set; }
    //    public bool? dachicredit { get; set; }
    //    public bool? dathuchidaily { get; set; }
    //    public string? mbl_Truck_LenhDieuXeNo { get; set; }
    //    public string? mbl_Truck_YeucauTruckingNo { get; set; }
    //    public DateTime? ActualDelDate { get; set; }
    //    public M_MBL DeepCopy()
    //    {
    //        string json = JsonSerializer.Serialize(this);
    //        return JsonSerializer.Deserialize<M_MBL>(json);
    //    }
    //}

}