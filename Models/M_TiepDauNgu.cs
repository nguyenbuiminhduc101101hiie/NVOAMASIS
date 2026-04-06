using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_TiepDauNgu
    {
        [Key]
        public Guid id { get; set; }
        public string? Loai { get; set; }
        public string? Hangso { get; set; }
        public string? POL { get; set; }
        public string? POD { get; set; }
    }
}
