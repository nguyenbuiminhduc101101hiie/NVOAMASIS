using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_TruckingCostRequest
    {
        [Key]
        public Guid Id { get; set; }
        public string? MaRFQ { get; set; }
        public DateTime? NgayYeuCau { get; set; }
        public string? Nguoiyeucau { get; set; }
        public Guid? CustomerId { get; set; }
        public string? Loaihang { get; set; }
        public double? Soluong { get; set; }
        public string? Donvitinh { get; set; }
        public string? Kichthuochang { get; set; }
        public string? Loaixe { get; set; }
        public string? Noilayhang { get; set; }
        public string? Noigiaohang { get; set; }
        public DateTime? Ngaylayhang { get; set; }
        public string? Ghichu { get; set; }
        public string? Trangthai { get; set; }
        public string? Userupdate { get; set; }
        public string? Dateupdate { get; set; }

        public string? Createat { get; set; }
        public string? Usercreate { get; set; }
        public bool? Approve { get; set; } = false;
        public bool? Continued { get; set; } = true;
        public bool? Editable { get; set; } = true;
        public string? Userhandle {  get; set; }

    }
}
