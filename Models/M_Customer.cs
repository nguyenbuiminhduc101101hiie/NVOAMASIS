using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace NVOAMASIS.Models
{
    public class M_Customer
    {

        [Key]
        public Guid Customer_ID { get; set; }
        public string? MaDT { get; set; }
        public string? MainCode { get; set; }
        public string? Customer_Code { get; set; }
        public string? hancongno { get; set; }
        public double tilecom { get; set; }
        public string? district { get; set; }
        public string? VIPCode { get; set; }
        public string? TaxCode { get; set; }
        public string? EnglishName { get; set; }
        public string? COMPANY { get; set; }
        public string? BIZName { get; set; }
        public string? Address { get; set; }
        public string? Tel { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? Web { get; set; }
        public string? Province { get; set; }
        public string? Country { get; set; }
        public string? Industry { get; set; }
        public string? Remarks_Customer { get; set; }
        public string? Remarks_sale { get; set; } = "0";
        public string? AccountPotantial { get; set; }
        public string? SaleName { get; set; }
        public string? ATTN { get; set; }
        public string? QuyenHan { get; set; }
        public string? strUser { get; set; }
        public Guid? Class_ID { get; set; }
        public DateTime? ngaythem { get; set; }
        public bool? New {get;set;}
        public string? PIC { get; set; }
        public string? commodity { get; set; }
        public string? trafic { get; set; }
        public string? sea { get; set; }
        public string? air { get; set; }
        public string? imp { get; set; }
        public string? exp { get; set; }
        public string? contentofreport { get; set; }
        public bool? ApprovePayer { get; set; }
        public bool? Approve { get; set; }
        public bool? Continued { get; set; }
        public bool? Editable { get; set; }
        public string? UserID { get; set; }
        public DateTime? Updatetime { get; set; }
        public string? sotaikhoan { get; set; }
        public string? phuphicourier { get; set; }
        public string? buy { get; set; }
        public string? sell { get; set; }
        public bool? courier { get; set; }
        public bool? logisticts { get; set; }
        public bool? airfreight { get; set; }
        public bool? seafreight { get; set; }
        public bool? others { get; set; }
        public bool? rivaldhl { get; set; }
        public bool? rivaltNT { get; set; }
        public bool? rivalfedex { get; set; }
        public bool? rivalups { get; set; }
        public bool? rivalothers { get; set; }
        public bool? fieldgarment { get; set; }
        public bool? fieldtextile { get; set; }
        public bool? fieldshoes { get; set; }
        public bool? fieldfurniture { get; set; }
        public bool? fieldforwarder { get; set; }
        public bool? fieldbanking { get; set; }
        public bool? fieldothers { get; set; }
        public bool? nationalasia { get; set; }
        public bool? nationalEurope { get; set; }
        public bool? nationalNAmerica { get; set; }
        public bool? nationalAustralia { get; set; }
        public bool? nationalOthers { get; set; }
        public bool? destasia { get; set; }
        public bool? desteurope { get; set; }
        public bool? destnamerica { get; set; }
        public bool? destaustralia { get; set; }
        public bool? destothers { get; set; }
        public bool? styleForeign { get; set; }
        public bool? styleStateowned { get; set; }
        public string? addresstiengviet { get; set; }
        public string? branch { get; set; }
        public string? kpis { get; set; }
        public string? shortname { get; set; }
        public string? birthday { get; set; }
        public double? PriceDEM { get; set; }
        public double? PriceDET { get; set; }
        public string? Cur { get; set; }
        public bool? cothue { get; set; }
        public string? giatri { get; set; }
        public string? tientegiatri { get; set; }
        public string? city { get; set; }
        public string? Type { get; set; }
        public string? seaopcode { get; set; }
        public string? airopcode { get; set; }
        public bool? freetime { get; set; }
        public bool? combine_mau { get; set; }
        public string? Nationality { get; set; }
        public string? TaxInvoice_SGN { get; set; }
        public string? AirImport_SGN { get; set; }
        public string? SeaImport_SGN { get; set; }
        public string? TaxInvoice_HAN { get; set; }
        public string? AirImport_HAN { get; set; }
        public string? SeaImport_HAN { get; set; }
        public string? TaxInvoice_HPH { get; set; }
        public string? AirImport_HPH { get; set; }
        public string? SeaImport_HPH { get; set; }
        public string? contactID { get; set; }
        public string? location { get; set; }
        public string? category { get; set; }
        public string? custypeservice { get; set; }
        public string? accref { get; set; }
        public string? inputpeople { get; set; }
        public string? sale_Fast { get; set; }
        public string? username_fast { get; set; }
        public string? password_fast { get; set; }
        public string? accessdescription { get; set; }
        public string? DonViDoiTac_gs { get; set; }
        public string? TenNoiMoToKhai_gs { get; set; }
        public string? MaHangKhaiBaoHSCode_gs { get; set; }
        public string? TenHang_gs { get; set; }
        public string? tenNuocXuatXu_gs { get; set; }
        public string? dieuKienGiaoHang_gs { get; set; }
        public string? PhuongTienVanChuyen_gs { get; set; }
        public string? diachidonvidoitac_gs { get; set; }
        public string? tendiadiemnhanhangcuoicung_gs { get; set; }
        public string? tendiadiemxephang_gs { get; set; }
        public string? status { get; set; }
        public int? thoihanmove { get; set; } = 0;
        public M_Customer DeepCopy()
        {
            string json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<M_Customer>(json);
        }
    }
}
