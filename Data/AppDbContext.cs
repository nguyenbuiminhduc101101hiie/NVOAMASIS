using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Models;
namespace NVOAMASIS.Data
{
    public class AppDbContext (DbContextOptions <AppDbContext> options) : DbContext(options)
    {
        public DbSet<AuthUser> UserList { get; set; }
        public DbSet<In_bound> Inbound { get; set; }
        public DbSet<InboundFull> InboundFulls { get; set; }
        public DbSet<Container_InBound> ContainerRepair { get; set; }
        public DbSet<M_Customer> Customer { get; set; }
        public DbSet<M_Customer_ThamChieu> Customer_ThamChieu { get; set; }
        public DbSet<InboundFreightModel> InboundFreight { get; set; }
        public DbSet<InboundFreight_HistoryModel> InboundFreight_History { get; set; }
        public DbSet<ChargeModel> Charge { get; set; }
        public DbSet<ListDebit> listDebits { get; set; }
        public DbSet<PortModel> Port { get; set; }
        public DbSet<Permission_M> Permissions { get; set; }
        public DbSet<FormMenu> MenuNames { get; set; }
        public DbSet<MARKET> MARKET { get; set; }
        public DbSet<CustomerMarket> CustomerMarket { get; set; }
        public DbSet<CustomerCommondity> CustomerCommondity { get; set; }
        public DbSet<CustomerCode> CustomerCode { get; set; }
        public DbSet<M_Commondity> Commondity { get; set; }
        public DbSet<M_PIC> PIC { get; set; }
        public DbSet<M_Agency> Agency { get; set; }
        public DbSet<M_Booking> Booking { get; set; }
        public DbSet<CustomerReport> CUSTOMERREPORT { get; set; }
        public DbSet<M_Buyer> Buyer { get; set; }
        public DbSet<M_DebitCreditCustomer> DebitCreditCustomer { get; set; }
        public DbSet<M_ListDept> ListDept { get; set; }
        public DbSet<M_CustomerBr> CustomerBr { get; set; }
        public DbSet<CustomerCommondityFileUpload> CustomerCommondityFileUpload { get; set; }
        public DbSet<M_Sale> Sale { get; set; }
        public DbSet<M_LenhCapContRong> LenhCapContRong { get; set; }
        public DbSet<M_CamKetMuonCont_TraRong> CamKetMuonContTraRong { get; set; }
        public DbSet<ParamTrongLuongCont> ParamTrongLuongCont { get; set; }

        public DbSet<Booking> CONTAINEROUTBOUNDNOTIFY_sale { get; set; }
        public DbSet<TrainSchedule> TERMINALDEPARTURE { get; set; }
        public DbSet<ChargeTemplete> ChargeTemplete { get; set; }
        public DbSet<DanhMucTaiKhoan> DanhMucTaiKhoan { get; set; }
        public DbSet<ShippingLine> ShippingLine { get; set; }
        public DbSet<M_Job> Job { get; set; }
        public DbSet<M_HBL> HBL { get; set; }
        public DbSet<M_MBL> MBL { get; set; }
        public DbSet<M_VesselSpace> VesselSpace { get; set; }
        public DbSet<M_Container> Container { get; set; }
        public DbSet<M_Debit> Debit { get; set; }
        public DbSet<M_CongNoHoaDonDauRa> CongNoHoaDonDauRa { get; set; }
        public DbSet<GetBk> SGN_bkno { get; set; }
        public DbSet<Terminal_> Terminal { get; set; }
        public DbSet<Quotation_Tico> QuotationTico { get; set; }
        public DbSet<MoveCustomer> MoveCustomer { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<M_RefNo> RefNo { get; set; }
        public DbSet<M_TiepDauNgu> TiepDauNgu { get; set; }
        public DbSet<M_Credit> Credit { get; set; }
        public DbSet<M_HoaDonDauRa> HoaDonDauRa { get; set; }
        public DbSet<M_HoaDonDauVao> HoaDonDauVao { get; set; }
        public DbSet<M_CongNoHoaDonDauVao> CongNoHoaDonDauVao { get; set; }
        public DbSet<TrangThai> TrangThai { get; set; }
        public DbSet<M_Quotation> Quotation { get; set; }
        public DbSet<M_GetQuoTationNo> GetQuotationNo { get; set; }
        public DbSet<M_Theodoilohang> Theodoilohang { get; set; }
        public DbSet<M_Duan> Duan { get; set; }
        public DbSet<M_HBLViTriLoHang> HBLViTriLoHang { get; set; }
        public DbSet<M_Product_Price> Product_Price { get; set; }
        public DbSet<M_Product_Price_Details> Product_Price_Details { get; set; }

        public DbSet<ZaloAccessToken> ZaloAccesstoken { get; set; }

        public DbSet<M_LocalCharge_pt>localcharges_pt { get; set; }
        public DbSet<M_LenhDieuXe> LenhDieuXe { get; set; }
        public DbSet<M_YeuCauTrucking> YeuCauTrucking { get; set; }
        public DbSet<M_TKHQ_Thongtinchitiet> TKHQ_Thongtinchitiet { get; set; }
        public DbSet<M_TKHQ_Thongtinchunglohang> TKHQ_Thongtinchunglohang { get; set; }
        public DbSet<M_Ykien_HQCO> Ykien_HQCO { get; set; }
        public DbSet<M_YeuCauTuVanTTHQ> YeuCauTuVanThuTucHQCO { get; set; }
        public DbSet<M_TraLoiYeuCauHQCO> TraLoiYeuCauHQCO { get; set; }
        public DbSet<M_Bieugianangha> BieuGiaNangHa { get; set; }
        public DbSet<M_Bieugiarutruot> Bieugiarutruot { get; set; }
        public DbSet<M_BieuGiaKDTV> BieugiaKDTV { get; set; }
        public DbSet<M_BieuGiaKTCL> BieugiaKTCL { get; set; }
        public DbSet<M_BieuGiaLuuKho> Bieugialuukho { get; set; }
        public DbSet<M_DNTU> DeNghiTamUng { get; set; }
        public DbSet<M_DNHU> deNghiHoanUng { get; set; }
        public DbSet<M_ChiTietBieuGiaLuuKho> chiTietBieuGiaLuuKho { get; set; }
        public DbSet<M_PhieuChi> Phieuchi { get; set; }
        public DbSet<M_PhieuThu> Phieuthu { get; set; }
        public DbSet<PermissionTemplate> PermissionTemplate { get; set; }
        public DbSet<M_PhieuKeToan> Phieuketoan { get; set; }
        public DbSet<M_TaxInvoice> TaxInvoice { get; set; }
        public DbSet<M_TaxDetail> TaxDetail { get; set; }
        public DbSet<M_RFQ> RFQ { get; set; }
        public DbSet<M_RFQ_Log> RFQ_Log { get; set; }
        public DbSet<M_DNTT_Logistics> DNTT_Logistics { get; set; }
        public DbSet<M_ExportCostRequest> ExportCostRequest { get; set; }
        public DbSet<M_ExportCostDetail> ExportCostDetail { get; set; }
        public DbSet<M_Duyet_DNTT> Duyet_DNTT { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<M_ImportCostRequest> ImportCostRequest { get; set; }

        public DbSet<M_TruckingCostRequest> TruckingCostRequest { get; set; }
        public DbSet<M_KTCLCostRequest> KTCLCostRequest { get; set; }
        public DbSet<M_Product_Price_detail_ALL> Product_Price_Detail_ALL { get; set; }
        public DbSet<M_CustomCostRequest> CustomCostRequest { get; set; }
        public DbSet<UserActionLog> UserActionLogs { get; set; }
        public DbSet<HistoryLog> HistoryLogs { get; set; }
        public DbSet<M_IssueReports> IssueReports { get; set; }
        public DbSet<M_Currency> Currency { get; set; }
        public DbSet<M_Test_getapi> Test_getapi { get; set; }
        public DbSet<M_CompanyInfo> CompanyInfomation { get; set; }
        public DbSet<M_Info_Company_other> Information_Comapny_Other { get; set; }

        public DbSet<M_CuocCont> CuocCont { get; set; }

        public DbSet<DebitCreditTemplate> DebitCreditTemplate { get; set; }

        public DbSet<WordDocument> WordDocuments { get; set; }

        public DbSet<OutRequest> OutRequests { get; set; }
        public DbSet<AttendanceLog> AttendanceLogs { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        public DbSet<M_ContainerMovement> ContainerMovement { get; set; }
        public DbSet<M_ContainerSeal> ContainerSeal { get; set; }
        public DbSet<M_ContainerDamage> ContainerDamage { get; set; }
        public DbSet<M_GateIn> GateIn { get; set; }
        public DbSet<M_GateOut> GateOut { get; set; }
        public DbSet<M_Stock> Stock { get; set; }
        public DbSet<M_StockGateOut> StockGateOut { get; set; }
        public DbSet<M_StockGateOut_HDS_08042026> StockGateOut_HDS_08042026 { get; set; }
        public DbSet<M_8_3_Arrived> AG_8_3_Arrived { get; set; }
        public DbSet<M_8_3_Exited> AG_8_3_Exited { get; set; }
        public DbSet<M_8_3_Unstuffed> AG_8_3_Unstuffed { get; set; }
        public DbSet<M_8_3_Stuffed> AG_8_3_Stuffed { get; set; }
        public DbSet<M_8_3_VSS_IN_OUT_YARD_Imp> YardMovement_VSS_26040808_Imp { get; set; }
        public DbSet<M_8_3_VSS_IN_OUT_YARD_Exp> YardMovement_VSS_26040808_Exp { get; set; }
        public DbSet<M_8_3_IN_OUT_YARD_1> YardMovement_AMS_26040816_Imp { get; set; }
        public DbSet<M_YardMovement_AMS_26040816_Current_In_Yard2> YardMovement_AMS_26040816_Current_In_Yard2 { get; set; }

        public DbSet<M_ChargeType> ChargeType { get; set; }
        public DbSet<M_TariffHeader> TariffHeader { get; set; }
        public DbSet<M_TariffTier> TariffTier { get; set; }
        public DbSet<M_ShipmentChargeContext> ShipmentChargeContext { get; set; }
        public DbSet<M_ShipmentChargeDateMapping> ShipmentChargeDateMappings { get; set; }

        public DbSet<M_SI> SI { get; set; }
        public DbSet<M_SI_Attachment> SI_Attachment { get; set; }

        public DbSet<M_TransactionTypes> TransactionTypes { get; set; }
        public DbSet<M_TransactionTypeMappings> TransactionTypeMappings { get; set; }
        public DbSet<M_AccountingVouchers> AccountingVouchers { get; set; }
        public DbSet<M_AccountingVoucherLines> AccountingVoucherLines { get; set; }
        public DbSet<M_GeneralLedgerEntries> GeneralLedgerEntries { get; set; }
        public DbSet<M_AccountMapping> AccountMappings { get; set; }
        public DbSet<M_AccountingPeriod> AccountingPeriods { get; set; }
        public DbSet<LocalizationResource> LocalizationResources { get; set; }

        public DbSet<UserThemePreference> UserThemePreferences { get; set; }
        public DbSet<DoQrTokenRecord> DoQrTokens { get; set; }
        public DbSet<ArrivalNoticeQrTokenRecord> ArrivalNoticeQrTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Create unique index on ResourceKey + Culture
            modelBuilder.Entity<LocalizationResource>()
                .HasIndex(e => new { e.ResourceKey, e.Culture })
                .IsUnique();

            // Create unique index on UsrId to ensure one preference per user
            modelBuilder.Entity<UserThemePreference>()
                .HasIndex(e => e.UsrId)
                .IsUnique();

            // Một HBL + Type chỉ có một token QR D/O (bảng DoQrToken, script CreateDoQrTokenTable.sql)
            modelBuilder.Entity<DoQrTokenRecord>()
                .ToTable("DoQrToken");
            modelBuilder.Entity<DoQrTokenRecord>()
                .HasKey(e => e.Token);
            modelBuilder.Entity<DoQrTokenRecord>()
                .HasIndex(e => new { e.HblId, e.Type })
                .IsUnique();

            // Một HBL + Type chỉ có một token QR Arrival Notice (bảng ArrivalNoticeQrToken)
            modelBuilder.Entity<ArrivalNoticeQrTokenRecord>()
                .ToTable("ArrivalNoticeQrToken");
            modelBuilder.Entity<ArrivalNoticeQrTokenRecord>()
                .HasKey(e => e.Token);
            modelBuilder.Entity<ArrivalNoticeQrTokenRecord>()
                .HasIndex(e => new { e.HblId, e.Type })
                .IsUnique();

            modelBuilder.Entity<M_Stock>().ToTable("Stock");
            modelBuilder.Entity<M_StockGateOut>().ToTable("StockGateOut");
            modelBuilder.Entity<M_StockGateOut>()
                .HasIndex(x => x.Container)
                .IsUnique();
            modelBuilder.Entity<M_StockGateOut_HDS_08042026>().ToTable("StockGateOut_HDS_08042026");
            modelBuilder.Entity<M_StockGateOut_HDS_08042026>().Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_StockGateOut_HDS_08042026>().Property(x => x.DateImport).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_Arrived>().ToTable("8_3_Arrived");
            modelBuilder.Entity<M_8_3_Arrived>().Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_Arrived>().Property(x => x.DateImport).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_Exited>().ToTable("8_3_Exited");
            modelBuilder.Entity<M_8_3_Exited>().Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_Exited>().Property(x => x.DateImport).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_Unstuffed>().ToTable("8_3_Unstuffed");
            modelBuilder.Entity<M_8_3_Unstuffed>().Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_Unstuffed>().Property(x => x.DateImport).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_Stuffed>().ToTable("8_3_Stuffed");
            modelBuilder.Entity<M_8_3_Stuffed>().Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_Stuffed>().Property(x => x.DateImport).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_VSS_IN_OUT_YARD_Imp>().ToTable("YardMovement_VSS_26040808_Imp");
            modelBuilder.Entity<M_8_3_VSS_IN_OUT_YARD_Imp>().Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_VSS_IN_OUT_YARD_Imp>().Property(x => x.DateImport).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_VSS_IN_OUT_YARD_Exp>().ToTable("YardMovement_VSS_26040808_Exp");
            modelBuilder.Entity<M_8_3_VSS_IN_OUT_YARD_Exp>().Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_VSS_IN_OUT_YARD_Exp>().Property(x => x.DateImport).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_IN_OUT_YARD_1>().ToTable("YardMovement_AMS_26040816_Imp");
            modelBuilder.Entity<M_8_3_IN_OUT_YARD_1>().Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_8_3_IN_OUT_YARD_1>().Property(x => x.DateImport).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_YardMovement_AMS_26040816_Current_In_Yard2>().ToTable("YardMovement_AMS_26040816_Current_In_Yard2");
            modelBuilder.Entity<M_YardMovement_AMS_26040816_Current_In_Yard2>().Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_YardMovement_AMS_26040816_Current_In_Yard2>().Property(x => x.DateImport).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity<M_ShipmentChargeContext>().ToTable("ShipmentChargeContext");
            modelBuilder.Entity<M_ShipmentChargeDateMapping>().ToTable("ShipmentChargeDateMapping");
            modelBuilder.Entity<M_ShipmentChargeDateMapping>().HasIndex(x => x.TargetField).IsUnique();
            modelBuilder.Entity<M_ShipmentChargeDateMapping>().Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            modelBuilder.Entity<M_SI_Attachment>().ToTable("SI_Attachment");
            modelBuilder.Entity<M_SI_Attachment>()
                .HasOne<M_SI>()
                .WithMany()
                .HasForeignKey(x => x.SIID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<M_TransactionTypeMappings>()
                .HasOne<M_TransactionTypes>()
                .WithMany()
                .HasForeignKey(x => x.TransactionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<M_AccountingPeriod>()
                .Property(x => x.PeriodCode)
                .UseIdentityColumn();

            modelBuilder.Entity<M_AccountingVoucherLines>()
                .HasOne<M_AccountingVouchers>()
                .WithMany()
                .HasForeignKey(x => x.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
