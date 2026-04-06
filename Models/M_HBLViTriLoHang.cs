using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_HBLViTriLoHang
    {
        [Key]
        public Guid ID { get; set; }
        public Guid HBLID { get; set; }
        public string HBLNo { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime ngay { get; set; }
        public DateTime Timestamp { get; set; }
        public double? Accuracy { get; set; }
        public double? Altitude { get; set; }
        public double? Speed { get; set; }
        public double? Heading { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
