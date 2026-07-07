using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace NVOAMASIS.Models
{
    public class M_Container
    {
        [Key]
        public Guid CTN_ID { get; set; }
        public Guid? mblid { get; set; }
        public Guid? hblid { get; set; }
        public string? CTN_SIZE_TYPE { get; set; }
        public string? CONTAINER_NO { get; set; }
        public string? Seal { get; set; }
        public double? pkgs { get; set; }
        public string? pkgsCode { get; set; }
        public string? cbm { get; set; }
        public double? NETWEIGHT { get; set; }
        public double? GrossWeight { get; set; }
        public string? description { get; set; }
        public bool? APPROVE { get; set; } = false;
        public bool? CONTINUED { get; set; } = true;
        public bool? EDITABLE { get; set; } = true;
        public string? USERID { get; set; }
        public DateTime? UPDATETIME { get; set; }
        public string? loaihang { get; set; }

        [StringLength(4)]
        public string? IsoCode { get; set; }

        [StringLength(10)]
        public string? OwnerType { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        [StringLength(20)]
        public string? Condition { get; set; }

        public bool? IsActive { get; set; }

        public decimal? TareWeight { get; set; }

        public decimal? Payload { get; set; }

        public decimal? MaxGrossWeight { get; set; }

        public decimal? LoadedWeight { get; set; }

        public decimal? CBMCapacity { get; set; }

        public bool? IsReefer { get; set; }

        public decimal? Temperature { get; set; }

        [StringLength(20)]
        public string? VentSetting { get; set; }

        public DateTime? PTIDate { get; set; }
        public string? Status_Cont {  get; set; }

        public string? In_Depot { get; set; }
        public string? In_Port { get; set; }
        public string? NVOCC_Status { get; set; }

        public bool? Decommision { get; set; } = false;
        public string? OwnerName { get; set; }
        public Guid? CamketmuonCont_id { get; set; }
        public DateTime? Empty_pickup_date { get; set; }
        public DateTime? Full_Discharge_Date { get; set; }
        public DateTime? Full_Delivery_Date { get; set; }
        public DateTime? Empty_Return_Date { get; set; }
        public DateTime? Storage_In_Date { get; set; }
        public DateTime? Storage_Out_Date { get; set; }
        public M_Container DeepCopy()
        {
            string json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<M_Container>(json);
        }
    }
}
