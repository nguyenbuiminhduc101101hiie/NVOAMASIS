USE [LMS]
GO

/****** Object:  Table [dbo].[UserThemePreferences]    Script Date: 24/02/2026 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Giá trị DEFAULT bên dưới phải trùng với NVOAMASIS.Models.ThemeDefaults (ThemeDefaults.cs). Chỉnh màu mặc định ở 1 chỗ: ThemeDefaults.cs.
CREATE TABLE [dbo].[UserThemePreferences](
	[PreferenceId] [uniqueidentifier] NOT NULL,
	[UsrId] [uniqueidentifier] NOT NULL,
	[NavMenuBackgroundColor] [nvarchar](50) NOT NULL DEFAULT '#78BCC4',
	[MainLayoutBackgroundColor] [nvarchar](50) NOT NULL DEFAULT '#002C3E',
	[CreatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
	[UpdatedAt] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
 CONSTRAINT [PK_UserThemePreferences] PRIMARY KEY CLUSTERED 
(
	[PreferenceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[UserThemePreferences]  WITH CHECK ADD  CONSTRAINT [FK_UserThemePreferences_UserList_UsrId] FOREIGN KEY([UsrId])
REFERENCES [dbo].[UserList] ([UsrId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[UserThemePreferences] CHECK CONSTRAINT [FK_UserThemePreferences_UserList_UsrId]
GO

-- Create unique index on UsrId to ensure one preference per user
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserThemePreferences_UsrId]
ON [dbo].[UserThemePreferences] ([UsrId] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
