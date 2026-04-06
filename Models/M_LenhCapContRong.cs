using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_LenhCapContRong
    {
        [Key]
        public Guid id { get; set; }
        public Guid bookingid { get; set; }
        public string? OrderNo { get; set; }
        public DateTime? HieuLucLenh { get; set; }
        public string? ContType { get; set; }

        public int Quantity { get; set; }
        public string? Saycont { get; set; }
        public string? HaContTai { get; set; }
        public string? TrongLuongCont { get; set; }
        public DateTime? ExportDate { get; set; }
        public string? UserExport { get; set; }
    }
}
