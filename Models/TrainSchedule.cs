using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace NVOAMASIS.Models
{
    public class TrainSchedule
    {
        [Key]
        public Guid TerminalDeparture_ID { get; set; }
        public string? Code { get; set; }
        public string? AgencyName { get; set; }
        public Guid? Vessel_ID { get; set; }
        public Guid? Port_ID { get; set; }
        public string? Voyage { get; set; }
        public string? WharfName { get; set; }
        public string? ArrivalPilot_A { get; set; }
        public string? PilotOnBoard_A { get; set; }
        public string? PilotOnBoard_D { get; set; }
        public string? FirstLine { get; set; }
        public string? LastLine { get; set; }
        public string? Commenced { get; set; }
        public string? Finished { get; set; }
        public double? FO_A { get; set; }
        public double? FO_D { get; set; }
        public double? DO_A { get; set; }
        public double? DO_D { get; set; }
        public double? FreshWater_A { get; set; }
        public double? FreshWater_D { get; set; }
        public double? DraftFwd_A { get; set; }
        public double? DraftFwd_D { get; set; }
        public double? DraftAft_A { get; set; }
        public double? DraftAft_D { get; set; }
        public int? TTLFull_A { get; set; }
        public int? TTLFull_D { get; set; }
        public int? TTLEmpty_A { get; set; }
        public int? TTLEmpty_D { get; set; }
        public int? TTLGross_A { get; set; }
        public int? TTLGross_D { get; set; }
        public int? TugsIn { get; set; }
        public int? TugsOut { get; set; }
        public string? Remark_A { get; set; }
        public string? Remark_D { get; set; }
        public string? PortTime { get; set; }
        public string? BerThedTime { get; set; }
        public string? OperationTime { get; set; }
        public string? CashToCaption { get; set; }
        public string? NextPortCall { get; set; }
        public string? ETANextPort { get; set; }
        public string? HatchCover { get; set; }
        public string? GM { get; set; }
        public string? Fore { get; set; }
        public string? Gui { get; set; }
        public string? _To { get; set; }
        public string? Port { get; set; }
        public string? After { get; set; }
        public string? KindOfCargo { get; set; }
        public string? Quantity { get; set; }
        public DateTime? NumCrew { get; set; }
        public DateTime? NumPassenger { get; set; }
        public string? LastPortCall { get; set; }
        public string? ActualDisplace { get; set; }
        public DateTime? EstimatedTime { get; set; }
        public string? PurposePort { get; set; }
        public DateTime? LastTimeArrival { get; set; }
        public string? PortOfArrival { get; set; }
        public string? PortArrivedFrom { get; set; }
        public string? DateArrival { get; set; }
        public string? MasterName { get; set; }
        public string? BreifParticular { get; set; }
        public string? QuantityCargo { get; set; }
        public string? QuantityDanger { get; set; }
        public string? PosPort { get; set; }
        public string? OtherConcer { get; set; }
        public string? Remarks { get; set; }
        public Guid? customerid { get; set; }
        public string? email { get; set; }
        public string? AttachmentFilesJson { get; set; }
        public string? AttachmentFilesDataJson { get; set; }
        public bool? editable { get; set; }
        public bool? continued { get; set; }
        public bool? approve { get; set; }
        public string? userid { get; set; }
        public DateTime? updatetime { get; set; }
    }
}
