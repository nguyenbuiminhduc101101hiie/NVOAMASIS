using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_GetQuoTationNo
    {
        [Key]
        public Guid id { get; set; }
        public string? thang { get; set; }
        public string? nam { get; set; }
        public int quotation_number { get; set; }
        public string? userget { get; set; }
        public bool approve { get; set; }
        public DateTime? timeget { get; set; }
    }
}
