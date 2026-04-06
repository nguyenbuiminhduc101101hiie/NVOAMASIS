using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class DanhMucTaiKhoan
    {
        [Key]
        public Guid Id { get; set; }
        public string? Taikhoan { get; set; }
        public string? Tentaikhoan { get; set; }
        public string? Accountname { get; set; }
        public string? Manguyente { get; set; }
        public bool? Approve { get; set; }
        public string? Userupdate { get; set; }
        public string? Dateupdate { get; set; }
        public bool? Tinhdauky_Cuoiky { get; set; }
        public string? AccountType { get; set; }
        public bool? IsPosting { get; set; } = true;
        public bool? RequiresCustomer { get; set; } =false;
        public bool? RequiresSupplier { get; set; } = false;
        public bool? RequiresEmployee { get; set; } = false;
        public bool? RequiresShipment { get; set; } = false;
        public bool? IsActive { get; set; } = true;
        public string? Diengiai { get; set; }

        
    }
}
