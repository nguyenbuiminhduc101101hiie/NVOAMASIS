using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("LCC_POL_Vietnam")]
    public class M_LccPolVietnam
    {
        public Guid Id { get; set; }

        /// <summary>Hãng tàu / line, vd: WAN HAI, COSCO, MSC...</summary>
        [MaxLength(150)]
        public string? Carrier { get; set; }

        /// <summary>Loại phí, vd: THC DRY, SEAL, B/L fee, TELEX...</summary>
        [MaxLength(250)]
        public string? ChargeType { get; set; }

        [MaxLength(300)]
        public string? Container20DC { get; set; }
        [MaxLength(300)]
        public string? Container40HC { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }
        [MaxLength(200)]
        public string? Userupdate { get; set; }
        [MaxLength(50)]
        public string? Dateupdate { get; set; }
        public DateTime? CreatedDate { get; set; }

        /// <summary>Tên file Excel đã import ra dòng này (rỗng nếu dòng được thêm/sửa thủ công qua CRUD).</summary>
        [MaxLength(260)]
        public string? SourceFileName { get; set; }
    }
}
