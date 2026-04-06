using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class CustomerReport
    {
        [Key]
        public Guid CusReport_ID { get; set; }
        public Guid? Customer_ID { get; set; }
        public string? Remarks { get; set; }
        public DateTime? Visitdate { get; set; }
        public bool? Daily { get; set; }
        public string? ValidUser { get; set; }
        public bool? editable { get; set; }
        public bool? continued { get; set; }
        public bool? approve { get; set; }
        public string? userid { get; set; }
        public DateTime? updatetime { get; set; }
        public string? KPI { get; set; }
        public string? thoigian { get; set; }
        public bool? hoanThanh { get; set; }
    }
}
