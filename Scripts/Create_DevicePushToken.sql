IF OBJECT_ID('dbo.DevicePushToken', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DevicePushToken
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        Token NVARCHAR(512) NOT NULL,
        Platform NVARCHAR(32) NULL,
        UpdatedAtUtc DATETIME2 NOT NULL
    );
    CREATE UNIQUE INDEX IX_DevicePushToken_Token ON dbo.DevicePushToken(Token);
    CREATE INDEX IX_DevicePushToken_UserId ON dbo.DevicePushToken(UserId);
END
GO
