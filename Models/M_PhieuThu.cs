using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_PhieuThu
    {
        [Key]
        public Guid PhieuthuID { get; set; }
        public Guid? Customer_Id { get; set; }
        public Guid? TaxInvoiceID { get; set; }
        public string? SoPhieuthu { get; set; }
        public string? TKNo { get; set; }
        public string? TKCo { get; set; }
        public DateTime? Ngay { get; set; }
        public DateTime? Ngayhachtoan { get; set; }
        public string? PTTT { get; set; }
        public string? Noidung { get; set; }
        public double? Sotien { get; set; }
        public double? Tigia { get; set; }
        public string? Currency { get; set; }
        public string? Chungtu { get; set; }
        public string? Nguoinoptien { get; set; }
        public string? BillNo { get; set; }
        public string? Bank { get; set; }
        public bool? Continued { get; set; }
        public bool? Approve { get; set; }
        public bool? Editable { get; set; }
        public string? UserUpdate { get; set; }
        public string? DateUpdate { get; set; }
        public int? STT { get; set; }
        public string? Branch { get; set; }
        public string? Loaiphieu { get; set; }
        public string? Socont { get; set; }
        public string? Travotai { get; set; }
        public string? Code { get; set; }
        public bool? Ruthangtaibai { get; set; }
        public string? Soluongcont { get; set; }
        public string? Hanlenhharong { get; set; }
        public string? Quyenso { get; set; }
        public string? Sohoadon { get; set; }
        public string? Soseri { get; set; }
        public string? Trangthai { get; set; }
        public string? Mbl { get; set; }
        public string? Taikhoan_Doiung { get; set; }
        public bool? Sodudauky { get; set; }
        public string? Lastcargo { get; set; }
        public string? Depoaddress { get; set; }
        public string? Customername { get; set; }
        public string? Luuycuoc { get; set; }
        public string? Remarks { get; set; }

        /// <summary>Token dùng chung cho link duyệt trên email (1 phiếu = 1 token).</summary>
        public string? ApproveToken { get; set; }
        public string? ApproveBy { get; set; }
        public Guid? ApproveByUserId { get; set; }
        public DateTime? ApproveDate { get; set; }
    }
}
