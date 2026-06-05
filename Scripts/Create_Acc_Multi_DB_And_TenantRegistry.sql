/*
  Registry database for multi-tenant NVOAMASIS.
  Server: logisticssoftware.vn
  Run with sa (or equivalent) — creates Acc_Multi_DB and TenantDatabaseRegistry.
*/
USE master;
GO

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'Acc_Multi_DB')
BEGIN
    CREATE DATABASE [Acc_Multi_DB];
END
GO

USE [Acc_Multi_DB];
GO

IF OBJECT_ID(N'dbo.TenantDatabaseRegistry', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TenantDatabaseRegistry]
    (
        [TenantId]          UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_TenantDatabaseRegistry_TenantId] DEFAULT (NEWID()),
        [DatabaseName]      NVARCHAR(128)    NOT NULL,
        [ServerName]        NVARCHAR(256)    NOT NULL CONSTRAINT [DF_TenantDatabaseRegistry_ServerName] DEFAULT (N'logisticssoftware.vn'),
        [SqlUserId]         NVARCHAR(128)    NOT NULL,
        [SqlPassword]       NVARCHAR(512)    NOT NULL,
        [DisplayName]       NVARCHAR(256)    NULL,
        [CreatedAtUtc]      DATETIME2(7)     NOT NULL CONSTRAINT [DF_TenantDatabaseRegistry_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [CreatedByAppUser]  NVARCHAR(128)    NULL,
        [IsActive]          BIT              NOT NULL CONSTRAINT [DF_TenantDatabaseRegistry_IsActive] DEFAULT (1),
        CONSTRAINT [PK_TenantDatabaseRegistry] PRIMARY KEY CLUSTERED ([TenantId] ASC),
        CONSTRAINT [UQ_TenantDatabaseRegistry_DatabaseName] UNIQUE ([DatabaseName])
    );

    CREATE NONCLUSTERED INDEX [IX_TenantDatabaseRegistry_IsActive]
        ON [dbo].[TenantDatabaseRegistry] ([IsActive] ASC)
        INCLUDE ([DatabaseName], [ServerName], [SqlUserId], [SqlPassword]);
END
GO

/*
  Đăng ký DB mới qua app (sa): BACKUP/RESTORE template -> CREATE/ALTER LOGIN -> db_owner trên DB tenant.
*/
