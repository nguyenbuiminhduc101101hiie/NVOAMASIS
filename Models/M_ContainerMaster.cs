using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace NVOAMASIS.Models
{
    public class M_ContainerMaster
    {
        [Key]
        public Guid ID { get; set; }

        /// <summary>Số container, ví dụ TGHU1234567. Chọn từ M_Container hoặc tự nhập tay.</summary>
        [Required]
        [StringLength(20)]
        public string? ContainerNo { get; set; }

        /// <summary>20GP, 40GP, 40HC, Reefer...</summary>
        [StringLength(20)]
        public string? ContainerType { get; set; }

        /// <summary>SOC / COC / Leased</summary>
        [StringLength(20)]
        public string? OwnershipType { get; set; }

        /// <summary>Chủ container, lưu theo M_Customer.Customer_ID.</summary>
        public Guid? OwnerID { get; set; }

        /// <summary>Available / Booked / Laden / Empty / Repair</summary>
        [StringLength(20)]
        public string? Status { get; set; }

        [StringLength(200)]
        public string? CurrentLocation { get; set; }

        [StringLength(200)]
        public string? CurrentDepot { get; set; }

        public DateTime? ManufactureDate { get; set; }

        public DateTime? LastInspectionDate { get; set; }

        /// <summary>Good / Damaged / Repair</summary>
        [StringLength(20)]
        public string? Condition { get; set; }

        public M_ContainerMaster DeepCopy()
        {
            string json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<M_ContainerMaster>(json);
        }
    }
}
