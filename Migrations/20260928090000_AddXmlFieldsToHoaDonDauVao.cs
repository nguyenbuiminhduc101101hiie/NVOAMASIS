using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <summary>Thêm field XML hóa đơn điện tử cho Hóa đơn đầu vào (5.15) — tương đương Scripts/AlterHoaDonDauVao_AddXml.sql</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260928090000_AddXmlFieldsToHoaDonDauVao")]
    public partial class AddXmlFieldsToHoaDonDauVao : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'mausohoadon') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [mausohoadon] nvarchar(50) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'kyhieuhoadon') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [kyhieuhoadon] nvarchar(50) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'loaihoadon') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [loaihoadon] nvarchar(255) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'hinhthucthanhtoan') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [hinhthucthanhtoan] nvarchar(255) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'xmlDocId') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [xmlDocId] nvarchar(100) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tennguoiban') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [tennguoiban] nvarchar(500) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'mstnguoiban') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [mstnguoiban] nvarchar(50) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'diachinguoiban') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [diachinguoiban] nvarchar(1000) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tennguoimua') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [tennguoimua] nvarchar(500) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'mstnguoimua') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [mstnguoimua] nvarchar(50) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'diachinguoimua') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [diachinguoimua] nvarchar(1000) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tienthue') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [tienthue] float NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'chietkhau') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [chietkhau] float NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tienbangchu') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [tienbangchu] nvarchar(500) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'billno') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [billno] nvarchar(100) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'vesselvoyage') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [vesselvoyage] nvarchar(255) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'pol') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [pol] nvarchar(255) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'pod') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [pod] nvarchar(255) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'ghichu') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [ghichu] nvarchar(max) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'nguonimport') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [nguonimport] nvarchar(20) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tenfile') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [tenfile] nvarchar(500) NULL;
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'xmlcontent') IS NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] ADD [xmlcontent] nvarchar(max) NULL;
                """);
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HoaDonDauVao_xmlDocId' AND object_id = OBJECT_ID(N'dbo.HoaDonDauVao'))
                    EXEC(N'CREATE INDEX [IX_HoaDonDauVao_xmlDocId] ON [dbo].[HoaDonDauVao] ([xmlDocId]) WHERE [xmlDocId] IS NOT NULL');
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HoaDonDauVao_xmlDocId' AND object_id = OBJECT_ID(N'dbo.HoaDonDauVao'))
                    DROP INDEX [IX_HoaDonDauVao_xmlDocId] ON [dbo].[HoaDonDauVao];
                """);
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'mausohoadon') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [mausohoadon];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'kyhieuhoadon') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [kyhieuhoadon];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'loaihoadon') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [loaihoadon];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'hinhthucthanhtoan') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [hinhthucthanhtoan];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'xmlDocId') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [xmlDocId];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tennguoiban') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [tennguoiban];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'mstnguoiban') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [mstnguoiban];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'diachinguoiban') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [diachinguoiban];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tennguoimua') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [tennguoimua];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'mstnguoimua') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [mstnguoimua];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'diachinguoimua') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [diachinguoimua];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tienthue') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [tienthue];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'chietkhau') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [chietkhau];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tienbangchu') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [tienbangchu];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'billno') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [billno];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'vesselvoyage') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [vesselvoyage];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'pol') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [pol];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'pod') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [pod];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'ghichu') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [ghichu];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'nguonimport') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [nguonimport];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tenfile') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [tenfile];
                IF COL_LENGTH(N'dbo.HoaDonDauVao', N'xmlcontent') IS NOT NULL
                    ALTER TABLE [dbo].[HoaDonDauVao] DROP COLUMN [xmlcontent];
                """);
        }
    }
}
