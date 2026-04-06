using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("CamKetMuonCont_TraRong")]
    public class M_CamKetMuonCont_TraRong
    {
        [Key]
        public Guid id { get; set; }
        public Guid? hblid { get; set; }
        public string? Khorieng { get; set; }
        public string? Noitrarong { get; set; }
        public string? Note { get; set; }
        public string? No { get; set; }

    }
}
