-- Tạo / cập nhật bảng UserThemePreferences (theme + font size theo account).
-- Chạy trên DB app đang dùng (DefaultConnection = nvoamasis), KHÔNG dùng LMS.
-- Giá trị DEFAULT trùng NVOAMASIS.Models.ThemeDefaults.

IF OBJECT_ID(N'[dbo].[UserThemePreferences]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserThemePreferences](
        [PreferenceId] [uniqueidentifier] NOT NULL,
        [UsrId] [uniqueidentifier] NOT NULL,
        [NavMenuBackgroundColor] [nvarchar](50) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_NavMenuBackgroundColor] DEFAULT N'#78BCC4',
        [NavMenuTextColor] [nvarchar](50) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_NavMenuTextColor] DEFAULT N'#333333',
        [MainLayoutBackgroundColor] [nvarchar](50) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_MainLayoutBackgroundColor] DEFAULT N'#002C3E',
        [FontHeaderH6] [nvarchar](20) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_FontHeaderH6] DEFAULT N'15px',
        [FontTieuDe] [nvarchar](20) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_FontTieuDe] DEFAULT N'9px',
        [FontNoiDung] [nvarchar](20) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_FontNoiDung] DEFAULT N'12px',
        [FontNoiDungLuoi] [nvarchar](20) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_FontNoiDungLuoi] DEFAULT N'9px',
        [FontNavMenu] [nvarchar](20) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_FontNavMenu] DEFAULT N'12px',
        [CreatedAt] [datetime2](7) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt] [datetime2](7) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_UpdatedAt] DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_UserThemePreferences] PRIMARY KEY CLUSTERED ([PreferenceId] ASC)
    );

    ALTER TABLE [dbo].[UserThemePreferences] WITH CHECK
        ADD CONSTRAINT [FK_UserThemePreferences_UserList_UsrId]
        FOREIGN KEY([UsrId]) REFERENCES [dbo].[UserList] ([UsrId]) ON DELETE CASCADE;

    CREATE UNIQUE NONCLUSTERED INDEX [IX_UserThemePreferences_UsrId]
        ON [dbo].[UserThemePreferences] ([UsrId] ASC);
END
GO

-- Nếu bảng đã có sẵn: chỉ thêm cột còn thiếu
IF OBJECT_ID(N'[dbo].[UserThemePreferences]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.UserThemePreferences', 'NavMenuTextColor') IS NULL
        ALTER TABLE [dbo].[UserThemePreferences]
            ADD [NavMenuTextColor] NVARCHAR(50) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_NavMenuTextColor] DEFAULT N'#333333';

    IF COL_LENGTH('dbo.UserThemePreferences', 'FontHeaderH6') IS NULL
        ALTER TABLE [dbo].[UserThemePreferences]
            ADD [FontHeaderH6] NVARCHAR(20) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_FontHeaderH6] DEFAULT N'15px';

    IF COL_LENGTH('dbo.UserThemePreferences', 'FontTieuDe') IS NULL
        ALTER TABLE [dbo].[UserThemePreferences]
            ADD [FontTieuDe] NVARCHAR(20) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_FontTieuDe] DEFAULT N'9px';

    IF COL_LENGTH('dbo.UserThemePreferences', 'FontNoiDung') IS NULL
        ALTER TABLE [dbo].[UserThemePreferences]
            ADD [FontNoiDung] NVARCHAR(20) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_FontNoiDung] DEFAULT N'12px';

    IF COL_LENGTH('dbo.UserThemePreferences', 'FontNoiDungLuoi') IS NULL
        ALTER TABLE [dbo].[UserThemePreferences]
            ADD [FontNoiDungLuoi] NVARCHAR(20) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_FontNoiDungLuoi] DEFAULT N'9px';

    IF COL_LENGTH('dbo.UserThemePreferences', 'FontNavMenu') IS NULL
        ALTER TABLE [dbo].[UserThemePreferences]
            ADD [FontNavMenu] NVARCHAR(20) NOT NULL
            CONSTRAINT [DF_UserThemePreferences_FontNavMenu] DEFAULT N'12px';
END
GO
