namespace NVOAMASIS.Models.Chat;

public enum ChatConversationType
{
    Direct = 1,
    Group = 2
}

public enum ChatParticipantRole
{
    Owner = 1,
    Member = 2
}

public enum ChatMessageKind
{
    Text = 1,
    File = 2,
    System = 3
}

/// <summary>Hội thoại 1-1 hoặc nhóm — script Scripts/CreateChatTables.sql</summary>
public class ChatConversation
{
    public Guid Id { get; set; }
    public ChatConversationType Type { get; set; }
    public string? Name { get; set; }

    /// <summary>"{minUserId:N}_{maxUserId:N}" cho hội thoại 1-1; null với nhóm.</summary>
    public string? DirectKey { get; set; }

    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessagePreview { get; set; }
}

public class ChatParticipant
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public ChatParticipantRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastReadAt { get; set; }

    /// <summary>"Xoá hội thoại phía mình": tin có CreatedAt &lt;= ClearedAt không còn hiển thị với user này.</summary>
    public DateTime? ClearedAt { get; set; }

    /// <summary>Ẩn khỏi danh sách cho tới khi có tin mới.</summary>
    public bool IsHidden { get; set; }
}

public class ChatMessageRecord
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderUserId { get; set; }
    public ChatMessageKind Kind { get; set; }
    public string Text { get; set; } = "";
    public string? AttachmentPath { get; set; }
    public string? AttachmentName { get; set; }
    public string? AttachmentContentType { get; set; }
    public long? AttachmentSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Thu hồi: nội dung và file đã bị xoá hẳn, chỉ giữ dòng để giữ thứ tự hội thoại.</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>Bản sao được chuyển tiếp từ hội thoại khác (file đã được sao chép riêng).</summary>
    public bool IsForwarded { get; set; }

    /// <summary>Ghim: tối đa ChatService.MaxPinsPerConversation tin mỗi hội thoại.</summary>
    public DateTime? PinnedAt { get; set; }
    public Guid? PinnedByUserId { get; set; }

    /// <summary>Trả lời tin khác trong cùng hội thoại.</summary>
    public Guid? ReplyToMessageId { get; set; }
}
