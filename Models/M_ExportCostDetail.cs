using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_ExportCostDetail
    {
        [Key]
        public Guid id { get; set; }
        public Guid exportcostrequestid { get; set; }
        public Guid? loaichiphi { get; set; }
        public string? motachitiet { get; set; }
        public double? chiphivnd { get; set; }
        public double? chiphiusd { get; set; }
        public DateTime? ngaybaogia { get; set; }
        public string? userupdate { get; set; }
        public string? dateupdate { get; set; }
        public bool? continued { get; set; }
        public bool? editable { get; set; }
    }
}
