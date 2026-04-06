using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_CustomerBr
    {
        [Key]
        public Guid customerBrID { get; set; }
        public Guid? customerid { get; set; }
        public string? citybr { get; set; }
        public string? namecitybr { get; set; }
        public string? typebr { get; set; }
        public string? telbr { get; set; }
        public string? faxbr { get; set; }
        public string? addressbr { get; set; }
        public bool? approve { get; set; }
        public string? userupdate { get; set; }
        public string? dateupdate { get; set; }
        public bool? editable { get; set; }
        public bool? continued { get; set; }
    }
}
