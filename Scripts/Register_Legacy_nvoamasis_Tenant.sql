/*
  Đăng ký database gốc nvoamasis vào Acc_Multi_DB (tùy chọn — app cũng tự ghi khi login lần đầu).
  Đổi @SqlUserId / @SqlPassword cho đúng tài khoản SQL bạn dùng để vào nvoamasis.
*/
USE [Acc_Multi_DB];
GO

DECLARE @DatabaseName NVARCHAR(128) = N'nvoamasis';
DECLARE @SqlUserId NVARCHAR(128) = N'sgn-fc-admin';
DECLARE @SqlPassword NVARCHAR(512) = N'qwe123!@#';
DECLARE @ServerName NVARCHAR(256) = N'logisticssoftware.vn';

IF NOT EXISTS (
    SELECT 1 FROM dbo.TenantDatabaseRegistry
    WHERE DatabaseName = @DatabaseName)
BEGIN
    INSERT INTO dbo.TenantDatabaseRegistry
        (TenantId, DatabaseName, ServerName, SqlUserId, SqlPassword, DisplayName, CreatedAtUtc, CreatedByAppUser, IsActive)
    VALUES
        (NEWID(), @DatabaseName, @ServerName, @SqlUserId, @SqlPassword, @DatabaseName, SYSUTCDATETIME(), N'manual-setup', 1);
END
GO
