using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{

    public class Container_InBound
    {
        [Key]
        public Guid InboundContainersID { get; set; }
        public Guid? inboundId { get; set; }
        public string? containerno { get; set; }
        public string? containertype { get; set; }
        public string? type { get; set; }
        public string? seal { get; set; }
        public string? sokien { get; set; }
        public string? sokg { get; set; }
        public string? sokhoi { get; set; }
        public string? chargeAble { get; set; }
        public bool? ContDayTaiCang { get; set; }
        public string? NgayCDTC { get; set; }
        public bool? ContDayDangTrenDuong { get; set; }
        public string? NgayCDDTD { get; set; }
        public bool? ContRongTaiBai { get; set; }
        public string? NgayCRTB { get; set; }
        public bool? ContDaTraVeDaiLy { get; set; }
        public string? NgayCDTVDL { get; set; }
        public bool? ContXuatFull { get; set; }
        public string? NgayCXF { get; set; }
        public string? nhietdo { get; set; }
        public string? thonggio { get; set; }
        public string? refExport { get; set; }
        public string? billExport { get; set; }
        public string? vesselExport { get; set; }
        public string? bairong { get; set; }
        public bool? check_ { get; set; }
        public string? ngaycddtd_agent { get; set; }
        public string? ngaycrtb_agent { get; set; }
        public bool? EmptyToShipper { get; set; }
        public string? NgayEmptyToShipper { get; set; }
        public bool? FullToAtQuay { get; set; }
        public string? NgayFullToAtQuay { get; set; }
        public bool? OnBoard { get; set; }
        public string? NgayOnboard { get; set; }
        public string? mblexport { get; set; }
        public string? hblexport { get; set; }
        public string? d_soUN { get; set; }
        public string? d_nhomhang { get; set; }
        public string? d_nhomphuso { get; set; }
        public string? d_diembocchay { get; set; }
        public string? d_onhiembien { get; set; }
        public string? d_vitrixephang { get; set; }
        public string? netweight { get; set; }
        public bool? IFD { get; set; }
        public string? ngayIFD { get; set; }
        public bool? DCO { get; set; }
        public string? ngayDCO { get; set; }
        public bool? EMM { get; set; }
        public string? ngayEMM { get; set; }
        public bool? DSO { get; set; }
        public string? ngayDSO { get; set; }
        public bool? OFO { get; set; }
        public string? ngayOFO { get; set; }
        public bool? OEO { get; set; }
        public string? ngayOEO { get; set; }
        public bool? BFF { get; set; }
        public string? ngayBFF { get; set; }
        public bool? AV { get; set; }
        public bool? DM { get; set; }
        public bool? RP { get; set; }
        public string? tinhtrang { get; set; }
        public string? danhapkho { get; set; }
        public string? daxuatkho { get; set; }
        public string? ngayan { get; set; }
        public string? ngaydo { get; set; }
        public string? Ghichu { get; set; }
        public Guid? ownerid { get; set; }
        public string? portarrival { get; set; }
        public bool? huhong { get; set; }
        public string? bkno { get; set; }
        public Guid? owneridold { get; set; }
        public string? ngaytinhluu { get; set; }
        public string? DATEUPDATE_CONTAINER { get; set; }
        public string? USERUPDATE_CONTAINER { get; set; }
        public string? seal2 { get; set; }
        public string? seal3 { get; set; }
        public string? descriptionContainer_ { get; set; }
        public string? descriptionContainer { get; set; }
        public string? Owner { get; set; }
        public bool? nguyhiem { get; set; }
    }
}
