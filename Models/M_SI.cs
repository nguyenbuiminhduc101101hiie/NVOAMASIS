using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NVOAMASIS.Models
{
    [Table("SI")]
    public class M_SI
    {
        [Key]
        public Guid SIID { get; set; }
        public string? mbl { get; set; }
        public string? shipper { get; set; }
        public string? consignee { get; set; }
        public string? notify1 { get; set; }
        public string? vessel { get; set; }
        public string? voy { get; set; }
        public string? porname { get; set; }
        public string? porcode { get; set; }
        public string? polname { get; set; }
        public string? polcode { get; set; }
        public string? podname { get; set; }
        public string? podcode { get; set; }
        public string? delName { get; set; }
        public string? delcode { get; set; }
        public string? bkno { get; set; }
        public string? hbl { get; set; }
        public string? markAndNumbers { get; set; }
        public string? NoOfPackages { get; set; }
        public string? description { get; set; }
        public string? gross { get; set; }
        public string? cbm { get; set; }
        public string? shipOnboard { get; set; }
        public string? say { get; set; }
        public string? freightPayableAt { get; set; }
        public string? numberOfOriginal { get; set; }
        public string? placeAndDate { get; set; }
        public string? collectat { get; set; }
        public string? dateLaden { get; set; }
        public string? freightAmount { get; set; }
        public string? forDelivery { get; set; }
        public string? Air_type { get; set; }
        public string? Usercreate { get; set; }

        /// <summary>JSON mảng <see cref="SiAttachmentHistoryEntry"/> — lịch đính kèm/ghỡ file (tên file + user + thời điểm).</summary>
        public string? AttachmentHistoryJson { get; set; }

        /// <summary>Chỉ UI: danh sách đính kèm (không map DB). Nội dung file lưu bảng SI_Attachment.</summary>
        [NotMapped]
        [JsonIgnore]
        public List<M_SI_AttachmentRef> AttachmentRefs { get; set; } = new();
    }
}

