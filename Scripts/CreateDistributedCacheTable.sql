-- Bảng dùng cho Distributed Cache (SQL Server), ví dụ: token D/O QR.
-- Chạy script này 1 lần trên database (cùng DB với DefaultConnection).
-- Tham khảo: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed#distributed-sql-server-cache

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DistributedCache]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DistributedCache] (
        [Id] [nvarchar](449) NOT NULL,
        [Value] [varbinary](max) NOT NULL,
        [ExpiresAtTime] [datetimeoffset](7) NOT NULL,
        [SlidingExpirationInSeconds] [bigint] NULL,
        [AbsoluteExpiration] [datetimeoffset](7) NULL,
        CONSTRAINT [pk_DistributedCache_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_DistributedCache_ExpiresAtTime]
        ON [dbo].[DistributedCache] ([ExpiresAtTime] ASC);
END
GO
