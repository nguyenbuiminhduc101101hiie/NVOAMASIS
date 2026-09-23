namespace NVOAMASIS.Models.Chat;

/// <summary>Tin nhắn gửi cho UI. Trung lập với người xem — UI tự so SenderUserId với user hiện tại.</summary>
public sealed record ChatMessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderUserId,
    string SenderName,
    ChatMessageKind Kind,
    string Text,
    string? AttachmentName,
    string? AttachmentContentType,
    long? AttachmentSizeBytes,
    DateTime CreatedAt,
    bool IsDeleted,
    bool IsForwarded,
    DateTime? PinnedAt,
    ChatReplyPreview? ReplyTo,
    DateTime? EditedAt)
{
    public bool IsPinned => PinnedAt is not null;

    public bool HasAttachment => Kind == ChatMessageKind.File && !IsDeleted;

    public bool IsImage => HasAttachment &&
        (AttachmentContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ?? false);

    public string AttachmentUrl => $"/api/chat/attachments/{Id}";
}

/// <summary>Trích dẫn tin gốc hiển thị trên tin trả lời. Text rỗng + IsDeleted = tin gốc đã thu hồi.</summary>
public sealed record ChatReplyPreview(
    Guid MessageId,
    string SenderName,
    string Text,
    string? AttachmentName,
    bool IsDeleted);

public sealed record ChatConversationSummary(
    Guid Id,
    ChatConversationType Type,
    string DisplayName,
    string? LastMessagePreview,
    DateTime? LastMessageAt,
    int UnreadCount,
    int MemberCount,
    Guid? OtherUserId);

public sealed record ChatMemberDto(
    Guid UserId,
    string Name,
    string? Department,
    ChatParticipantRole Role,
    DateTime? LastReadAt);

public sealed record ChatUserOption(Guid UserId, string Name, string? Department);

public sealed record ChatMessagePage(IReadOnlyList<ChatMessageDto> Messages, bool HasMore);
