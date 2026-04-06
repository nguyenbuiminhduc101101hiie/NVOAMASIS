namespace NVOAMASIS.Models
{
    public class M_TKHQ_Thongtinchunglohang
    {
        public Guid Id { get; set; }
        public string? Mst_Xk { get; set; }
        public string? Company_Xk { get; set; }
        public string? Address_Xk { get; set; }
        public string? Mst_Nk { get; set; }
        public string? Company_Nk { get; set; }
        public string? Address_Nk { get; set; }
        public string? Sotokhai { get; set; }
        public DateTime? Ngaykhaibao { get; set; }
        public string? Noikhaibao { get; set; }
        public string? Phuongthucvanchuyen { get; set; }
        public string? Phuongtienvanchuyen { get; set; }
        public string? Noixephang { get; set; }
        public string? Noidohang { get; set; }
        public string? Sohoadon { get; set; }
        public DateTime? Ngaylaphoadon { get; set; }
        public double? Sotien { get; set; }
        public string? Dieukiengiaohang { get; set; }
        public string? Orther { get; set; }
        public DateTime? dateupdate { get; set; }
        public string? userupdate { get; set; }
        public string? nguoiyeucau { get; set; }
        public string? nguoitraloi { get; set; }
        public string? Links { get; set; }
        public string? TieuDeEmail { get; set; }
        public string? NoidungEmail { get; set; }
        public string? NoidungChitietEmail { get; set; }
        public string? Trangthai { get; set; }

        public bool? Approve { get; set; } = false;
    }
}
