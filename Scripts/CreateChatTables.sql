/*
  Chat nội bộ giữa user (1-1 + nhóm).
  Chạy trên từng tenant DB. Safe to re-run.
  Tương đương migration 20260923100000_AddInternalChat.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.ChatConversations', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ChatConversations](
        [Id] uniqueidentifier NOT NULL,
        [Type] int NOT NULL,
        [Name] nvarchar(200) NULL,
        [DirectKey] nvarchar(80) NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastMessageAt] datetime2 NULL,
        [LastMessagePreview] nvarchar(200) NULL,
        CONSTRAINT [PK_ChatConversations] PRIMARY KEY CLUSTERED ([Id])
    );
    CREATE UNIQUE INDEX [IX_ChatConversations_DirectKey]
        ON [dbo].[ChatConversations]([DirectKey]) WHERE [DirectKey] IS NOT NULL;
END;

IF OBJECT_ID(N'dbo.ChatParticipants', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ChatParticipants](
        [ConversationId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Role] int NOT NULL,
        [JoinedAt] datetime2 NOT NULL,
        [LastReadAt] datetime2 NULL,
        CONSTRAINT [PK_ChatParticipants] PRIMARY KEY CLUSTERED ([ConversationId], [UserId]),
        CONSTRAINT [FK_ChatParticipants_ChatConversations] FOREIGN KEY ([ConversationId])
            REFERENCES [dbo].[ChatConversations]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_ChatParticipants_UserId] ON [dbo].[ChatParticipants]([UserId]);
END;

IF OBJECT_ID(N'dbo.ChatMessages', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ChatMessages](
        [Id] uniqueidentifier NOT NULL,
        [ConversationId] uniqueidentifier NOT NULL,
        [SenderUserId] uniqueidentifier NOT NULL,
        [Kind] int NOT NULL,
        [Text] nvarchar(4000) NOT NULL,
        [AttachmentPath] nvarchar(500) NULL,
        [AttachmentName] nvarchar(260) NULL,
        [AttachmentContentType] nvarchar(150) NULL,
        [AttachmentSizeBytes] bigint NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_ChatMessages_ChatConversations] FOREIGN KEY ([ConversationId])
            REFERENCES [dbo].[ChatConversations]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_ChatMessages_ConversationId_CreatedAt]
        ON [dbo].[ChatMessages]([ConversationId], [CreatedAt]);
END;

COMMIT TRANSACTION;
