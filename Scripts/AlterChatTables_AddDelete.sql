/*
  Chat nội bộ — thu hồi tin nhắn / xoá hội thoại phía mình.
  Chạy SAU Scripts/CreateChatTables.sql, trên từng tenant DB. Safe to re-run.
  Tương đương migration 20260924090000_AddChatDelete.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.ChatMessages', N'DeletedAt') IS NULL
    ALTER TABLE [dbo].[ChatMessages] ADD [DeletedAt] datetime2 NULL;

IF COL_LENGTH(N'dbo.ChatParticipants', N'ClearedAt') IS NULL
    ALTER TABLE [dbo].[ChatParticipants] ADD [ClearedAt] datetime2 NULL;

IF COL_LENGTH(N'dbo.ChatParticipants', N'IsHidden') IS NULL
    ALTER TABLE [dbo].[ChatParticipants] ADD [IsHidden] bit NOT NULL
        CONSTRAINT [DF_ChatParticipants_IsHidden] DEFAULT (0);

COMMIT TRANSACTION;

PRINT N'Chat delete columns added.';
