/*
  Chat nội bộ — trả lời, chuyển tiếp, ghim tin nhắn.
  Chạy SAU Scripts/AlterChatTables_AddDelete.sql, trên từng tenant DB. Safe to re-run.
  Tương đương migration 20260925090000_AddChatForwardPin.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.ChatMessages', N'IsForwarded') IS NULL
    ALTER TABLE [dbo].[ChatMessages] ADD [IsForwarded] bit NOT NULL
        CONSTRAINT [DF_ChatMessages_IsForwarded] DEFAULT (0);

IF COL_LENGTH(N'dbo.ChatMessages', N'PinnedAt') IS NULL
    ALTER TABLE [dbo].[ChatMessages] ADD [PinnedAt] datetime2 NULL;

IF COL_LENGTH(N'dbo.ChatMessages', N'PinnedByUserId') IS NULL
    ALTER TABLE [dbo].[ChatMessages] ADD [PinnedByUserId] uniqueidentifier NULL;

-- Không FK: tin gốc có thể bị xoá khi giải tán nhóm (cả hội thoại bị xoá cùng lúc).
IF COL_LENGTH(N'dbo.ChatMessages', N'ReplyToMessageId') IS NULL
    ALTER TABLE [dbo].[ChatMessages] ADD [ReplyToMessageId] uniqueidentifier NULL;

COMMIT TRANSACTION;

-- Index lọc cho thanh tin ghim (tách batch: cột PinnedAt phải tồn tại trước khi compile).
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChatMessages_Pinned' AND object_id = OBJECT_ID(N'dbo.ChatMessages'))
    EXEC(N'CREATE INDEX [IX_ChatMessages_Pinned] ON [dbo].[ChatMessages]([ConversationId], [PinnedAt]) WHERE [PinnedAt] IS NOT NULL');

PRINT N'Chat reply/forward/pin columns added.';
