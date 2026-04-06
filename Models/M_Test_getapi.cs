using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Test_getapi
    {
        [Key]
        public Guid Id { get; set; }
        public string? Time { get; set; }
        public string? Ten { get; set; }
        public string? Email { get; set; }
        public string? Sdt { get; set; }
    }
}
