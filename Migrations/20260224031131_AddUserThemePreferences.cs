using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <inheritdoc />
    public partial class AddUserThemePreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agency",
                columns: table => new
                {
                    Agency_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HangdingFee = table.Column<double>(type: "float", nullable: true),
                    AgencyRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pic_ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    skype = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    yahoo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agency", x => x.Agency_ID);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsOnsite = table.Column<bool>(type: "bit", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LocalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Session = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BieugiaKDTV",
                columns: table => new
                {
                    KDTVID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BieuGiaKDTVNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayApDung = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChiCucKiemDich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChiChiCuc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiHinhKiemDich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CONT20_1 = table.Column<double>(type: "float", nullable: true),
                    CONT20_2 = table.Column<double>(type: "float", nullable: true),
                    CONT20_3 = table.Column<double>(type: "float", nullable: true),
                    CONT20_4 = table.Column<double>(type: "float", nullable: true),
                    CONT20_5 = table.Column<double>(type: "float", nullable: true),
                    CONT40_1 = table.Column<double>(type: "float", nullable: true),
                    CONT40_2 = table.Column<double>(type: "float", nullable: true),
                    CONT40_3 = table.Column<double>(type: "float", nullable: true),
                    CONT40_4 = table.Column<double>(type: "float", nullable: true),
                    CONT40_5 = table.Column<double>(type: "float", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BieugiaKDTV", x => x.KDTVID);
                });

            migrationBuilder.CreateTable(
                name: "BieugiaKTCL",
                columns: table => new
                {
                    BieuGiaKTCLID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayHieuLuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DichVuKiemTra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoQuanThucHien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiHoaDon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDungDichVu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiMucPhi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoLuong = table.Column<double>(type: "float", nullable: true),
                    DonGia = table.Column<double>(type: "float", nullable: true),
                    DonViTinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanhTien = table.Column<double>(type: "float", nullable: true),
                    TienTe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    BieuGiaKTCLNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BieugiaKTCL", x => x.BieuGiaKTCLID);
                });

            migrationBuilder.CreateTable(
                name: "Bieugialuukho",
                columns: table => new
                {
                    BieuGiaLuuKhoID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VenderID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NgayHieuLuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Bieugialuukhono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bieugialuukho", x => x.BieuGiaLuuKhoID);
                });

            migrationBuilder.CreateTable(
                name: "BieuGiaNangHa",
                columns: table => new
                {
                    BieuGiaNangHaID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CangICDDepot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiContainer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiHinhNangHa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianApDung = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NangLenPhuongTien = table.Column<double>(type: "float", nullable: true),
                    HaXuongBai = table.Column<double>(type: "float", nullable: true),
                    NangHaNoiBo = table.Column<double>(type: "float", nullable: true),
                    NangHaRong = table.Column<double>(type: "float", nullable: true),
                    NangHaHang = table.Column<double>(type: "float", nullable: true),
                    NangHaKiemHoaSuaChuaVeSinh = table.Column<double>(type: "float", nullable: true),
                    NangHaKiemHoa = table.Column<double>(type: "float", nullable: true),
                    NangHaSieuTruongSieuTrong = table.Column<double>(type: "float", nullable: true),
                    NangHaNgoaiGioHanhChinh = table.Column<double>(type: "float", nullable: true),
                    NangHaKhuCachLy = table.Column<double>(type: "float", nullable: true),
                    PhuPhiHangNguyHiem = table.Column<double>(type: "float", nullable: true),
                    PhuPhiNgoaiGio = table.Column<double>(type: "float", nullable: true),
                    PhuPhiDungThietBiDacBiet = table.Column<double>(type: "float", nullable: true),
                    Loaitiente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BieugiananghaNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BieuGiaNangHa", x => x.BieuGiaNangHaID);
                });

            migrationBuilder.CreateTable(
                name: "Bieugiarutruot",
                columns: table => new
                {
                    RutRuotID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NgayHieuLuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CangICDDepot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RutRuotTaiBaiCY = table.Column<double>(type: "float", nullable: true),
                    RutRuoctTaiKhoCFS = table.Column<double>(type: "float", nullable: true),
                    RutRuocKiemHoa = table.Column<double>(type: "float", nullable: true),
                    PhiNangContainerTuDauKeoLenBaiRutHang = table.Column<double>(type: "float", nullable: true),
                    PhiNangRongLenXeTraContainerRong = table.Column<double>(type: "float", nullable: true),
                    PhiLuuContainer = table.Column<double>(type: "float", nullable: true),
                    PhiLuuBaiHang = table.Column<double>(type: "float", nullable: true),
                    PhiThueKhoCFSKiemHoa = table.Column<double>(type: "float", nullable: true),
                    PhiNhanCongKiemHoa = table.Column<double>(type: "float", nullable: true),
                    PhiNangContainerKiemHoa = table.Column<double>(type: "float", nullable: true),
                    PhiVeSinhContainer = table.Column<double>(type: "float", nullable: true),
                    PhiHaTangCangBien = table.Column<double>(type: "float", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BieugiarutruotNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TienTe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bieugiarutruot", x => x.RutRuotID);
                });

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    Booking_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Customer_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    POD_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    POL_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Agency_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SL20GPOwner = table.Column<double>(type: "float", nullable: true),
                    SL40GPOwner = table.Column<double>(type: "float", nullable: true),
                    SL40HCOwner = table.Column<double>(type: "float", nullable: true),
                    SL45HCOwner = table.Column<double>(type: "float", nullable: true),
                    SL20RFOwner = table.Column<double>(type: "float", nullable: true),
                    SL40RFOwner = table.Column<double>(type: "float", nullable: true),
                    SL40RHOwner = table.Column<double>(type: "float", nullable: true),
                    SL20OTOwner = table.Column<double>(type: "float", nullable: true),
                    SL40OTOwner = table.Column<double>(type: "float", nullable: true),
                    Sl20FROwner = table.Column<double>(type: "float", nullable: true),
                    SL40FROwner = table.Column<double>(type: "float", nullable: true),
                    SLCBMOwner = table.Column<double>(type: "float", nullable: true),
                    SL20GPSale = table.Column<double>(type: "float", nullable: true),
                    SL40GPSale = table.Column<double>(type: "float", nullable: true),
                    SL40HCSale = table.Column<double>(type: "float", nullable: true),
                    SL45HCSale = table.Column<double>(type: "float", nullable: true),
                    SL20RFSale = table.Column<double>(type: "float", nullable: true),
                    SL40RFSale = table.Column<double>(type: "float", nullable: true),
                    SL40RHSale = table.Column<double>(type: "float", nullable: true),
                    SL20OTSale = table.Column<double>(type: "float", nullable: true),
                    SL40OTSale = table.Column<double>(type: "float", nullable: true),
                    SL20FRSale = table.Column<double>(type: "float", nullable: true),
                    SL40FRSale = table.Column<double>(type: "float", nullable: true),
                    SLCBMSale = table.Column<double>(type: "float", nullable: true),
                    Validate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Commission = table.Column<double>(type: "float", nullable: true),
                    SCNO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    userid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updatetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    buy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sell = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking", x => x.Booking_ID);
                });

            migrationBuilder.CreateTable(
                name: "Buyer",
                columns: table => new
                {
                    BuyerID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Customer_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Buyer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PIC1Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PIC1Pos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PIC1Tel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PIC1Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Commodity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    userid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updatetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    consigneeshipper = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buyer", x => x.BuyerID);
                });

            migrationBuilder.CreateTable(
                name: "Charge",
                columns: table => new
                {
                    CHARGE_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    stt = table.Column<int>(type: "int", nullable: true),
                    CHARGE_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Show = table.Column<bool>(type: "bit", nullable: true),
                    show1 = table.Column<bool>(type: "bit", nullable: true),
                    CHARGE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DVT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tariffFCL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tarifffclib = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tarifflcl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tarifflclib = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    APPROVE = table.Column<bool>(type: "bit", nullable: true),
                    CONTINUED = table.Column<bool>(type: "bit", nullable: true),
                    EDITABLE = table.Column<bool>(type: "bit", nullable: true),
                    USERID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UPDATETIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    tiengtrung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    other = table.Column<bool>(type: "bit", nullable: true),
                    tk1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tk2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tk3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loaidichvu_charge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    accselling = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    accbuying = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charge", x => x.CHARGE_ID);
                });

            migrationBuilder.CreateTable(
                name: "ChargeTemplete",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CHARGEID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cont20 = table.Column<double>(type: "float", nullable: true),
                    Cont40 = table.Column<double>(type: "float", nullable: true),
                    Incvat = table.Column<double>(type: "float", nullable: true),
                    Cur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ncc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    APPROVE = table.Column<bool>(type: "bit", nullable: true),
                    CONTINUED = table.Column<bool>(type: "bit", nullable: true),
                    EDITABLE = table.Column<bool>(type: "bit", nullable: true),
                    USERupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargeTemplete", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "chiTietBieuGiaLuuKho",
                columns: table => new
                {
                    ChiTietBieuGiaLuuKhoID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BieuGiaLuuKhoID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DienGiai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiContainerHangHoa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DonViTinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaTien = table.Column<double>(type: "float", nullable: true),
                    TienTe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChuHangHoa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    So = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chiTietBieuGiaLuuKho", x => x.ChiTietBieuGiaLuuKhoID);
                });

            migrationBuilder.CreateTable(
                name: "Commondity",
                columns: table => new
                {
                    Commondity_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Commondity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EDI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Updatetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commondity", x => x.Commondity_ID);
                });

            migrationBuilder.CreateTable(
                name: "CompanyInfomation",
                columns: table => new
                {
                    CompanyID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameTiengViet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressTiengViet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SwiftCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddDebit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Accno1Debit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Accno2Debit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanmovecus = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyInfomation", x => x.CompanyID);
                });

            migrationBuilder.CreateTable(
                name: "CongNoHoaDonDauRa",
                columns: table => new
                {
                    congnoHoadonDauraid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hoadondauraid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tongtien = table.Column<double>(type: "float", nullable: true),
                    ngaytra = table.Column<DateTime>(type: "datetime2", nullable: true),
                    tiente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    tigia = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongNoHoaDonDauRa", x => x.congnoHoadonDauraid);
                });

            migrationBuilder.CreateTable(
                name: "CongNoHoaDonDauVao",
                columns: table => new
                {
                    congnoHoadonDauvaoid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hoadondauvaoid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tongtien = table.Column<double>(type: "float", nullable: true),
                    tigia = table.Column<double>(type: "float", nullable: true),
                    ngaytra = table.Column<DateTime>(type: "datetime2", nullable: true),
                    tiente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongNoHoaDonDauVao", x => x.congnoHoadonDauvaoid);
                });

            migrationBuilder.CreateTable(
                name: "Container",
                columns: table => new
                {
                    CTN_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    mblid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    hblid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CTN_SIZE_TYPE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CONTAINER_NO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Seal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pkgs = table.Column<double>(type: "float", nullable: true),
                    pkgsCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cbm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NETWEIGHT = table.Column<double>(type: "float", nullable: true),
                    GrossWeight = table.Column<double>(type: "float", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    APPROVE = table.Column<bool>(type: "bit", nullable: true),
                    CONTINUED = table.Column<bool>(type: "bit", nullable: true),
                    EDITABLE = table.Column<bool>(type: "bit", nullable: true),
                    USERID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UPDATETIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    loaihang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsoCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    OwnerType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Condition = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    TareWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Payload = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxGrossWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LoadedWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CBMCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsReefer = table.Column<bool>(type: "bit", nullable: true),
                    Temperature = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VentSetting = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PTIDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Container", x => x.CTN_ID);
                });

            migrationBuilder.CreateTable(
                name: "ContainerDamage",
                columns: table => new
                {
                    DamageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CTN_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DamageType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DamageArea = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DamageDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DetectedAt = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DetectedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsibleParty = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ClaimStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RepairCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerDamage", x => x.DamageId);
                });

            migrationBuilder.CreateTable(
                name: "ContainerMovement",
                columns: table => new
                {
                    MovementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CTN_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GateInDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GateOutDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MovementStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MovementType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DepotId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PortCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    VesselName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VoyageNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerMovement", x => x.MovementId);
                });

            migrationBuilder.CreateTable(
                name: "CONTAINEROUTBOUNDNOTIFY_sale",
                columns: table => new
                {
                    ContainerOutBoundNotifyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quotationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Fileno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bkcarrier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookingNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Customer_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ATTN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Market_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vessel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoyNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ETD = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ETA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sale_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SaleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaleCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_from = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    hano_Closing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_Closingdelivery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoFCL_Quality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_delivery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_Commodity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_WeightCBMPKG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_POR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_PORCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_POL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_POLCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_POD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_PODCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_POT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_POTCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hano_Payment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoLCL_Consolidator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoLCL_CFSWH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoLCL_Add = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoLCL_VNACCSCODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoLCL_WHPIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoLCL_BookingPIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAir_AIRlines = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAir_AirportofDeparture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAir_AirportofDestination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAir_ShipperConsignee = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAir_Descriptionofgoods = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAir_Freightrateterm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAIr_MAWBHAWBNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAir_RoutingVoyage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAIr_Flighttime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAir_Cutoffdatetime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAir_Deliverycontactat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAir_Morning = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAir_Afternoon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoAir_Placeofreceipt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarksFCL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarksLCL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarksAir = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FCL_LCL_Air = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    iol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    serviceContract = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Emptydepot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmpContReDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnLadenPlace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OPSCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceOfStuffing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecialCont = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullReturnDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SICutOff = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DC20 = table.Column<int>(type: "int", nullable: true),
                    HC40 = table.Column<int>(type: "int", nullable: true),
                    RF20 = table.Column<int>(type: "int", nullable: true),
                    RF40 = table.Column<int>(type: "int", nullable: true),
                    PickupAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DropOff = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Lo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cncust = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    agencyID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    dateRelease = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    datecancel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoadingStuffingDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BorderOfDelivery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceOfDelivery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contact_Stuffing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    routingInformation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    kien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    kgS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    khoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soxe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    THOIGIANNHANHANG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    diadiemnhanhang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nguoilienhenhanhang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    THOIGIANgiaoHANG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    diadiemgiaohang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nguoilienhegiaohang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    chungtudinhkem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    yeucaudacbiet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soxetai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    congviecthuchien1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    congviecthuchien2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    congviecthuchien3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    congviecthuchien4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    congviecthuchien5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoigian1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoigian2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoigian3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoigian4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoigian5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    timecargo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    timevgm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    timesi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tendest = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    codedest = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cont_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    agencyname_box = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    etatime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    etdtime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoair_cutofdaytimetime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hanoair_cutoffdaytimetime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loaikien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PRICINGMAN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ANETA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BILLTYPE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CONSIGNEEID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    containerno_bk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status_bk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tk1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tk2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tk3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tk4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tk5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaytk1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaytk2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaytk3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaytk4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaytk5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soinv1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soinv2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soinv3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soinv4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soinv5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaincusBk_Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PricingRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TPC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaincusBk_Id_ = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    shipperfreetext = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationCode_ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaytimeline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    giotimeline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    userstimeline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    noidungtimeline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    daguitimeline = table.Column<bool>(type: "bit", nullable: true),
                    DC40 = table.Column<int>(type: "int", nullable: true),
                    RH40 = table.Column<int>(type: "int", nullable: true),
                    HC45 = table.Column<int>(type: "int", nullable: true),
                    OT20 = table.Column<int>(type: "int", nullable: true),
                    OT40 = table.Column<int>(type: "int", nullable: true),
                    FR20 = table.Column<int>(type: "int", nullable: true),
                    FR40 = table.Column<int>(type: "int", nullable: true),
                    Ref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contnum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    stuffingdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks_ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    emailtaixe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    chksea = table.Column<bool>(type: "bit", nullable: false),
                    chkair = table.Column<bool>(type: "bit", nullable: false),
                    chkcustoms = table.Column<bool>(type: "bit", nullable: false),
                    chktrucking = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONTAINEROUTBOUNDNOTIFY_sale", x => x.ContainerOutBoundNotifyId);
                });

            migrationBuilder.CreateTable(
                name: "ContainerRepair",
                columns: table => new
                {
                    InboundContainersID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    inboundId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    containerno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    containertype = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sokien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sokg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sokhoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    chargeAble = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContDayTaiCang = table.Column<bool>(type: "bit", nullable: true),
                    NgayCDTC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContDayDangTrenDuong = table.Column<bool>(type: "bit", nullable: true),
                    NgayCDDTD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContRongTaiBai = table.Column<bool>(type: "bit", nullable: true),
                    NgayCRTB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContDaTraVeDaiLy = table.Column<bool>(type: "bit", nullable: true),
                    NgayCDTVDL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContXuatFull = table.Column<bool>(type: "bit", nullable: true),
                    NgayCXF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nhietdo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thonggio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    refExport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    billExport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vesselExport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bairong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    check_ = table.Column<bool>(type: "bit", nullable: true),
                    ngaycddtd_agent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaycrtb_agent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmptyToShipper = table.Column<bool>(type: "bit", nullable: true),
                    NgayEmptyToShipper = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullToAtQuay = table.Column<bool>(type: "bit", nullable: true),
                    NgayFullToAtQuay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OnBoard = table.Column<bool>(type: "bit", nullable: true),
                    NgayOnboard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mblexport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hblexport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    d_soUN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    d_nhomhang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    d_nhomphuso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    d_diembocchay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    d_onhiembien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    d_vitrixephang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    netweight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IFD = table.Column<bool>(type: "bit", nullable: true),
                    ngayIFD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DCO = table.Column<bool>(type: "bit", nullable: true),
                    ngayDCO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMM = table.Column<bool>(type: "bit", nullable: true),
                    ngayEMM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DSO = table.Column<bool>(type: "bit", nullable: true),
                    ngayDSO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OFO = table.Column<bool>(type: "bit", nullable: true),
                    ngayOFO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OEO = table.Column<bool>(type: "bit", nullable: true),
                    ngayOEO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BFF = table.Column<bool>(type: "bit", nullable: true),
                    ngayBFF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AV = table.Column<bool>(type: "bit", nullable: true),
                    DM = table.Column<bool>(type: "bit", nullable: true),
                    RP = table.Column<bool>(type: "bit", nullable: true),
                    tinhtrang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    danhapkho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    daxuatkho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaydo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ghichu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ownerid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    portarrival = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    huhong = table.Column<bool>(type: "bit", nullable: true),
                    bkno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    owneridold = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngaytinhluu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DATEUPDATE_CONTAINER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    USERUPDATE_CONTAINER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seal2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seal3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    descriptionContainer_ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    descriptionContainer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Owner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nguyhiem = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerRepair", x => x.InboundContainersID);
                });

            migrationBuilder.CreateTable(
                name: "ContainerSeal",
                columns: table => new
                {
                    SealId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CTN_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SealNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SealType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SealMaterial = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AppliedAt = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AppliedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerSeal", x => x.SealId);
                });

            migrationBuilder.CreateTable(
                name: "Credit",
                columns: table => new
                {
                    creditid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    mblid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hblid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quotationid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customerid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    itemid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    dongia = table.Column<double>(type: "float", nullable: true),
                    soluong = table.Column<double>(type: "float", nullable: true),
                    thanhtien = table.Column<double>(type: "float", nullable: true),
                    tiente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thue = table.Column<double>(type: "float", nullable: true),
                    thanhtiensauthue = table.Column<double>(type: "float", nullable: true),
                    chiho = table.Column<bool>(type: "bit", nullable: true),
                    tigiacredit = table.Column<double>(type: "float", nullable: true),
                    tigiahoadondauvao = table.Column<double>(type: "float", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DNTT = table.Column<bool>(type: "bit", nullable: true),
                    DNTU = table.Column<bool>(type: "bit", nullable: true),
                    Copied = table.Column<bool>(type: "bit", nullable: true),
                    sodntt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ghichu_credit = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credit", x => x.creditid);
                });

            migrationBuilder.CreateTable(
                name: "CuocCont",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hblid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    itemid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customerid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    No = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    unitprice = table.Column<double>(type: "float", nullable: true),
                    quantity = table.Column<double>(type: "float", nullable: true),
                    total = table.Column<double>(type: "float", nullable: true),
                    cur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    exc = table.Column<double>(type: "float", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    Vat = table.Column<double>(type: "float", nullable: true),
                    thanhtiensauthue = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuocCont", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                columns: table => new
                {
                    CurrencyID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exchange = table.Column<double>(type: "float", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.CurrencyID);
                });

            migrationBuilder.CreateTable(
                name: "CustomCostRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaRFQ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    NgayYeuCau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserCreate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loaihang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loaihinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noidung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Userhandle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomCostRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Customer_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaDT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MainCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Customer_Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hancongno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tilecom = table.Column<double>(type: "float", nullable: false),
                    district = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VIPCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    COMPANY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BIZName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Web = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Province = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks_Customer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks_sale = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountPotantial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ATTN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuyenHan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    strUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Class_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngaythem = table.Column<DateTime>(type: "datetime2", nullable: true),
                    New = table.Column<bool>(type: "bit", nullable: true),
                    PIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    commodity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trafic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    air = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    imp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    exp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contentofreport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovePayer = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Updatetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    sotaikhoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phuphicourier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    buy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sell = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    courier = table.Column<bool>(type: "bit", nullable: true),
                    logisticts = table.Column<bool>(type: "bit", nullable: true),
                    airfreight = table.Column<bool>(type: "bit", nullable: true),
                    seafreight = table.Column<bool>(type: "bit", nullable: true),
                    others = table.Column<bool>(type: "bit", nullable: true),
                    rivaldhl = table.Column<bool>(type: "bit", nullable: true),
                    rivaltNT = table.Column<bool>(type: "bit", nullable: true),
                    rivalfedex = table.Column<bool>(type: "bit", nullable: true),
                    rivalups = table.Column<bool>(type: "bit", nullable: true),
                    rivalothers = table.Column<bool>(type: "bit", nullable: true),
                    fieldgarment = table.Column<bool>(type: "bit", nullable: true),
                    fieldtextile = table.Column<bool>(type: "bit", nullable: true),
                    fieldshoes = table.Column<bool>(type: "bit", nullable: true),
                    fieldfurniture = table.Column<bool>(type: "bit", nullable: true),
                    fieldforwarder = table.Column<bool>(type: "bit", nullable: true),
                    fieldbanking = table.Column<bool>(type: "bit", nullable: true),
                    fieldothers = table.Column<bool>(type: "bit", nullable: true),
                    nationalasia = table.Column<bool>(type: "bit", nullable: true),
                    nationalEurope = table.Column<bool>(type: "bit", nullable: true),
                    nationalNAmerica = table.Column<bool>(type: "bit", nullable: true),
                    nationalAustralia = table.Column<bool>(type: "bit", nullable: true),
                    nationalOthers = table.Column<bool>(type: "bit", nullable: true),
                    destasia = table.Column<bool>(type: "bit", nullable: true),
                    desteurope = table.Column<bool>(type: "bit", nullable: true),
                    destnamerica = table.Column<bool>(type: "bit", nullable: true),
                    destaustralia = table.Column<bool>(type: "bit", nullable: true),
                    destothers = table.Column<bool>(type: "bit", nullable: true),
                    styleForeign = table.Column<bool>(type: "bit", nullable: true),
                    styleStateowned = table.Column<bool>(type: "bit", nullable: true),
                    addresstiengviet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    kpis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    shortname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    birthday = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriceDEM = table.Column<double>(type: "float", nullable: true),
                    PriceDET = table.Column<double>(type: "float", nullable: true),
                    Cur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cothue = table.Column<bool>(type: "bit", nullable: true),
                    giatri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tientegiatri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seaopcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    airopcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    freetime = table.Column<bool>(type: "bit", nullable: true),
                    combine_mau = table.Column<bool>(type: "bit", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxInvoice_SGN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AirImport_SGN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeaImport_SGN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxInvoice_HAN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AirImport_HAN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeaImport_HAN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxInvoice_HPH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AirImport_HPH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeaImport_HPH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contactID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    custypeservice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    accref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    inputpeople = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sale_Fast = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    username_fast = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    password_fast = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    accessdescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DonViDoiTac_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenNoiMoToKhai_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaHangKhaiBaoHSCode_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenHang_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tenNuocXuatXu_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dieuKienGiaoHang_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhuongTienVanChuyen_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    diachidonvidoitac_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tendiadiemnhanhangcuoicung_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tendiadiemxephang_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoihanmove = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Customer_ID);
                });

            migrationBuilder.CreateTable(
                name: "Customer_ThamChieu",
                columns: table => new
                {
                    Customer_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaDT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MainCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Customer_Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hancongno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tilecom = table.Column<double>(type: "float", nullable: false),
                    district = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VIPCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnglishName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    COMPANY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BIZName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Web = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Province = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks_Customer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks_sale = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountPotantial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ATTN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuyenHan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    strUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Class_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ngaythem = table.Column<DateTime>(type: "datetime2", nullable: true),
                    New = table.Column<bool>(type: "bit", nullable: true),
                    PIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    commodity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trafic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    air = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    imp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    exp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contentofreport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovePayer = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Updatetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    sotaikhoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phuphicourier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    buy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sell = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    courier = table.Column<bool>(type: "bit", nullable: true),
                    logisticts = table.Column<bool>(type: "bit", nullable: true),
                    airfreight = table.Column<bool>(type: "bit", nullable: true),
                    seafreight = table.Column<bool>(type: "bit", nullable: true),
                    others = table.Column<bool>(type: "bit", nullable: true),
                    rivaldhl = table.Column<bool>(type: "bit", nullable: true),
                    rivaltNT = table.Column<bool>(type: "bit", nullable: true),
                    rivalfedex = table.Column<bool>(type: "bit", nullable: true),
                    rivalups = table.Column<bool>(type: "bit", nullable: true),
                    rivalothers = table.Column<bool>(type: "bit", nullable: true),
                    fieldgarment = table.Column<bool>(type: "bit", nullable: true),
                    fieldtextile = table.Column<bool>(type: "bit", nullable: true),
                    fieldshoes = table.Column<bool>(type: "bit", nullable: true),
                    fieldfurniture = table.Column<bool>(type: "bit", nullable: true),
                    fieldforwarder = table.Column<bool>(type: "bit", nullable: true),
                    fieldbanking = table.Column<bool>(type: "bit", nullable: true),
                    fieldothers = table.Column<bool>(type: "bit", nullable: true),
                    nationalasia = table.Column<bool>(type: "bit", nullable: true),
                    nationalEurope = table.Column<bool>(type: "bit", nullable: true),
                    nationalNAmerica = table.Column<bool>(type: "bit", nullable: true),
                    nationalAustralia = table.Column<bool>(type: "bit", nullable: true),
                    nationalOthers = table.Column<bool>(type: "bit", nullable: true),
                    destasia = table.Column<bool>(type: "bit", nullable: true),
                    desteurope = table.Column<bool>(type: "bit", nullable: true),
                    destnamerica = table.Column<bool>(type: "bit", nullable: true),
                    destaustralia = table.Column<bool>(type: "bit", nullable: true),
                    destothers = table.Column<bool>(type: "bit", nullable: true),
                    styleForeign = table.Column<bool>(type: "bit", nullable: true),
                    styleStateowned = table.Column<bool>(type: "bit", nullable: true),
                    addresstiengviet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    kpis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    shortname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    birthday = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriceDEM = table.Column<double>(type: "float", nullable: true),
                    PriceDET = table.Column<double>(type: "float", nullable: true),
                    Cur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cothue = table.Column<bool>(type: "bit", nullable: true),
                    giatri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tientegiatri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seaopcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    airopcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    freetime = table.Column<bool>(type: "bit", nullable: true),
                    combine_mau = table.Column<bool>(type: "bit", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxInvoice_SGN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AirImport_SGN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeaImport_SGN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxInvoice_HAN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AirImport_HAN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeaImport_HAN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxInvoice_HPH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AirImport_HPH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeaImport_HPH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contactID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    custypeservice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    accref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    inputpeople = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sale_Fast = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    username_fast = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    password_fast = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    accessdescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DonViDoiTac_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenNoiMoToKhai_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaHangKhaiBaoHSCode_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenHang_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tenNuocXuatXu_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dieuKienGiaoHang_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhuongTienVanChuyen_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    diachidonvidoitac_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tendiadiemnhanhangcuoicung_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tendiadiemxephang_gs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoihanmove = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer_ThamChieu", x => x.Customer_ID);
                });

            migrationBuilder.CreateTable(
                name: "CustomerBr",
                columns: table => new
                {
                    customerBrID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customerid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    citybr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    namecitybr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    typebr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    telbr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    faxbr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    addressbr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerBr", x => x.customerBrID);
                });

            migrationBuilder.CreateTable(
                name: "CustomerCode",
                columns: table => new
                {
                    CustomerCode_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerCode_Ref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCode", x => x.CustomerCode_ID);
                });

            migrationBuilder.CreateTable(
                name: "CustomerCommondity",
                columns: table => new
                {
                    CustomerCommondity_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Customer_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Commondity_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    From_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    To_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Season = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Min_tueMonth = table.Column<double>(type: "float", nullable: true),
                    AVG_tueMonth = table.Column<double>(type: "float", nullable: true),
                    Max_tueMonth = table.Column<double>(type: "float", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KhachHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoHD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayKy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayHetHan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HD_NguyenTac = table.Column<bool>(type: "bit", nullable: true),
                    HD_DaiLyHaiQuan = table.Column<bool>(type: "bit", nullable: true),
                    HD_UyThacXuatKhau = table.Column<bool>(type: "bit", nullable: true),
                    Other = table.Column<bool>(type: "bit", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: true),
                    hinhhopdong = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCommondity", x => x.CustomerCommondity_ID);
                });

            migrationBuilder.CreateTable(
                name: "CustomerCommondityFileUpload",
                columns: table => new
                {
                    FileID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerCommondityID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCommondityFileUpload", x => x.FileID);
                });

            migrationBuilder.CreateTable(
                name: "CustomerMarket",
                columns: table => new
                {
                    CustomerMarket_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Customer_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Market_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    exim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    country_market = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    port = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerMarket", x => x.CustomerMarket_Id);
                });

            migrationBuilder.CreateTable(
                name: "CUSTOMERREPORT",
                columns: table => new
                {
                    CusReport_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Customer_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Visitdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Daily = table.Column<bool>(type: "bit", nullable: true),
                    ValidUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    userid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updatetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KPI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoigian = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hoanThanh = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CUSTOMERREPORT", x => x.CusReport_ID);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucTaiKhoan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Taikhoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tentaikhoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Accountname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Manguyente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tinhdauky_Cuoiky = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucTaiKhoan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Debit",
                columns: table => new
                {
                    debitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quotationid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    mblid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hblid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    itemid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customerid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    debitno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    del = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dongia = table.Column<double>(type: "float", nullable: true),
                    soluong = table.Column<double>(type: "float", nullable: true),
                    thanhtien = table.Column<double>(type: "float", nullable: true),
                    tiente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thue = table.Column<double>(type: "float", nullable: true),
                    thanhtiensauthue = table.Column<double>(type: "float", nullable: true),
                    thuho = table.Column<bool>(type: "bit", nullable: true),
                    tigiadebit = table.Column<double>(type: "float", nullable: true),
                    tigiahoandondaura = table.Column<double>(type: "float", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    nhom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ghichu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    daIndebit = table.Column<bool>(type: "bit", nullable: true),
                    daXuatHoadon = table.Column<bool>(type: "bit", nullable: true),
                    khoa = table.Column<bool>(type: "bit", nullable: true),
                    InArrival = table.Column<bool>(type: "bit", nullable: true),
                    sohoadondaura = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Copied = table.Column<bool>(type: "bit", nullable: true),
                    ghichu_debit = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Debit", x => x.debitId);
                });

            migrationBuilder.CreateTable(
                name: "DebitCreditCustomer",
                columns: table => new
                {
                    debitcreditcustomerid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customerid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    kyhieuhoadon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sohoadon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayphathanhhoadon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mathang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    giatruocthue = table.Column<double>(type: "float", nullable: true),
                    thuesuat = table.Column<double>(type: "float", nullable: true),
                    thuegtgt = table.Column<double>(type: "float", nullable: true),
                    bank = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    price = table.Column<double>(type: "float", nullable: true),
                    tigia = table.Column<double>(type: "float", nullable: true),
                    ngay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pttt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tennganhang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mota = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phanbo = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitCreditCustomer", x => x.debitcreditcustomerid);
                });

            migrationBuilder.CreateTable(
                name: "DebitCreditTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DebitCredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserCreate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShipmentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    Cur = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitCreditTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "deNghiHoanUng",
                columns: table => new
                {
                    DeNghiHoanUngID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeNghiTamUngID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    So = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Kinhgui = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tennguoidenghi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bophan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noidung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sotien = table.Column<double>(type: "float", nullable: true),
                    Loaitiente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Chungtukemtheo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bkno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    DateApprove = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Files = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tigia = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deNghiHoanUng", x => x.DeNghiHoanUngID);
                });

            migrationBuilder.CreateTable(
                name: "DeNghiTamUng",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    So = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NGAY = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Kinhgui = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tennguoidenghi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bophan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noidung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sotien = table.Column<double>(type: "float", nullable: true),
                    Loaitiente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Chungtukemtheo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bkno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    DateApprove = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Files = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sotienhoanung = table.Column<double>(type: "float", nullable: true),
                    Sotienconlai = table.Column<double>(type: "float", nullable: true),
                    tigia = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeNghiTamUng", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateTime = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "DNTT_Logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Customerid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    So = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NGAY = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Kinhgui = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tennguoidenghi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bophan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noidung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PTTT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sotientruocthue = table.Column<double>(type: "float", nullable: true),
                    Sotienthue = table.Column<double>(type: "float", nullable: true),
                    Thanhtien = table.Column<double>(type: "float", nullable: true),
                    Loaitiente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sotk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chungtukemtheo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bkno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nguoilap = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ketoantruong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Giamdoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tigia = table.Column<double>(type: "float", nullable: true),
                    SoLanGuiEmail = table.Column<int>(type: "int", nullable: true),
                    EmailUserApprove = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Lidoduyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    timeDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DNTT_Logistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Duan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenDuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaDuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiPhuTrach = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CongNghe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NhanVienThamGia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailNguoiPhuTrach = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUPdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Links = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pricingno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiHangHoa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrongLuong = table.Column<double>(type: "float", nullable: true),
                    KichThuoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianGiaoHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhuongThucVanChuyen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DichVuBoSung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Duyet_DNTT",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdDntt = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Ngayduyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Userduyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    So = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Thanhtien = table.Column<double>(type: "float", nullable: true),
                    Cur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kinhgui = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bophan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoLanGuiEmail = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailUserDeNghi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duyet_DNTT", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExportCostDetail",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    exportcostrequestid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    loaichiphi = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    motachitiet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    chiphivnd = table.Column<double>(type: "float", nullable: true),
                    chiphiusd = table.Column<double>(type: "float", nullable: true),
                    ngaybaogia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportCostDetail", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ExportCostRequest",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    maRFQ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tenhang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soluong = table.Column<double>(type: "float", nullable: true),
                    donvitinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cangdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cangden = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    incoterm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phuongthucVc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayguiyeucau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    noidung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    Userhandle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Usercreate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loaihinh = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportCostRequest", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "GetQuotationNo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    thang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nam = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quotation_number = table.Column<int>(type: "int", nullable: false),
                    userget = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: false),
                    timeget = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GetQuotationNo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "HBLViTriLoHang",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HBLID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HBLNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    ngay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Accuracy = table.Column<double>(type: "float", nullable: true),
                    Altitude = table.Column<double>(type: "float", nullable: true),
                    Speed = table.Column<double>(type: "float", nullable: true),
                    Heading = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HBLViTriLoHang", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HistoryLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    No = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Changes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoaDonDauRa",
                columns: table => new
                {
                    hoadondauraid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    mblid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hblid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    itemid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customerid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    dongia = table.Column<double>(type: "float", nullable: true),
                    soluong = table.Column<double>(type: "float", nullable: true),
                    thanhtien = table.Column<double>(type: "float", nullable: true),
                    tiente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thue = table.Column<double>(type: "float", nullable: true),
                    thanhtiensauthue = table.Column<double>(type: "float", nullable: true),
                    sohoadonNoibo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sohoadonDientu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayphathanhhoadonDientu = table.Column<DateTime>(type: "datetime2", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    ghiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soThuTu = table.Column<int>(type: "int", nullable: true),
                    tigia = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonDauRa", x => x.hoadondauraid);
                });

            migrationBuilder.CreateTable(
                name: "HoaDonDauVao",
                columns: table => new
                {
                    hoadondauvaoid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    mblid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    hblid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customerid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    dongia = table.Column<double>(type: "float", nullable: true),
                    soluong = table.Column<double>(type: "float", nullable: true),
                    thanhtien = table.Column<double>(type: "float", nullable: true),
                    tiente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tigia = table.Column<double>(type: "float", nullable: true),
                    thue = table.Column<double>(type: "float", nullable: true),
                    thanhtiensauthue = table.Column<double>(type: "float", nullable: true),
                    sohoadonNoibo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sohoadonDientu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayphathanhhoadonDientu = table.Column<DateTime>(type: "datetime2", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonDauVao", x => x.hoadondauvaoid);
                });

            migrationBuilder.CreateTable(
                name: "ImportCostRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaRFQ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tenhang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Soluong = table.Column<double>(type: "float", nullable: true),
                    Donvitinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quocgiaxuatxu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cangxuat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cangnhap = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Incoterm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngayguiyeucau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loaihinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noidung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Userhandle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Usercreate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportCostRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inbound",
                columns: table => new
                {
                    Blib_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    soref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gflc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Eta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vessel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Voyage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Consignee = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dest = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MacangFcl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bl_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiadiemGiaoHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CangGiaoHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    Air = table.Column<bool>(type: "bit", nullable: true),
                    Fcl = table.Column<bool>(type: "bit", nullable: true),
                    Lcl = table.Column<bool>(type: "bit", nullable: true),
                    Consol = table.Column<bool>(type: "bit", nullable: true),
                    Paidreceived = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Lot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceRequest = table.Column<bool>(type: "bit", nullable: true),
                    InvoiceRequestDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DebitIssued = table.Column<bool>(type: "bit", nullable: true),
                    InvoiceIssued = table.Column<bool>(type: "bit", nullable: true),
                    Paid = table.Column<bool>(type: "bit", nullable: true),
                    Paiddebit = table.Column<bool>(type: "bit", nullable: true),
                    PaidCredit = table.Column<bool>(type: "bit", nullable: true),
                    NhanLenh = table.Column<bool>(type: "bit", nullable: true),
                    CloseFile = table.Column<bool>(type: "bit", nullable: true),
                    MBL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HBL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BKNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Shipper = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notify = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoDebit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoCredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateReport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Nvocc = table.Column<bool>(type: "bit", nullable: true),
                    Packages = table.Column<double>(type: "float", nullable: true),
                    Kgs = table.Column<double>(type: "float", nullable: true),
                    Cbm = table.Column<double>(type: "float", nullable: true),
                    Container20 = table.Column<int>(type: "int", nullable: true),
                    Container40 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inbound", x => x.Blib_id);
                });

            migrationBuilder.CreateTable(
                name: "InboundFreight",
                columns: table => new
                {
                    Inboundfreightid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Inboundid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quotationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    customerid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    itemid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    containertype = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    unitprice = table.Column<double>(type: "float", nullable: true),
                    pricetruocthue = table.Column<double>(type: "float", nullable: true),
                    pricenotaxvnd = table.Column<double>(type: "float", nullable: true),
                    taxprice = table.Column<double>(type: "float", nullable: true),
                    pricethue = table.Column<double>(type: "float", nullable: true),
                    price = table.Column<double>(type: "float", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quantity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    paycheck = table.Column<bool>(type: "bit", nullable: true),
                    os = table.Column<bool>(type: "bit", nullable: true),
                    ngay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayhoadon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tigia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vitri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    showarrival = table.Column<bool>(type: "bit", nullable: true),
                    soNgayCongNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    othercredit = table.Column<bool>(type: "bit", nullable: true),
                    onbehaft = table.Column<bool>(type: "bit", nullable: true),
                    daily = table.Column<bool>(type: "bit", nullable: true),
                    stt = table.Column<int>(type: "int", nullable: true),
                    showVND = table.Column<bool>(type: "bit", nullable: true),
                    container = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    freeDEM = table.Column<int>(type: "int", nullable: true),
                    freeDET = table.Column<int>(type: "int", nullable: true),
                    no_ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    f1 = table.Column<int>(type: "int", nullable: true),
                    t1 = table.Column<int>(type: "int", nullable: true),
                    p1 = table.Column<double>(type: "float", nullable: true),
                    f2 = table.Column<int>(type: "int", nullable: true),
                    t2 = table.Column<int>(type: "int", nullable: true),
                    p2 = table.Column<double>(type: "float", nullable: true),
                    level = table.Column<int>(type: "int", nullable: true),
                    unitprice_ = table.Column<double>(type: "float", nullable: true),
                    price_ = table.Column<double>(type: "float", nullable: true),
                    dntt = table.Column<bool>(type: "bit", nullable: true),
                    KTT = table.Column<bool>(type: "bit", nullable: true),
                    boss = table.Column<bool>(type: "bit", nullable: true),
                    debit = table.Column<bool>(type: "bit", nullable: true),
                    dongiatruocthueVND = table.Column<double>(type: "float", nullable: true),
                    thanhtientruocthueVND = table.Column<double>(type: "float", nullable: true),
                    tienthueVND = table.Column<double>(type: "float", nullable: true),
                    thanhtiensauthueVND = table.Column<double>(type: "float", nullable: true),
                    ref_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    housebill_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    kiemtra = table.Column<bool>(type: "bit", nullable: true),
                    userkiemtra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaykiemtra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pttt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KYHIEU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaythanhtoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dept = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nhatky = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dachuyen3a = table.Column<bool>(type: "bit", nullable: true),
                    seri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nh_dk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tkno_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tkco_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tkhq_dc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tkhq_dc_ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tennhomphi_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sochungtu_3a = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tax_customerid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    level1_phanquyen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    level2_manager = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    level3_branchmanager = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    level4_general = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mbl_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tendntt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tenktt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tenboss = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gio_dntt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    freightdatereport = table.Column<DateTime>(type: "datetime2", nullable: true),
                    quyenbaocao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phu = table.Column<bool>(type: "bit", nullable: true),
                    EraseNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Book_EraseNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thuevat = table.Column<double>(type: "float", nullable: true),
                    Erase_Date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngay_Erase = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "InboundFreight_History",
                columns: table => new
                {
                    Inboundfreightid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Inboundid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quotationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    customerid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    itemid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    containertype = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    unitprice = table.Column<double>(type: "float", nullable: true),
                    pricetruocthue = table.Column<double>(type: "float", nullable: true),
                    pricenotaxvnd = table.Column<double>(type: "float", nullable: true),
                    taxprice = table.Column<double>(type: "float", nullable: true),
                    pricethue = table.Column<double>(type: "float", nullable: true),
                    price = table.Column<double>(type: "float", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quantity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    paycheck = table.Column<bool>(type: "bit", nullable: true),
                    os = table.Column<bool>(type: "bit", nullable: true),
                    ngay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayhoadon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tigia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vitri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    showarrival = table.Column<bool>(type: "bit", nullable: true),
                    soNgayCongNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    othercredit = table.Column<bool>(type: "bit", nullable: true),
                    onbehaft = table.Column<bool>(type: "bit", nullable: true),
                    daily = table.Column<bool>(type: "bit", nullable: true),
                    stt = table.Column<int>(type: "int", nullable: true),
                    showVND = table.Column<bool>(type: "bit", nullable: true),
                    container = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    freeDEM = table.Column<int>(type: "int", nullable: true),
                    freeDET = table.Column<int>(type: "int", nullable: true),
                    no_ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    f1 = table.Column<int>(type: "int", nullable: true),
                    t1 = table.Column<int>(type: "int", nullable: true),
                    p1 = table.Column<double>(type: "float", nullable: true),
                    f2 = table.Column<int>(type: "int", nullable: true),
                    t2 = table.Column<int>(type: "int", nullable: true),
                    p2 = table.Column<double>(type: "float", nullable: true),
                    level = table.Column<int>(type: "int", nullable: true),
                    unitprice_ = table.Column<double>(type: "float", nullable: true),
                    price_ = table.Column<double>(type: "float", nullable: true),
                    dntt = table.Column<bool>(type: "bit", nullable: true),
                    KTT = table.Column<bool>(type: "bit", nullable: true),
                    boss = table.Column<bool>(type: "bit", nullable: true),
                    debit = table.Column<bool>(type: "bit", nullable: true),
                    dongiatruocthueVND = table.Column<double>(type: "float", nullable: true),
                    thanhtientruocthueVND = table.Column<double>(type: "float", nullable: true),
                    tienthueVND = table.Column<double>(type: "float", nullable: true),
                    thanhtiensauthueVND = table.Column<double>(type: "float", nullable: true),
                    ref_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    housebill_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    kiemtra = table.Column<bool>(type: "bit", nullable: true),
                    userkiemtra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaykiemtra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pttt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KYHIEU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaythanhtoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dept = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nhatky = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dachuyen3a = table.Column<bool>(type: "bit", nullable: true),
                    seri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nh_dk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tkno_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tkco_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tkhq_dc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tkhq_dc_ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tennhomphi_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sochungtu_3a = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tax_customerid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    level1_phanquyen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    level2_manager = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    level3_branchmanager = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    level4_general = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mbl_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tendntt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tenktt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tenboss = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gio_dntt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    freightdatereport = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "InboundFulls",
                columns: table => new
                {
                    BLIB_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BLIB_NO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    printdebit = table.Column<bool>(type: "bit", nullable: true),
                    clockCredit = table.Column<bool>(type: "bit", nullable: true),
                    InvoiceRequest = table.Column<bool>(type: "bit", nullable: true),
                    InvoiceRequestDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DebitIssued = table.Column<bool>(type: "bit", nullable: true),
                    InvoiceIssued = table.Column<bool>(type: "bit", nullable: true),
                    Paid = table.Column<bool>(type: "bit", nullable: true),
                    MBL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HBL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    REF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bkno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SHIPPER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CONSIGNEE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NOTIFY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CY_CFS_ITEM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BL_TYPE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KHO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingMARKS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SAYCONTAINER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KgAvailable = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DESCRIPTION = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bkkcomm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FCL = table.Column<bool>(type: "bit", nullable: true),
                    LCL = table.Column<bool>(type: "bit", nullable: true),
                    CONSOL = table.Column<bool>(type: "bit", nullable: true),
                    air = table.Column<bool>(type: "bit", nullable: true),
                    VESSEL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VOYAGE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SAILINGDATE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ETA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DEL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DEST = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportCY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingLine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgencyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaleCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nhanlenh = table.Column<bool>(type: "bit", nullable: true),
                    Ngaynhanlenh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nguoinhanlenh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pkgs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    attachList = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManifestDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentHPGDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    closeFile = table.Column<bool>(type: "bit", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    paidreceived = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tigia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    tongsoluong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tongsoluong1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loai1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tongsoluong2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loai2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tongkien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tongkg = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tongkhoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soHoSo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    namDangkyhoSo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loaiChungtu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    chucNangCuaChungTu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loaichuyendi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tentau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soIMO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cangden = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoiGianDen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hohieu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nguoiGuiHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nguoiNhanHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cangChuyentai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cangGiaohang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cangXephang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cangDoHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soVanDon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayPhatHanhVanDon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soVanDonGoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayPhatHanhVanDonGoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayKhoiHanh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tongSoKienLoaiKien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hsCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notify1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notify2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    diadiemgiaohang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loaihang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ghichu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    stt = table.Column<int>(type: "int", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    daguiemail = table.Column<bool>(type: "bit", nullable: true),
                    tkhq = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemSITC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thoihanhoantamung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaydi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayve = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mucdich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NOMI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SALE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POLCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ops = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    datereport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    agentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customerid_showTC = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoDebit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    noCredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemSITC1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemSITC2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    coquanhaiquan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_kinhgui = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_tau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_chuyen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_POL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_POD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_ETA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_hbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_mbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_soContSeal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_soluong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_chitiethanghoa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_trongluong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_khoiluong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_BillType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_phi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    arrival_tongcong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DO_Kinhgui = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DO_Consignee = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bophan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loaikien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaynhanhang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaytravorong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GioGiaHanDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayGiaHanDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    kinhguiGCV = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soluongGCV = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KhoiluongThetich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nhom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    rf = table.Column<bool>(type: "bit", nullable: true),
                    for_ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tongteu = table.Column<double>(type: "float", nullable: true),
                    tongcbm = table.Column<double>(type: "float", nullable: true),
                    nvocc = table.Column<bool>(type: "bit", nullable: true),
                    thoihanlenh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    d_thongtinbosung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    d_noiky = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    d_ngayky = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    d_nguoiky = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    oprcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayhabai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    transit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    showfreeDem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    showfreeDet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaKhaiBaoVNSW = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DealineSubmitVNSW = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    guilan1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    guilan2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    macangFCL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    giahanlan1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    giahanlan2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    giahanlan3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gFLC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gSC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quotationno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pickupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    freeDemUpto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    freestoUpto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nhamay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    multiuser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loaihanghoa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    oref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayhttthq = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    volume = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    schedule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    COLOADER_INBOUND = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tennhomphi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    paiddebit = table.Column<bool>(type: "bit", nullable: true),
                    paidcredit = table.Column<bool>(type: "bit", nullable: true),
                    depot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phuongannhancontedo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hannhancontedo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hantracontedo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sotienedo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ghichuedo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    solenhedo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mabaomatedo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarksdo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    destpreedays = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    expiri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    songaymien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    freestorage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    deposit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    subjectemail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreightAmount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vprepaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    commodity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    code_shippingline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    charge_profit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    payment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dc1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dc2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    profit_new = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    charge_profit_incvat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    payment_incvat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dc1_incvat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dc2_incvat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    profit_new_incvat = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "IssueReports",
                columns: table => new
                {
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StepsToReproduce = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reporter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScreenshotUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserHandle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    No = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueReports", x => x.IssueId);
                });

            migrationBuilder.CreateTable(
                name: "Job",
                columns: table => new
                {
                    JobID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FLC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    statusJob = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Datecreate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Salecode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Job", x => x.JobID);
                });

            migrationBuilder.CreateTable(
                name: "KTCLCostRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaRFQ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tenhang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loaihinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noidung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayYeuCau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Userhandle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Usercreate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KTCLCostRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveType = table.Column<int>(type: "int", nullable: false),
                    DurationType = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApproverComment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LenhDieuXe",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    YeuCauTruckingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Nvdieuxe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Taixe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phuxe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nhaxe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ghichu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loaidhvc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loaiphuongtien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Soxe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Somooc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaCost = table.Column<double>(type: "float", nullable: true),
                    Tongkm = table.Column<double>(type: "float", nullable: true),
                    Dinhmucdau = table.Column<double>(type: "float", nullable: true),
                    Tamung = table.Column<double>(type: "float", nullable: true),
                    Sophieu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngaylap = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ngaynhanlenh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ngaydi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ngayve = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Lenhdieuxeno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangthaiCt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Pol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pod = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LenhDieuXe", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "listDebits",
                columns: table => new
                {
                    company = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dept = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    item = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quantity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    containertype = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    unitprice_ = table.Column<double>(type: "float", nullable: true),
                    taxprice = table.Column<double>(type: "float", nullable: true),
                    price_ = table.Column<double>(type: "float", nullable: true),
                    unitprice = table.Column<double>(type: "float", nullable: true),
                    price = table.Column<double>(type: "float", nullable: true),
                    thanhtiensauthueVND = table.Column<double>(type: "float", nullable: true),
                    tigia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    itemid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    no_ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    inboundid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    inboundfreightid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customerid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    container = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    boss = table.Column<bool>(type: "bit", nullable: true),
                    ktt = table.Column<bool>(type: "bit", nullable: true),
                    freedem = table.Column<int>(type: "int", nullable: true),
                    freedet = table.Column<int>(type: "int", nullable: true),
                    pricetruocthue = table.Column<double>(type: "float", nullable: true),
                    pricenotaxvnd = table.Column<double>(type: "float", nullable: true),
                    pricethue = table.Column<double>(type: "float", nullable: true),
                    paycheck = table.Column<bool>(type: "bit", nullable: true),
                    os = table.Column<bool>(type: "bit", nullable: true),
                    ngay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngayhoadon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dongiatruocthueVND = table.Column<double>(type: "float", nullable: true),
                    thanhtientruocthueVND = table.Column<double>(type: "float", nullable: true),
                    tienthueVND = table.Column<double>(type: "float", nullable: true),
                    eraseno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bkno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    showarrival = table.Column<bool>(type: "bit", nullable: true),
                    soNgayCongNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    daily = table.Column<bool>(type: "bit", nullable: true),
                    housebill_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ref_debitcredit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    stt = table.Column<int>(type: "int", nullable: true),
                    showvnd = table.Column<bool>(type: "bit", nullable: true),
                    f1 = table.Column<int>(type: "int", nullable: true),
                    f2 = table.Column<int>(type: "int", nullable: true),
                    t1 = table.Column<int>(type: "int", nullable: true),
                    t2 = table.Column<int>(type: "int", nullable: true),
                    p1 = table.Column<double>(type: "float", nullable: true),
                    p2 = table.Column<double>(type: "float", nullable: true),
                    level = table.Column<int>(type: "int", nullable: true),
                    freightdatereport = table.Column<DateTime>(type: "datetime2", nullable: true),
                    quyenbaocao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "ListDept",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Viewername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tablename = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListDept", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "localcharges_pt",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Index_ = table.Column<int>(type: "int", nullable: true),
                    Carr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Thc20dc = table.Column<double>(type: "float", nullable: true),
                    Thc40hc = table.Column<double>(type: "float", nullable: true),
                    Thc20rf = table.Column<double>(type: "float", nullable: true),
                    Thc40rf = table.Column<double>(type: "float", nullable: true),
                    EBS20dc = table.Column<double>(type: "float", nullable: true),
                    EBS40hc = table.Column<double>(type: "float", nullable: true),
                    EBS20rf = table.Column<double>(type: "float", nullable: true),
                    EBS40rf = table.Column<double>(type: "float", nullable: true),
                    Seal = table.Column<double>(type: "float", nullable: true),
                    BL = table.Column<double>(type: "float", nullable: true),
                    Telex = table.Column<double>(type: "float", nullable: true),
                    ENS = table.Column<double>(type: "float", nullable: true),
                    LatePayMentFEE = table.Column<double>(type: "float", nullable: true),
                    CIC = table.Column<double>(type: "float", nullable: true),
                    LSS = table.Column<double>(type: "float", nullable: true),
                    ISPS = table.Column<double>(type: "float", nullable: true),
                    MTF = table.Column<double>(type: "float", nullable: true),
                    MAF = table.Column<double>(type: "float", nullable: true),
                    CURRENCY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngaybatdau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ngayketthuc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_localcharges_pt", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalizationResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResourceKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Culture = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizationResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MARKET",
                columns: table => new
                {
                    Market_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarketCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Market = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RemarksBooking = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MARKET", x => x.Market_ID);
                });

            migrationBuilder.CreateTable(
                name: "MenuNames",
                columns: table => new
                {
                    MenuID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuNames", x => x.MenuID);
                });

            migrationBuilder.CreateTable(
                name: "MoveCustomer",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoveCustomer", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceiverUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MenuName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    See = table.Column<bool>(type: "bit", nullable: true),
                    Edit = table.Column<bool>(type: "bit", nullable: true),
                    Del = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Add = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "PermissionTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MenuName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dept = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    canView = table.Column<bool>(type: "bit", nullable: true),
                    canAdd = table.Column<bool>(type: "bit", nullable: true),
                    canEdit = table.Column<bool>(type: "bit", nullable: true),
                    canDelete = table.Column<bool>(type: "bit", nullable: true),
                    canApprove = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Phieuchi",
                columns: table => new
                {
                    PhieuchiID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Customer_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Sophieuchi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKnophieuchi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKCophieuchi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PTTT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noidung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sotien = table.Column<double>(type: "float", nullable: true),
                    Tigia = table.Column<double>(type: "float", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chungtu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nguoinoptien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bank = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoHoaDon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Useupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STT = table.Column<int>(type: "int", nullable: true),
                    Vesselvoy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loaiphieu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quyenso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mbl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phieuchi", x => x.PhieuchiID);
                });

            migrationBuilder.CreateTable(
                name: "Phieuketoan",
                columns: table => new
                {
                    PhieuketoanID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Customer_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Sophieuketoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKnophieuketoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKCophieuketoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PTTT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noidung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sotien = table.Column<double>(type: "float", nullable: true),
                    SotienVnd = table.Column<double>(type: "float", nullable: true),
                    Tigia = table.Column<double>(type: "float", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chungtu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nguoinoptien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bank = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mblcarrier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoHD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Userid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Updatetime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STT = table.Column<int>(type: "int", nullable: true),
                    Thoihanhoantamung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngaydi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngayve = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mucdich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OLC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OFC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IFC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ILC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ICB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CLD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TK = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NDC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YDC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NOMI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SALE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ozb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Masterbill = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vesselvoy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loaiphieu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quyenso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mbl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phieuketoan", x => x.PhieuketoanID);
                });

            migrationBuilder.CreateTable(
                name: "Phieuthu",
                columns: table => new
                {
                    PhieuthuID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Customer_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TaxInvoiceID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SoPhieuthu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKCo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ngayhachtoan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PTTT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noidung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sotien = table.Column<double>(type: "float", nullable: true),
                    Tigia = table.Column<double>(type: "float", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chungtu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nguoinoptien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bank = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STT = table.Column<int>(type: "int", nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loaiphieu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Socont = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Travotai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ruthangtaibai = table.Column<bool>(type: "bit", nullable: true),
                    Soluongcont = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hanlenhharong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quyenso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sohoadon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Soseri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Taikhoan_Doiung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sodudauky = table.Column<bool>(type: "bit", nullable: true),
                    Lastcargo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Depoaddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Customername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Luuycuoc = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phieuthu", x => x.PhieuthuID);
                });

            migrationBuilder.CreateTable(
                name: "PIC",
                columns: table => new
                {
                    PIC_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Customer_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectLine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PIC", x => x.PIC_ID);
                });

            migrationBuilder.CreateTable(
                name: "Port",
                columns: table => new
                {
                    PORT_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PORT_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PORT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MARKETCODETS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MARKETCODESALE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TEL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FAX = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADDRESS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    COUNTRY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DATEEXP = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TRADECODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverW20 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverW40 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    APPROVE = table.Column<bool>(type: "bit", nullable: true),
                    CONTINUED = table.Column<bool>(type: "bit", nullable: true),
                    EDITABLE = table.Column<bool>(type: "bit", nullable: true),
                    USERID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UPDATETIME = table.Column<DateTime>(type: "datetime2", nullable: true),
                    show = table.Column<bool>(type: "bit", nullable: true),
                    dept = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Port", x => x.PORT_ID);
                });

            migrationBuilder.CreateTable(
                name: "Product_Price",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pricingid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaRFQ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    agent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    line = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Productpriceno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUPdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    noidung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    noidungcongviec = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tieude = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    userguiemail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product_Price", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Product_Price_Detail_ALL",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RFQ_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Itemid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Dongia = table.Column<double>(type: "float", nullable: true),
                    Vat = table.Column<double>(type: "float", nullable: true),
                    Thanhtien = table.Column<double>(type: "float", nullable: true),
                    Dieukhoanthanhtoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngaybaogia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Createat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Usercreate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Cur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserRespond = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loaicont = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tigia = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product_Price_Detail_ALL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product_Price_Details",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    product_price_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    itemid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    price = table.Column<double>(type: "float", nullable: true),
                    cur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUPdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayBaoGia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    CreateAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserCreate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product_Price_Details", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Quotation",
                columns: table => new
                {
                    quotationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    quotationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookingNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    o1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    o2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    o3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    o4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    terms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransitTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transitport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    shippingline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    frequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Routing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    validDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViTri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MasterColoader = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ETA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ETD = table.Column<DateTime>(type: "datetime2", nullable: true),
                    docid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    dated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RFQ_No = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SL = table.Column<double>(type: "float", nullable: true),
                    loaivolume = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotation", x => x.quotationID);
                });

            migrationBuilder.CreateTable(
                name: "QuotationTico",
                columns: table => new
                {
                    QuotationTicoID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    No = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    den = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    diachi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vessel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    volume = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    por = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    term = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    shipper = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    consignee = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    containerType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    polpod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    etd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    validity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    text1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    text2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    text3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sale = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sea_air = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cusid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    closed = table.Column<bool>(type: "bit", nullable: true),
                    text4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    text5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    text6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lichtau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    placeofdelivery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    placeofpickup = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationTico", x => x.QuotationTicoID);
                });

            migrationBuilder.CreateTable(
                name: "RefNo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    thang = table.Column<int>(type: "int", nullable: true),
                    nam = table.Column<int>(type: "int", nullable: true),
                    ref_number = table.Column<int>(type: "int", nullable: true),
                    used_job = table.Column<bool>(type: "bit", nullable: true),
                    userused_job = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_hbl = table.Column<bool>(type: "bit", nullable: true),
                    userused_hbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    userused_debit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_debit = table.Column<bool>(type: "bit", nullable: true),
                    userused_pricing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_pricing = table.Column<bool>(type: "bit", nullable: true),
                    userused_Product_Price = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_Product_Price = table.Column<bool>(type: "bit", nullable: true),
                    userused_LDX = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_LDX = table.Column<bool>(type: "bit", nullable: true),
                    userused_YCTrucking = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_YCTrucking = table.Column<bool>(type: "bit", nullable: true),
                    userused_TKHQ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_TKHQ = table.Column<bool>(type: "bit", nullable: true),
                    userused_YeuCauTuVanHq = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_YeuCauTuVanHQ = table.Column<bool>(type: "bit", nullable: true),
                    userused_BGNH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_BGNH = table.Column<bool>(type: "bit", nullable: true),
                    userused_BGRR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_BGRR = table.Column<bool>(type: "bit", nullable: true),
                    userused_Invoice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_Invoice = table.Column<bool>(type: "bit", nullable: true),
                    userused_KDTV = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_KDTV = table.Column<bool>(type: "bit", nullable: true),
                    userused_KTCL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_KTCL = table.Column<bool>(type: "bit", nullable: true),
                    userused_BGLK = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_BGLK = table.Column<bool>(type: "bit", nullable: true),
                    userused_DNTU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_DNTU = table.Column<bool>(type: "bit", nullable: true),
                    userused_DNHU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_DNHU = table.Column<bool>(type: "bit", nullable: true),
                    userused_ChitietLuuKho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_ChitietLuuKho = table.Column<bool>(type: "bit", nullable: true),
                    userused_PT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_PT = table.Column<bool>(type: "bit", nullable: true),
                    userused_PC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_PC = table.Column<bool>(type: "bit", nullable: true),
                    userused_PKT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_PkT = table.Column<bool>(type: "bit", nullable: true),
                    userused_RFQ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_RFQ = table.Column<bool>(type: "bit", nullable: true),
                    userused_RFQ_Log = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_RFQ_Log = table.Column<bool>(type: "bit", nullable: true),
                    userused_QUO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_QUO = table.Column<bool>(type: "bit", nullable: true),
                    userused_DNTT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_DNTT = table.Column<bool>(type: "bit", nullable: true),
                    userused_Duyet_DNTT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_Duyet_DNTT = table.Column<bool>(type: "bit", nullable: true),
                    userused_pricing_import = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_pricing_import = table.Column<bool>(type: "bit", nullable: true),
                    userused_pricing_truck = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_pricing_truck = table.Column<bool>(type: "bit", nullable: true),
                    userused_pricing_KTCL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_pricing_KTCL = table.Column<bool>(type: "bit", nullable: true),
                    userused_pricing_Customs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_pricing_Customs = table.Column<bool>(type: "bit", nullable: true),
                    userused_IssueRpt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_IssueRpt = table.Column<bool>(type: "bit", nullable: true),
                    userused_CC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    used_CC = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefNo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "RFQ",
                columns: table => new
                {
                    RFQ_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RFQNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PickupAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Incoterm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CargoDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HSCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDangerous = table.Column<bool>(type: "bit", nullable: true),
                    IsFragile = table.Column<bool>(type: "bit", nullable: true),
                    IsTemperatureControlled = table.Column<bool>(type: "bit", nullable: true),
                    IsHighValue = table.Column<bool>(type: "bit", nullable: true),
                    PackageQuantity = table.Column<int>(type: "int", nullable: true),
                    TotalWeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalVolumeCBM = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PackageDimensions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContainerType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReadyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestedShipDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NeedInlandTransport = table.Column<bool>(type: "bit", nullable: true),
                    NeedCustomsClearance = table.Column<bool>(type: "bit", nullable: true),
                    NeedPacking = table.Column<bool>(type: "bit", nullable: true),
                    NeedWarehousing = table.Column<bool>(type: "bit", nullable: true),
                    NeedLabeling = table.Column<bool>(type: "bit", nullable: true),
                    NeedAMSOrISF = table.Column<bool>(type: "bit", nullable: true),
                    SpecialNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RFQStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SalesPerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuoteDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    filesPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFQ", x => x.RFQ_ID);
                });

            migrationBuilder.CreateTable(
                name: "RFQ_Log",
                columns: table => new
                {
                    RFQ_logID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RFQID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusBefore = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusAfter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFQ_Log", x => x.RFQ_logID);
                });

            migrationBuilder.CreateTable(
                name: "Sale",
                columns: table => new
                {
                    Sale_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsrID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SaleCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nhomsale = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Usr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Target = table.Column<double>(type: "float", nullable: true),
                    Tile = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sale", x => x.Sale_ID);
                });

            migrationBuilder.CreateTable(
                name: "SGN_bkno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    autonumber = table.Column<int>(type: "int", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    userget = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    timeget = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SGN_bkno", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShippingLine",
                columns: table => new
                {
                    SHIPPINGLINEID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SHIPPINGLINE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADDRESS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TEL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FAX = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PIC1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POS1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PIC2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POS2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PIC3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POS3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Noidi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noiden = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingLine", x => x.SHIPPINGLINEID);
                });

            migrationBuilder.CreateTable(
                name: "TaxDetail",
                columns: table => new
                {
                    taxDetailID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    taxInvoiceID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    STT = table.Column<int>(type: "int", nullable: true),
                    Items = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    itemid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Clucidat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soLuong = table.Column<double>(type: "float", nullable: true),
                    dongiatruocthueVND = table.Column<double>(type: "float", nullable: true),
                    thanhtientruocthueVND = table.Column<double>(type: "float", nullable: true),
                    Container_Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExchangeRate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxDetail", x => x.taxDetailID);
                });

            migrationBuilder.CreateTable(
                name: "TaxInvoice",
                columns: table => new
                {
                    TaxInvoiceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Customer_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BillNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SerialNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soPhieuThu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInvoice = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MethodPayment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VAT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VATShow = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chungtu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exchange = table.Column<double>(type: "float", nullable: true),
                    huy = table.Column<bool>(type: "bit", nullable: true),
                    lyDoHuy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    REF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Updatetime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VesselVoy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    daXuatHDDT = table.Column<bool>(type: "bit", nullable: true),
                    mauSo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceGUID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    daKy = table.Column<bool>(type: "bit", nullable: true),
                    tongTruocThue = table.Column<double>(type: "float", nullable: true),
                    tongThue = table.Column<double>(type: "float", nullable: true),
                    tongSauThue = table.Column<double>(type: "float", nullable: true),
                    ghichuNoibo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxInvoice", x => x.TaxInvoiceID);
                });

            migrationBuilder.CreateTable(
                name: "Terminal",
                columns: table => new
                {
                    TerminalID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TermiNalName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    FreeStorage = table.Column<double>(type: "float", nullable: true),
                    ValidOrder = table.Column<int>(type: "int", nullable: true),
                    OrderReport = table.Column<int>(type: "int", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    code_nvocc = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terminal", x => x.TerminalID);
                });

            migrationBuilder.CreateTable(
                name: "TERMINALDEPARTURE",
                columns: table => new
                {
                    TerminalDeparture_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgencyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vessel_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Port_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Voyage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WharfName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArrivalPilot_A = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PilotOnBoard_A = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PilotOnBoard_D = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstLine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastLine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Commenced = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Finished = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FO_A = table.Column<double>(type: "float", nullable: true),
                    FO_D = table.Column<double>(type: "float", nullable: true),
                    DO_A = table.Column<double>(type: "float", nullable: true),
                    DO_D = table.Column<double>(type: "float", nullable: true),
                    FreshWater_A = table.Column<double>(type: "float", nullable: true),
                    FreshWater_D = table.Column<double>(type: "float", nullable: true),
                    DraftFwd_A = table.Column<double>(type: "float", nullable: true),
                    DraftFwd_D = table.Column<double>(type: "float", nullable: true),
                    DraftAft_A = table.Column<double>(type: "float", nullable: true),
                    DraftAft_D = table.Column<double>(type: "float", nullable: true),
                    TTLFull_A = table.Column<int>(type: "int", nullable: true),
                    TTLFull_D = table.Column<int>(type: "int", nullable: true),
                    TTLEmpty_A = table.Column<int>(type: "int", nullable: true),
                    TTLEmpty_D = table.Column<int>(type: "int", nullable: true),
                    TTLGross_A = table.Column<int>(type: "int", nullable: true),
                    TTLGross_D = table.Column<int>(type: "int", nullable: true),
                    TugsIn = table.Column<int>(type: "int", nullable: true),
                    TugsOut = table.Column<int>(type: "int", nullable: true),
                    Remark_A = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remark_D = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BerThedTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperationTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CashToCaption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NextPortCall = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ETANextPort = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HatchCover = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fore = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gui = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    _To = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Port = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    After = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KindOfCargo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumCrew = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumPassenger = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPortCall = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualDisplace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PurposePort = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastTimeArrival = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PortOfArrival = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortArrivedFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateArrival = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MasterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BreifParticular = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuantityCargo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuantityDanger = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PosPort = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherConcer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    continued = table.Column<bool>(type: "bit", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    userid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updatetime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TERMINALDEPARTURE", x => x.TerminalDeparture_ID);
                });

            migrationBuilder.CreateTable(
                name: "Test_getapi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Time = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sdt = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Test_getapi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Theodoilohang",
                columns: table => new
                {
                    theodoilohangID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    outboundid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Items = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    validUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngaychungtu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bangoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    intheodoi = table.Column<bool>(type: "bit", nullable: true),
                    LOCATION = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    toadoMap = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Theodoilohang", x => x.theodoilohangID);
                });

            migrationBuilder.CreateTable(
                name: "TiepDauNgu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Loai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hangso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POD = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiepDauNgu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TKHQ_Thongtinchitiet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id_Tkhq = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tenhanghoa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mahs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Soluong = table.Column<double>(type: "float", nullable: true),
                    Dontvitinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Giatrihanghoa = table.Column<double>(type: "float", nullable: true),
                    Xuatxuhanghoa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TKHQ_Thongtinchitiet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TKHQ_Thongtinchunglohang",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Mst_Xk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Company_Xk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Xk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mst_Nk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Company_Nk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Nk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sotokhai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngaykhaibao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Noikhaibao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phuongthucvanchuyen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phuongtienvanchuyen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noixephang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noidohang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sohoadon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngaylaphoadon = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sotien = table.Column<double>(type: "float", nullable: true),
                    Dieukiengiaohang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Orther = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nguoiyeucau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nguoitraloi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Links = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TieuDeEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoidungEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoidungChitietEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TKHQ_Thongtinchunglohang", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TraLoiYeuCauHQCO",
                columns: table => new
                {
                    TraLoiyeuCauHQCOID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    YctvtthqcoID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NguoiTraLoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraLoiYeuCauHQCO", x => x.TraLoiyeuCauHQCOID);
                });

            migrationBuilder.CreateTable(
                name: "TrangThai",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tieude = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Noidung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mamau = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrangThai", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TruckingCostRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaRFQ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayYeuCau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Nguoiyeucau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Loaihang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Soluong = table.Column<double>(type: "float", nullable: true),
                    Donvitinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kichthuochang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Loaixe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noilayhang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Noigiaohang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngaylayhang = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ghichu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Createat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Usercreate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Userhandle = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckingCostRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserActionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserList",
                columns: table => new
                {
                    UsrId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Usr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pass_viettel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Manager2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NickName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Zaloid = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserList", x => x.UsrId);
                });

            migrationBuilder.CreateTable(
                name: "VesselSpace",
                columns: table => new
                {
                    VesselSpaceID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PODCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vessel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Voy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ETD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GATEINLADEN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Carrier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Confirm_ = table.Column<int>(type: "int", nullable: true),
                    Ghichu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselSpace", x => x.VesselSpaceID);
                });

            migrationBuilder.CreateTable(
                name: "WordDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YeuCauTrucking",
                columns: table => new
                {
                    YeuCauTruckingID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    YeuCauTruckingNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nguoiyeucau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nguoitraloi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TieuDeEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoidungEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContainerLocation_empty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaDiemNhanHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaDiemTraHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiCont = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDukienLayHang = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayDuKienTraHang = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Routing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trangthai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Links = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoidungChitietEmail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauTrucking", x => x.YeuCauTruckingID);
                });

            migrationBuilder.CreateTable(
                name: "YeuCauTuVanThuTucHQCO",
                columns: table => new
                {
                    YctvtthqcoID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Congty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    masothue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    diachi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoYeuCau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserYeuCau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserTraLoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TieuDeYeuCau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    YKien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserUPdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    Links = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauTuVanThuTucHQCO", x => x.YctvtthqcoID);
                });

            migrationBuilder.CreateTable(
                name: "Ykien_HQCO",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Ykien = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ykien_HQCO", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ZaloAccesstoken",
                columns: table => new
                {
                    Accesstoken = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Refreshtoken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZaloAccesstoken", x => x.Accesstoken);
                });

            migrationBuilder.CreateTable(
                name: "MBL",
                columns: table => new
                {
                    MblID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Jobid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SaleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bkno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Shipper = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Consignee = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notify1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notify2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vessel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Voy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Porname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Porcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Polname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Polcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Podname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Podcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Delname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Delcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreightPayableAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfOriginal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlaceAndDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreightAmount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateLaden = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ForDelivery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Say = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoadStowCount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShipOnboard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    Editable = table.Column<bool>(type: "bit", nullable: true),
                    Approve = table.Column<bool>(type: "bit", nullable: true),
                    UserUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    statusMBL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    markAndNumbers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoOfPackages = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gross = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CBM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_ShipperAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_ConsigneeAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_IssuingCarrier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_AccountingInfomation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_AgentIATACode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_AccountNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_AirportofDeparture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_to = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_ByfirstCarrier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_to2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_by2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_to3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_by3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_CHGSCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_PPD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_COLL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_otherPPD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_otherCOLL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_DeclaredValuedForCarriage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_DeclaredValueForCustoms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_AirportOfDestination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_FlightDate1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_FlightDate2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_AmountOfInsurance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_HandlingInfomation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_NoOfPiecesRCP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_GrossWieght = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_kglb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_RateClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_CommodityItemNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_ChargeableWeight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_RateCharge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_Total = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_NatureAndquantityOfgoods = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_WeightCharge_Prepaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_WeightCharge_Collect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_Valuation_Prepaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_Valuation_Collect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_Tax_Prepaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_Tax_Collect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalOtherChargesDueAgent_Prepaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalOtherChargesDueAgent_Collect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalOtherChargesDueCarrier_Prepaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalOtherChargesDueCarrier_Collect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalPrepaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalCollect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_CurrencyConversionRates = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_CCChargesInDestCurrenct = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_ChargesatDestination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalCollectCharges = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_PAM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_OtherCharges = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_HisAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ETA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ETD = table.Column<DateTime>(type: "datetime2", nullable: true),
                    dathudebit = table.Column<bool>(type: "bit", nullable: true),
                    dachicredit = table.Column<bool>(type: "bit", nullable: true),
                    dathuchidaily = table.Column<bool>(type: "bit", nullable: true),
                    mbl_Truck_LenhDieuXeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mbl_Truck_YeucauTruckingNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualDelDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MBL", x => x.MblID);
                    table.ForeignKey(
                        name: "FK_MBL_Job_Jobid",
                        column: x => x.Jobid,
                        principalTable: "Job",
                        principalColumn: "JobID");
                });

            migrationBuilder.CreateTable(
                name: "UserThemePreferences",
                columns: table => new
                {
                    PreferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsrId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NavMenuBackgroundColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MainLayoutBackgroundColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserThemePreferences", x => x.PreferenceId);
                    table.ForeignKey(
                        name: "FK_UserThemePreferences_UserList_UsrId",
                        column: x => x.UsrId,
                        principalTable: "UserList",
                        principalColumn: "UsrId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HBL",
                columns: table => new
                {
                    hblID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    mblid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    bkno = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hbl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    statusHBL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    shipper = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    consignee = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notify1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notify2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    vessel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    voy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    porname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    porcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    polname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    polcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    podcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    delcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    delName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    markAndNumbers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoOfPackages = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gross = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cbm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    freightPayableAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    numberOfOriginal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    placeAndDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    freightAmount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateLaden = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    forDelivery = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    shippingMark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    statusMBL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    say = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loadStowCount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    shipOnboard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Continued = table.Column<bool>(type: "bit", nullable: true),
                    editable = table.Column<bool>(type: "bit", nullable: true),
                    approve = table.Column<bool>(type: "bit", nullable: true),
                    userupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dateupdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_ShipperAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_ConsigneeAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_IssuingCarrier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_AccountingInfomation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_AgentIATACode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_AccountNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_AirportofDeparture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_to = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_ByfirstCarrier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_to2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_by2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_to3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_by3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_CHGSCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_PPD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_COLL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_otherPPD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_otherCOLL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_DeclaredValuedForCarriage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_DeclaredValueForCustoms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_AirportOfDestination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_FlightDate1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_FlightDate2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_AmountOfInsurance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_HandlingInfomation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_NoOfPiecesRCP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_GrossWieght = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_kglb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_RateClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_CommodityItemNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_ChargeableWeight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_RateCharge = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_Total = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_NatureAndquantityOfgoods = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_WeightCharge_Prepaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_WeightCharge_Collect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_Valuation_Prepaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_Valuation_Collect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_Tax_Prepaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_Tax_Collect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalOtherChargesDueAgent_Prepaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalOtherChargesDueAgent_Collect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalOtherChargesDueCarrier_Prepaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalOtherChargesDueCarrier_Collect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalPrepaid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalCollect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_CurrencyConversionRates = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_CCChargesInDestCurrenct = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_OtherCharges = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_ChargesatDestination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_TotalCollectCharges = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_PAM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_HisAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_wareHouse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Air_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ETA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ETD = table.Column<DateTime>(type: "datetime2", nullable: true),
                    issueinvoice = table.Column<bool>(type: "bit", nullable: true),
                    dathudebit = table.Column<bool>(type: "bit", nullable: true),
                    dachicredit = table.Column<bool>(type: "bit", nullable: true),
                    dathuchidaily = table.Column<bool>(type: "bit", nullable: true),
                    Truck_LenhDieuXeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Truck_YeucauTruckingNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Jobid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TKHQ_MaHaiQuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKHQ_TenHaiQuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKHQ_MaLoaiHinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKHQ_TenLoaiHinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKHQ_NamDangKy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKHQ_SoToKhai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKHQ_NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TKHQ_MaDonVi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKHQ_NgayThongQuan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TKHQ_NgayQuaKhuVucGiamSat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TKHQ_TenLuong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualDelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    datereport = table.Column<DateTime>(type: "datetime2", nullable: true),
                    release = table.Column<bool>(type: "bit", nullable: true),
                    releaseNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TKHQ_SoTKdangky = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HBL", x => x.hblID);
                    table.ForeignKey(
                        name: "FK_HBL_MBL_mblid",
                        column: x => x.mblid,
                        principalTable: "MBL",
                        principalColumn: "MblID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HBL_mblid",
                table: "HBL",
                column: "mblid");

            migrationBuilder.CreateIndex(
                name: "IX_LocalizationResources_ResourceKey_Culture",
                table: "LocalizationResources",
                columns: new[] { "ResourceKey", "Culture" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MBL_Jobid",
                table: "MBL",
                column: "Jobid");

            migrationBuilder.CreateIndex(
                name: "IX_UserThemePreferences_UsrId",
                table: "UserThemePreferences",
                column: "UsrId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agency");

            migrationBuilder.DropTable(
                name: "AttendanceLogs");

            migrationBuilder.DropTable(
                name: "BieugiaKDTV");

            migrationBuilder.DropTable(
                name: "BieugiaKTCL");

            migrationBuilder.DropTable(
                name: "Bieugialuukho");

            migrationBuilder.DropTable(
                name: "BieuGiaNangHa");

            migrationBuilder.DropTable(
                name: "Bieugiarutruot");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "Buyer");

            migrationBuilder.DropTable(
                name: "Charge");

            migrationBuilder.DropTable(
                name: "ChargeTemplete");

            migrationBuilder.DropTable(
                name: "chiTietBieuGiaLuuKho");

            migrationBuilder.DropTable(
                name: "Commondity");

            migrationBuilder.DropTable(
                name: "CompanyInfomation");

            migrationBuilder.DropTable(
                name: "CongNoHoaDonDauRa");

            migrationBuilder.DropTable(
                name: "CongNoHoaDonDauVao");

            migrationBuilder.DropTable(
                name: "Container");

            migrationBuilder.DropTable(
                name: "ContainerDamage");

            migrationBuilder.DropTable(
                name: "ContainerMovement");

            migrationBuilder.DropTable(
                name: "CONTAINEROUTBOUNDNOTIFY_sale");

            migrationBuilder.DropTable(
                name: "ContainerRepair");

            migrationBuilder.DropTable(
                name: "ContainerSeal");

            migrationBuilder.DropTable(
                name: "Credit");

            migrationBuilder.DropTable(
                name: "CuocCont");

            migrationBuilder.DropTable(
                name: "Currency");

            migrationBuilder.DropTable(
                name: "CustomCostRequest");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "Customer_ThamChieu");

            migrationBuilder.DropTable(
                name: "CustomerBr");

            migrationBuilder.DropTable(
                name: "CustomerCode");

            migrationBuilder.DropTable(
                name: "CustomerCommondity");

            migrationBuilder.DropTable(
                name: "CustomerCommondityFileUpload");

            migrationBuilder.DropTable(
                name: "CustomerMarket");

            migrationBuilder.DropTable(
                name: "CUSTOMERREPORT");

            migrationBuilder.DropTable(
                name: "DanhMucTaiKhoan");

            migrationBuilder.DropTable(
                name: "Debit");

            migrationBuilder.DropTable(
                name: "DebitCreditCustomer");

            migrationBuilder.DropTable(
                name: "DebitCreditTemplate");

            migrationBuilder.DropTable(
                name: "deNghiHoanUng");

            migrationBuilder.DropTable(
                name: "DeNghiTamUng");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "DNTT_Logistics");

            migrationBuilder.DropTable(
                name: "Duan");

            migrationBuilder.DropTable(
                name: "Duyet_DNTT");

            migrationBuilder.DropTable(
                name: "ExportCostDetail");

            migrationBuilder.DropTable(
                name: "ExportCostRequest");

            migrationBuilder.DropTable(
                name: "GetQuotationNo");

            migrationBuilder.DropTable(
                name: "HBL");

            migrationBuilder.DropTable(
                name: "HBLViTriLoHang");

            migrationBuilder.DropTable(
                name: "HistoryLogs");

            migrationBuilder.DropTable(
                name: "HoaDonDauRa");

            migrationBuilder.DropTable(
                name: "HoaDonDauVao");

            migrationBuilder.DropTable(
                name: "ImportCostRequest");

            migrationBuilder.DropTable(
                name: "Inbound");

            migrationBuilder.DropTable(
                name: "InboundFreight");

            migrationBuilder.DropTable(
                name: "InboundFreight_History");

            migrationBuilder.DropTable(
                name: "InboundFulls");

            migrationBuilder.DropTable(
                name: "IssueReports");

            migrationBuilder.DropTable(
                name: "KTCLCostRequest");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "LenhDieuXe");

            migrationBuilder.DropTable(
                name: "listDebits");

            migrationBuilder.DropTable(
                name: "ListDept");

            migrationBuilder.DropTable(
                name: "localcharges_pt");

            migrationBuilder.DropTable(
                name: "LocalizationResources");

            migrationBuilder.DropTable(
                name: "MARKET");

            migrationBuilder.DropTable(
                name: "MenuNames");

            migrationBuilder.DropTable(
                name: "MoveCustomer");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OutRequests");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "PermissionTemplate");

            migrationBuilder.DropTable(
                name: "Phieuchi");

            migrationBuilder.DropTable(
                name: "Phieuketoan");

            migrationBuilder.DropTable(
                name: "Phieuthu");

            migrationBuilder.DropTable(
                name: "PIC");

            migrationBuilder.DropTable(
                name: "Port");

            migrationBuilder.DropTable(
                name: "Product_Price");

            migrationBuilder.DropTable(
                name: "Product_Price_Detail_ALL");

            migrationBuilder.DropTable(
                name: "Product_Price_Details");

            migrationBuilder.DropTable(
                name: "Quotation");

            migrationBuilder.DropTable(
                name: "QuotationTico");

            migrationBuilder.DropTable(
                name: "RefNo");

            migrationBuilder.DropTable(
                name: "RFQ");

            migrationBuilder.DropTable(
                name: "RFQ_Log");

            migrationBuilder.DropTable(
                name: "Sale");

            migrationBuilder.DropTable(
                name: "SGN_bkno");

            migrationBuilder.DropTable(
                name: "ShippingLine");

            migrationBuilder.DropTable(
                name: "TaxDetail");

            migrationBuilder.DropTable(
                name: "TaxInvoice");

            migrationBuilder.DropTable(
                name: "Terminal");

            migrationBuilder.DropTable(
                name: "TERMINALDEPARTURE");

            migrationBuilder.DropTable(
                name: "Test_getapi");

            migrationBuilder.DropTable(
                name: "Theodoilohang");

            migrationBuilder.DropTable(
                name: "TiepDauNgu");

            migrationBuilder.DropTable(
                name: "TKHQ_Thongtinchitiet");

            migrationBuilder.DropTable(
                name: "TKHQ_Thongtinchunglohang");

            migrationBuilder.DropTable(
                name: "TraLoiYeuCauHQCO");

            migrationBuilder.DropTable(
                name: "TrangThai");

            migrationBuilder.DropTable(
                name: "TruckingCostRequest");

            migrationBuilder.DropTable(
                name: "UserActionLogs");

            migrationBuilder.DropTable(
                name: "UserThemePreferences");

            migrationBuilder.DropTable(
                name: "VesselSpace");

            migrationBuilder.DropTable(
                name: "WordDocuments");

            migrationBuilder.DropTable(
                name: "YeuCauTrucking");

            migrationBuilder.DropTable(
                name: "YeuCauTuVanThuTucHQCO");

            migrationBuilder.DropTable(
                name: "Ykien_HQCO");

            migrationBuilder.DropTable(
                name: "ZaloAccesstoken");

            migrationBuilder.DropTable(
                name: "MBL");

            migrationBuilder.DropTable(
                name: "UserList");

            migrationBuilder.DropTable(
                name: "Job");
        }
    }
}
