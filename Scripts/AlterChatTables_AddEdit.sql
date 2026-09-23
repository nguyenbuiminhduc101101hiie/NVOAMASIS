/*
  Chat nội bộ — chỉnh sửa tin nhắn.
  Chạy SAU Scripts/AlterChatTables_AddForwardPin.sql, trên từng tenant DB. Safe to re-run.
  Tương đương migration 20260926090000_AddChatEdit.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.ChatMessages', N'EditedAt') IS NULL
    ALTER TABLE [dbo].[ChatMessages] ADD [EditedAt] datetime2 NULL;

COMMIT TRANSACTION;

PRINT N'Chat edit column added.';
