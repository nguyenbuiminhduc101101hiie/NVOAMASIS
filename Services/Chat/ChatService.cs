using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models.Chat;

namespace NVOAMASIS.Services.Chat;

/// <summary>Lỗi nghiệp vụ chat; Message là key localization (xem Scripts/Localization_Chat.sql).</summary>
public sealed class ChatException(string code) : Exception(code);

/// <summary>
/// Nghiệp vụ chat nội bộ. Mọi method kiểm tra caller là thành viên hội thoại.
/// Dùng DbContext ngắn hạn cho mỗi thao tác vì event realtime có thể đến giữa lúc component đang truy vấn.
/// </summary>
public sealed class ChatService(
    IDbContextFactory<AppDbContext> dbFactory,
    ChatNotifier notifier,
    ChatFileStorage fileStorage,
    ILogger<ChatService> logger)
{
    public const int PageSize = 50;
    public const int MaxTextLength = 4000;
    private const int MaxJumpWindow = 500;
    private const int PreviewLength = 200;
    private static readonly TimeSpan RecallWindow = TimeSpan.FromHours(24);
    private static readonly TimeSpan EditWindow = TimeSpan.FromHours(24);

    /// <summary>LastMessagePreview rỗng = tin cuối đã bị thu hồi; UI hiển thị chuỗi localize "chat_message_recalled".</summary>
    public const string RecalledPreview = "";

    public const int MaxPinsPerConversation = 3;
    public const int MaxForwardTargets = 10;

    // ---------- Danh bạ ----------

    public async Task<IReadOnlyList<ChatUserOption>> GetUserDirectoryAsync(ChatCaller caller)
    {
        await using var db = dbFactory.CreateDbContext();
        var users = await db.UserList.AsNoTracking()
            .Where(u => u.UsrId != caller.UserId)
            .Select(u => new { u.UsrId, u.Name, u.Usr, u.Department })
            .ToListAsync();

        return users
            .Select(u => new ChatUserOption(u.UsrId, DisplayName(u.Name, u.Usr), u.Department))
            .OrderBy(u => u.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    // ---------- Hội thoại ----------

    public async Task<IReadOnlyList<ChatConversationSummary>> GetConversationsAsync(ChatCaller caller)
    {
        await using var db = dbFactory.CreateDbContext();
        var me = caller.UserId;

        // Hội thoại 1-1 chưa có tin chỉ hiện với người tạo, tránh làm rối danh sách của người kia.
        var conversations = await (
                from p in db.ChatParticipants
                join c in db.ChatConversations on p.ConversationId equals c.Id
                where p.UserId == me && !p.IsHidden &&
                      (c.LastMessageAt != null || c.Type == ChatConversationType.Group || c.CreatedByUserId == me)
                select new { Conversation = c, p.ClearedAt })
            .AsNoTracking()
            .ToListAsync();

        if (conversations.Count == 0)
            return [];

        var memberRows = await (
                from mine in db.ChatParticipants
                join other in db.ChatParticipants on mine.ConversationId equals other.ConversationId
                join u in db.UserList on other.UserId equals u.UsrId into users
                from u in users.DefaultIfEmpty()
                where mine.UserId == me
                select new { other.ConversationId, other.UserId, Name = u == null ? null : u.Name, Usr = u == null ? null : u.Usr })
            .AsNoTracking()
            .ToListAsync();
        var membersByConversation = memberRows.ToLookup(x => x.ConversationId);

        var unreadByConversation = await UnreadQuery(db, me)
            .GroupBy(m => m.ConversationId)
            .Select(g => new { ConversationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ConversationId, x => x.Count);

        return conversations
            .Select(row =>
            {
                var c = row.Conversation;
                // Tin cuối nằm trong phần lịch sử user đã xoá → không hiện preview/thời gian.
                var cleared = row.ClearedAt is { } clearedAt && (c.LastMessageAt is null || c.LastMessageAt <= clearedAt);
                var members = membersByConversation[c.Id].ToList();
                var other = c.Type == ChatConversationType.Direct
                    ? members.FirstOrDefault(m => m.UserId != me)
                    : null;
                var displayName = c.Type == ChatConversationType.Direct
                    ? DisplayName(other?.Name, other?.Usr)
                    : c.Name ?? "";

                return new ChatConversationSummary(
                    c.Id,
                    c.Type,
                    displayName,
                    cleared ? null : c.LastMessagePreview,
                    cleared ? null : c.LastMessageAt,
                    unreadByConversation.GetValueOrDefault(c.Id),
                    members.Count,
                    other?.UserId);
            })
            .OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue)
            .ToList();
    }

    public async Task<int> GetTotalUnreadAsync(ChatCaller caller)
    {
        await using var db = dbFactory.CreateDbContext();
        return await UnreadQuery(db, caller.UserId).CountAsync();
    }

    public async Task<Guid> GetOrCreateDirectAsync(ChatCaller caller, Guid otherUserId)
    {
        if (otherUserId == caller.UserId)
            throw new ChatException("chat_user_not_found");

        await using var db = dbFactory.CreateDbContext();
        if (!await db.UserList.AnyAsync(u => u.UsrId == otherUserId))
            throw new ChatException("chat_user_not_found");

        var directKey = DirectKey(caller.UserId, otherUserId);
        var existingId = await FindDirectAsync(db, directKey);
        if (existingId is { } id)
        {
            // Đã "xoá phía mình" trước đó → hiện lại, lịch sử cũ vẫn ẩn theo ClearedAt.
            await db.ChatParticipants
                .Where(p => p.ConversationId == id && p.UserId == caller.UserId && p.IsHidden)
                .ExecuteUpdateAsync(set => set.SetProperty(p => p.IsHidden, false));
            return id;
        }

        var now = DateTime.UtcNow;
        var conversation = new ChatConversation
        {
            Id = Guid.NewGuid(),
            Type = ChatConversationType.Direct,
            DirectKey = directKey,
            CreatedByUserId = caller.UserId,
            CreatedAt = now
        };
        db.ChatConversations.Add(conversation);
        db.ChatParticipants.AddRange(
            NewParticipant(conversation.Id, caller.UserId, ChatParticipantRole.Member, now),
            NewParticipant(conversation.Id, otherUserId, ChatParticipantRole.Member, now));

        try
        {
            await db.SaveChangesAsync();
            return conversation.Id;
        }
        catch (DbUpdateException)
        {
            // Hai người mở chat với nhau cùng lúc → unique DirectKey chặn bản thứ hai; dùng bản đã có.
            await using var retryDb = dbFactory.CreateDbContext();
            return await FindDirectAsync(retryDb, directKey) ?? throw new ChatException("chat_error_generic");
        }
    }

    public async Task<Guid> CreateGroupAsync(ChatCaller caller, string name, IEnumerable<Guid> memberIds)
    {
        var groupName = (name ?? "").Trim();
        if (groupName.Length == 0)
            throw new ChatException("chat_group_name_required");
        if (groupName.Length > 200)
            groupName = groupName[..200];

        var members = memberIds.Where(id => id != caller.UserId).Distinct().ToList();
        if (members.Count < 2)
            throw new ChatException("chat_group_min_members");

        await using var db = dbFactory.CreateDbContext();
        await EnsureUsersExistAsync(db, members);

        var now = DateTime.UtcNow;
        var conversation = new ChatConversation
        {
            Id = Guid.NewGuid(),
            Type = ChatConversationType.Group,
            Name = groupName,
            CreatedByUserId = caller.UserId,
            CreatedAt = now
        };
        db.ChatConversations.Add(conversation);
        db.ChatParticipants.Add(NewParticipant(conversation.Id, caller.UserId, ChatParticipantRole.Owner, now));
        db.ChatParticipants.AddRange(members.Select(m => NewParticipant(conversation.Id, m, ChatParticipantRole.Member, now)));
        await db.SaveChangesAsync();

        await AddSystemMessageAsync(db, caller.TenantKey, conversation.Id, caller.UserId, "chat_sys_group_created", now);
        notifier.Publish(caller.TenantKey, members.Append(caller.UserId), new ChatConversationChanged(conversation.Id));
        return conversation.Id;
    }

    public async Task<IReadOnlyList<ChatMemberDto>> GetMembersAsync(ChatCaller caller, Guid conversationId)
    {
        await using var db = dbFactory.CreateDbContext();
        await EnsureMemberAsync(db, caller, conversationId);

        var rows = await (
                from p in db.ChatParticipants
                join u in db.UserList on p.UserId equals u.UsrId into users
                from u in users.DefaultIfEmpty()
                where p.ConversationId == conversationId
                select new { p.UserId, p.Role, p.LastReadAt, Name = u == null ? null : u.Name, Usr = u == null ? null : u.Usr, Department = u == null ? null : u.Department })
            .AsNoTracking()
            .ToListAsync();

        return rows
            .Select(r => new ChatMemberDto(r.UserId, DisplayName(r.Name, r.Usr), r.Department, r.Role, r.LastReadAt))
            .OrderBy(m => m.Role)
            .ThenBy(m => m.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public async Task RenameGroupAsync(ChatCaller caller, Guid conversationId, string name)
    {
        var groupName = (name ?? "").Trim();
        if (groupName.Length == 0)
            throw new ChatException("chat_group_name_required");
        if (groupName.Length > 200)
            groupName = groupName[..200];

        await using var db = dbFactory.CreateDbContext();
        var conversation = await EnsureGroupOwnerAsync(db, caller, conversationId);
        conversation.Name = groupName;
        await db.SaveChangesAsync();

        await AddSystemMessageAsync(db, caller.TenantKey, conversationId, caller.UserId, "chat_sys_group_renamed", DateTime.UtcNow);
        await PublishToMembersAsync(db, caller.TenantKey, conversationId, new ChatConversationChanged(conversationId));
    }

    public async Task AddMembersAsync(ChatCaller caller, Guid conversationId, IEnumerable<Guid> memberIds)
    {
        await using var db = dbFactory.CreateDbContext();
        await EnsureGroupOwnerAsync(db, caller, conversationId);

        var current = await db.ChatParticipants
            .Where(p => p.ConversationId == conversationId)
            .Select(p => p.UserId)
            .ToListAsync();
        var toAdd = memberIds.Distinct().Except(current).ToList();
        if (toAdd.Count == 0)
            return;

        await EnsureUsersExistAsync(db, toAdd);

        var now = DateTime.UtcNow;
        db.ChatParticipants.AddRange(toAdd.Select(id => NewParticipant(conversationId, id, ChatParticipantRole.Member, now)));
        await db.SaveChangesAsync();

        await AddSystemMessageAsync(db, caller.TenantKey, conversationId, caller.UserId, "chat_sys_members_added", now);
        await PublishToMembersAsync(db, caller.TenantKey, conversationId, new ChatConversationChanged(conversationId));
    }

    public async Task RemoveMemberAsync(ChatCaller caller, Guid conversationId, Guid memberId)
    {
        if (memberId == caller.UserId)
            throw new ChatException("chat_cannot_remove_self");

        await using var db = dbFactory.CreateDbContext();
        await EnsureGroupOwnerAsync(db, caller, conversationId);

        var participant = await db.ChatParticipants
            .SingleOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == memberId);
        if (participant is null)
            return;

        db.ChatParticipants.Remove(participant);
        await db.SaveChangesAsync();

        await AddSystemMessageAsync(db, caller.TenantKey, conversationId, caller.UserId, "chat_sys_member_removed", DateTime.UtcNow);
        var evt = new ChatConversationChanged(conversationId);
        await PublishToMembersAsync(db, caller.TenantKey, conversationId, evt);
        notifier.Publish(caller.TenantKey, [memberId], evt);
    }

    public async Task LeaveGroupAsync(ChatCaller caller, Guid conversationId)
    {
        await using var db = dbFactory.CreateDbContext();
        var conversation = await EnsureMemberAsync(db, caller, conversationId);
        if (conversation.Type != ChatConversationType.Group)
            throw new ChatException("chat_group_only");

        var participants = await db.ChatParticipants
            .Where(p => p.ConversationId == conversationId)
            .ToListAsync();
        var me = participants.Single(p => p.UserId == caller.UserId);
        db.ChatParticipants.Remove(me);

        // Owner rời → chuyển quyền cho người tham gia sớm nhất; nhóm rỗng vẫn giữ lịch sử.
        if (me.Role == ChatParticipantRole.Owner)
        {
            var nextOwner = participants
                .Where(p => p.UserId != caller.UserId)
                .OrderBy(p => p.JoinedAt)
                .FirstOrDefault();
            if (nextOwner is not null)
                nextOwner.Role = ChatParticipantRole.Owner;
        }
        await db.SaveChangesAsync();

        await AddSystemMessageAsync(db, caller.TenantKey, conversationId, caller.UserId, "chat_sys_member_left", DateTime.UtcNow);
        var evt = new ChatConversationChanged(conversationId);
        await PublishToMembersAsync(db, caller.TenantKey, conversationId, evt);
        notifier.Publish(caller.TenantKey, [caller.UserId], evt);
    }

    // ---------- Tin nhắn ----------

    /// <summary>Trang tin nhắn, cũ → mới. <paramref name="before"/> = CreatedAt của tin cũ nhất đang hiển thị.</summary>
    public async Task<ChatMessagePage> GetMessagesAsync(ChatCaller caller, Guid conversationId, DateTime? before = null)
    {
        await using var db = dbFactory.CreateDbContext();
        var clearedAt = await GetClearedAtAsync(db, caller, conversationId);

        var query = VisibleMessages(db, conversationId, clearedAt);
        if (before is { } cursor)
            query = query.Where(m => m.CreatedAt < cursor);

        var rows = await ProjectWithSender(db, query.OrderByDescending(m => m.CreatedAt).Take(PageSize + 1))
            .ToListAsync();

        var hasMore = rows.Count > PageSize;
        var messages = rows.Take(PageSize).Reverse().ToList();
        return new ChatMessagePage(messages, hasMore);
    }

    /// <summary>Tải từ tin <paramref name="messageId"/> tới mới nhất (tối đa 500) để nhảy tới kết quả tìm kiếm.</summary>
    public async Task<ChatMessagePage> GetMessagesFromAsync(ChatCaller caller, Guid conversationId, Guid messageId)
    {
        await using var db = dbFactory.CreateDbContext();
        var clearedAt = await GetClearedAtAsync(db, caller, conversationId);
        var visible = VisibleMessages(db, conversationId, clearedAt);

        var target = await visible
            .Where(m => m.Id == messageId)
            .Select(m => (DateTime?)m.CreatedAt)
            .SingleOrDefaultAsync()
            ?? throw new ChatException("chat_message_not_found");

        var rows = await ProjectWithSender(db, visible
                .Where(m => m.CreatedAt >= target)
                .OrderBy(m => m.CreatedAt)
                .Take(MaxJumpWindow))
            .ToListAsync();

        var hasMore = await visible.AnyAsync(m => m.CreatedAt < target);
        return new ChatMessagePage(rows, hasMore);
    }

    public async Task<IReadOnlyList<ChatMessageDto>> SearchMessagesAsync(ChatCaller caller, Guid conversationId, string keyword)
    {
        var term = (keyword ?? "").Trim();
        if (term.Length == 0)
            return [];

        await using var db = dbFactory.CreateDbContext();
        var clearedAt = await GetClearedAtAsync(db, caller, conversationId);

        var query = VisibleMessages(db, conversationId, clearedAt)
            .Where(m => m.Kind != ChatMessageKind.System &&
                        m.DeletedAt == null &&
                        (m.Text.Contains(term) || (m.AttachmentName != null && m.AttachmentName.Contains(term))))
            .OrderByDescending(m => m.CreatedAt)
            .Take(PageSize);

        return await ProjectWithSender(db, query).ToListAsync();
    }

    public async Task<ChatMessageDto> SendTextAsync(ChatCaller caller, Guid conversationId, string text, Guid? replyToMessageId = null)
    {
        var normalized = (text ?? "").Trim();
        if (normalized.Length == 0)
            throw new ChatException("chat_message_empty");
        if (normalized.Length > MaxTextLength)
            throw new ChatException("chat_message_too_long");

        await using var db = dbFactory.CreateDbContext();
        var conversation = await EnsureMemberAsync(db, caller, conversationId, tracking: true);
        await EnsureReplyTargetAsync(db, conversationId, replyToMessageId);

        var message = new ChatMessageRecord
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderUserId = caller.UserId,
            Kind = ChatMessageKind.Text,
            Text = normalized,
            ReplyToMessageId = replyToMessageId,
            CreatedAt = DateTime.UtcNow
        };

        return await SaveAndPublishAsync(db, caller, conversation, message, Preview(normalized));
    }

    public async Task<ChatMessageDto> SendAttachmentAsync(
        ChatCaller caller,
        Guid conversationId,
        string fileName,
        string? contentType,
        long sizeBytes,
        Stream content,
        Guid? replyToMessageId = null,
        CancellationToken cancellationToken = default)
    {
        if (sizeBytes > ChatFileStorage.MaxAttachmentBytes)
            throw new ChatException("chat_attachment_too_large");
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ChatException("chat_attachment_name_required");

        await using var db = dbFactory.CreateDbContext();
        var conversation = await EnsureMemberAsync(db, caller, conversationId, tracking: true);
        await EnsureReplyTargetAsync(db, conversationId, replyToMessageId);

        var messageId = Guid.NewGuid();
        string storedPath;
        try
        {
            storedPath = await fileStorage.SaveAsync(caller.TenantKey, conversationId, messageId, fileName, content, cancellationToken);
        }
        catch (ArgumentException)
        {
            throw new ChatException("chat_attachment_too_large");
        }

        var safeName = Path.GetFileName(fileName);
        var message = new ChatMessageRecord
        {
            Id = messageId,
            ConversationId = conversationId,
            SenderUserId = caller.UserId,
            Kind = ChatMessageKind.File,
            Text = "",
            AttachmentPath = storedPath,
            AttachmentName = safeName.Length > 260 ? safeName[^260..] : safeName,
            AttachmentContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            AttachmentSizeBytes = sizeBytes,
            ReplyToMessageId = replyToMessageId,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            return await SaveAndPublishAsync(db, caller, conversation, message, Preview("📎 " + message.AttachmentName));
        }
        catch
        {
            fileStorage.TryDelete(storedPath);
            throw;
        }
    }

    /// <summary>Đặt LastReadAt = tin mới nhất. Trả về false nếu không có gì thay đổi.</summary>
    public async Task<bool> MarkReadAsync(ChatCaller caller, Guid conversationId)
    {
        await using var db = dbFactory.CreateDbContext();
        var participant = await db.ChatParticipants
            .SingleOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == caller.UserId)
            ?? throw new ChatException("chat_not_member");

        var latest = await db.ChatMessages.AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .MaxAsync(m => (DateTime?)m.CreatedAt);
        if (latest is null || participant.LastReadAt >= latest)
            return false;

        participant.LastReadAt = latest;
        await db.SaveChangesAsync();

        await PublishToMembersAsync(db, caller.TenantKey, conversationId,
            new ChatReadUpdated(conversationId, caller.UserId, latest.Value));
        return true;
    }

    public async Task NotifyTypingAsync(ChatCaller caller, Guid conversationId, string userName)
    {
        await using var db = dbFactory.CreateDbContext();
        var memberIds = await db.ChatParticipants.AsNoTracking()
            .Where(p => p.ConversationId == conversationId)
            .Select(p => p.UserId)
            .ToListAsync();
        if (!memberIds.Contains(caller.UserId))
            return;

        notifier.Publish(caller.TenantKey, memberIds.Where(id => id != caller.UserId),
            new ChatTyping(conversationId, caller.UserId, userName));
    }

    // ---------- Chỉnh sửa ----------

    /// <summary>Người gửi sửa tin chữ của mình trong 24 giờ. Không lưu nội dung cũ.</summary>
    public async Task<ChatMessageDto> EditMessageAsync(ChatCaller caller, Guid messageId, string text)
    {
        var normalized = (text ?? "").Trim();
        if (normalized.Length == 0)
            throw new ChatException("chat_message_empty");
        if (normalized.Length > MaxTextLength)
            throw new ChatException("chat_message_too_long");

        await using var db = dbFactory.CreateDbContext();
        var message = await db.ChatMessages.SingleOrDefaultAsync(m => m.Id == messageId)
            ?? throw new ChatException("chat_message_not_found");

        if (message.SenderUserId != caller.UserId || message.Kind != ChatMessageKind.Text || message.DeletedAt is not null)
            throw new ChatException("chat_edit_not_allowed");
        if (DateTime.UtcNow - message.CreatedAt > EditWindow)
            throw new ChatException("chat_edit_expired");

        var conversation = await EnsureMemberAsync(db, caller, message.ConversationId, tracking: true);
        if (message.Text != normalized)
        {
            message.Text = normalized;
            message.EditedAt = DateTime.UtcNow;
            if (conversation.LastMessageAt == message.CreatedAt)
                conversation.LastMessagePreview = Preview(normalized);
            await db.SaveChangesAsync();

            await PublishToMembersAsync(db, caller.TenantKey, message.ConversationId,
                new ChatMessageEdited(message.ConversationId, message.Id, message.Text, message.EditedAt.Value));
        }

        return await ProjectWithSender(db, db.ChatMessages.AsNoTracking().Where(m => m.Id == messageId)).SingleAsync();
    }

    // ---------- Chuyển tiếp ----------

    /// <summary>
    /// Chuyển tiếp 1 tin tới tối đa 10 đích (hội thoại có sẵn và/hoặc user → chat 1-1).
    /// File được sao chép riêng. Trả về số hội thoại đã gửi.
    /// </summary>
    public async Task<int> ForwardAsync(
        ChatCaller caller,
        Guid messageId,
        IReadOnlyCollection<Guid> conversationIds,
        IReadOnlyCollection<Guid> userIds)
    {
        if (conversationIds.Count + userIds.Count == 0)
            throw new ChatException("chat_forward_no_target");
        if (conversationIds.Count + userIds.Count > MaxForwardTargets)
            throw new ChatException("chat_forward_limit");

        ChatMessageRecord source;
        await using (var db = dbFactory.CreateDbContext())
        {
            source = await db.ChatMessages.AsNoTracking().SingleOrDefaultAsync(m => m.Id == messageId)
                ?? throw new ChatException("chat_message_not_found");
            var clearedAt = await GetClearedAtAsync(db, caller, source.ConversationId);
            if (source.Kind == ChatMessageKind.System || source.DeletedAt is not null ||
                (clearedAt is { } cleared && source.CreatedAt <= cleared))
                throw new ChatException("chat_forward_not_allowed");
        }

        var targets = new HashSet<Guid>(conversationIds);
        foreach (var userId in userIds)
            targets.Add(await GetOrCreateDirectAsync(caller, userId));

        foreach (var targetId in targets)
        {
            await using var db = dbFactory.CreateDbContext();
            var conversation = await EnsureMemberAsync(db, caller, targetId, tracking: true);

            var copy = new ChatMessageRecord
            {
                Id = Guid.NewGuid(),
                ConversationId = targetId,
                SenderUserId = caller.UserId,
                Kind = source.Kind,
                Text = source.Text,
                AttachmentName = source.AttachmentName,
                AttachmentContentType = source.AttachmentContentType,
                AttachmentSizeBytes = source.AttachmentSizeBytes,
                IsForwarded = true,
                CreatedAt = DateTime.UtcNow
            };
            if (source.AttachmentPath is not null)
                copy.AttachmentPath = await fileStorage.CopyAsync(
                    source.AttachmentPath, caller.TenantKey, targetId, copy.Id, source.AttachmentName ?? "file");

            var preview = copy.Kind == ChatMessageKind.File ? Preview("📎 " + copy.AttachmentName) : Preview(copy.Text);
            try
            {
                await SaveAndPublishAsync(db, caller, conversation, copy, preview);
            }
            catch
            {
                if (copy.AttachmentPath is not null)
                    fileStorage.TryDelete(copy.AttachmentPath);
                throw;
            }
        }

        return targets.Count;
    }

    // ---------- Ghim ----------

    /// <summary>Tin đang ghim (mới nhất trước), bỏ tin thuộc lịch sử user đã xoá phía mình.</summary>
    public async Task<IReadOnlyList<ChatMessageDto>> GetPinnedAsync(ChatCaller caller, Guid conversationId)
    {
        await using var db = dbFactory.CreateDbContext();
        var clearedAt = await GetClearedAtAsync(db, caller, conversationId);
        return await ProjectWithSender(db, VisibleMessages(db, conversationId, clearedAt)
                .Where(m => m.PinnedAt != null)
                .OrderByDescending(m => m.PinnedAt))
            .ToListAsync();
    }

    /// <summary>Mọi thành viên được ghim; quá 3 tin thì tin ghim cũ nhất tự bỏ ghim.</summary>
    public async Task PinAsync(ChatCaller caller, Guid messageId)
    {
        await using var db = dbFactory.CreateDbContext();
        var message = await db.ChatMessages.SingleOrDefaultAsync(m => m.Id == messageId)
            ?? throw new ChatException("chat_message_not_found");
        await EnsureMemberAsync(db, caller, message.ConversationId);
        if (message.Kind == ChatMessageKind.System || message.DeletedAt is not null)
            throw new ChatException("chat_pin_not_allowed");
        if (message.PinnedAt is not null)
            return;

        var pinned = await db.ChatMessages
            .Where(m => m.ConversationId == message.ConversationId && m.PinnedAt != null)
            .OrderBy(m => m.PinnedAt)
            .ToListAsync();
        foreach (var oldest in pinned.Take(Math.Max(0, pinned.Count - MaxPinsPerConversation + 1)))
        {
            oldest.PinnedAt = null;
            oldest.PinnedByUserId = null;
        }

        var now = DateTime.UtcNow;
        message.PinnedAt = now;
        message.PinnedByUserId = caller.UserId;
        await db.SaveChangesAsync();

        await AddSystemMessageAsync(db, caller.TenantKey, message.ConversationId, caller.UserId, "chat_sys_pinned", now);
        await PublishToMembersAsync(db, caller.TenantKey, message.ConversationId, new ChatPinsChanged(message.ConversationId));
    }

    public async Task UnpinAsync(ChatCaller caller, Guid messageId)
    {
        await using var db = dbFactory.CreateDbContext();
        var message = await db.ChatMessages.SingleOrDefaultAsync(m => m.Id == messageId)
            ?? throw new ChatException("chat_message_not_found");
        await EnsureMemberAsync(db, caller, message.ConversationId);
        if (message.PinnedAt is null)
            return;

        message.PinnedAt = null;
        message.PinnedByUserId = null;
        await db.SaveChangesAsync();

        await AddSystemMessageAsync(db, caller.TenantKey, message.ConversationId, caller.UserId, "chat_sys_unpinned", DateTime.UtcNow);
        await PublishToMembersAsync(db, caller.TenantKey, message.ConversationId, new ChatPinsChanged(message.ConversationId));
    }

    // ---------- Xoá ----------

    /// <summary>Người gửi thu hồi tin của mình trong 24 giờ: xoá hẳn nội dung và file.</summary>
    public async Task RecallMessageAsync(ChatCaller caller, Guid messageId)
    {
        await using var db = dbFactory.CreateDbContext();
        var message = await db.ChatMessages.SingleOrDefaultAsync(m => m.Id == messageId)
            ?? throw new ChatException("chat_message_not_found");

        if (message.SenderUserId != caller.UserId || message.Kind == ChatMessageKind.System)
            throw new ChatException("chat_recall_not_allowed");
        if (message.DeletedAt is not null)
            return;
        if (DateTime.UtcNow - message.CreatedAt > RecallWindow)
            throw new ChatException("chat_recall_expired");

        var conversation = await EnsureMemberAsync(db, caller, message.ConversationId, tracking: true);
        var storedPath = message.AttachmentPath;

        message.DeletedAt = DateTime.UtcNow;
        message.Text = "";
        message.AttachmentPath = null;
        message.AttachmentName = null;
        message.AttachmentContentType = null;
        message.AttachmentSizeBytes = null;
        var wasPinned = message.PinnedAt is not null;
        message.PinnedAt = null;
        message.PinnedByUserId = null;
        if (conversation.LastMessageAt == message.CreatedAt)
            conversation.LastMessagePreview = RecalledPreview;
        await db.SaveChangesAsync();

        if (storedPath is not null)
            fileStorage.TryDelete(storedPath);

        await PublishToMembersAsync(db, caller.TenantKey, message.ConversationId,
            new ChatMessageRecalled(message.ConversationId, message.Id));
        if (wasPinned)
            await PublishToMembersAsync(db, caller.TenantKey, message.ConversationId, new ChatPinsChanged(message.ConversationId));
    }

    /// <summary>Xoá hội thoại phía mình: ẩn khỏi danh sách và ẩn toàn bộ lịch sử hiện có. Người khác không bị ảnh hưởng.</summary>
    public async Task HideConversationAsync(ChatCaller caller, Guid conversationId)
    {
        await using var db = dbFactory.CreateDbContext();
        var participant = await db.ChatParticipants
            .SingleOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == caller.UserId)
            ?? throw new ChatException("chat_not_member");

        var now = DateTime.UtcNow;
        participant.ClearedAt = now;
        participant.LastReadAt = now;
        participant.IsHidden = true;
        await db.SaveChangesAsync();

        notifier.Publish(caller.TenantKey, [caller.UserId], new ChatConversationHidden(conversationId));
    }

    /// <summary>Trưởng nhóm giải tán nhóm: xoá hội thoại, toàn bộ tin nhắn (FK cascade) và file.</summary>
    public async Task DisbandGroupAsync(ChatCaller caller, Guid conversationId)
    {
        await using var db = dbFactory.CreateDbContext();
        await EnsureGroupOwnerAsync(db, caller, conversationId);

        var memberIds = await db.ChatParticipants.AsNoTracking()
            .Where(p => p.ConversationId == conversationId)
            .Select(p => p.UserId)
            .ToListAsync();

        await db.ChatConversations.Where(c => c.Id == conversationId).ExecuteDeleteAsync();
        fileStorage.TryDeleteConversation(caller.TenantKey, conversationId);

        notifier.Publish(caller.TenantKey, memberIds, new ChatConversationDeleted(conversationId));
    }

    /// <summary>Dùng cho controller tải file: trả về null nếu không có quyền hoặc không phải tin đính kèm.</summary>
    public async Task<(string Path, string Name, string ContentType)?> FindAttachmentAsync(ChatCaller caller, Guid messageId)
    {
        await using var db = dbFactory.CreateDbContext();
        var row = await (
                from m in db.ChatMessages
                join p in db.ChatParticipants on m.ConversationId equals p.ConversationId
                where m.Id == messageId && p.UserId == caller.UserId && m.AttachmentPath != null &&
                      m.DeletedAt == null && (p.ClearedAt == null || m.CreatedAt > p.ClearedAt)
                select new { m.AttachmentPath, m.AttachmentName, m.AttachmentContentType })
            .AsNoTracking()
            .SingleOrDefaultAsync();

        return row is null
            ? null
            : (row.AttachmentPath!, row.AttachmentName ?? "attachment", row.AttachmentContentType ?? "application/octet-stream");
    }

    // ---------- Helpers ----------

    private async Task<ChatMessageDto> SaveAndPublishAsync(
        AppDbContext db,
        ChatCaller caller,
        ChatConversation conversation,
        ChatMessageRecord message,
        string preview)
    {
        db.ChatMessages.Add(message);
        conversation.LastMessageAt = message.CreatedAt;
        conversation.LastMessagePreview = preview;

        // Người gửi coi như đã đọc tới tin của chính mình.
        var sender = await db.ChatParticipants
            .SingleAsync(p => p.ConversationId == conversation.Id && p.UserId == caller.UserId);
        sender.LastReadAt = message.CreatedAt;

        await db.SaveChangesAsync();

        await db.ChatParticipants
            .Where(p => p.ConversationId == conversation.Id && p.IsHidden)
            .ExecuteUpdateAsync(set => set.SetProperty(p => p.IsHidden, false));

        // Đọc lại qua cùng projection để DTO có tên người gửi + trích dẫn tin trả lời.
        var dto = await ProjectWithSender(db, db.ChatMessages.AsNoTracking().Where(m => m.Id == message.Id)).SingleAsync();

        await PublishToMembersAsync(db, caller.TenantKey, conversation.Id, new ChatMessageReceived(dto));
        return dto;
    }

    private async Task AddSystemMessageAsync(AppDbContext db, string tenantKey, Guid conversationId, Guid actorUserId, string code, DateTime at)
    {
        // Tin System lưu key localization; UI dịch khi hiển thị. SenderUserId = người thực hiện.
        var message = new ChatMessageRecord
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderUserId = actorUserId,
            Kind = ChatMessageKind.System,
            Text = code,
            CreatedAt = at
        };
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        // Phát realtime để khung chat đang mở hiện ngay (không chỉ khi tải lại).
        var dto = await ProjectWithSender(db, db.ChatMessages.AsNoTracking().Where(m => m.Id == message.Id)).SingleAsync();
        await PublishToMembersAsync(db, tenantKey, conversationId, new ChatMessageReceived(dto));
    }

    private static async Task EnsureReplyTargetAsync(AppDbContext db, Guid conversationId, Guid? replyToMessageId)
    {
        if (replyToMessageId is not { } replyId)
            return;
        var valid = await db.ChatMessages.AnyAsync(m =>
            m.Id == replyId && m.ConversationId == conversationId && m.Kind != ChatMessageKind.System);
        if (!valid)
            throw new ChatException("chat_message_not_found");
    }

    private async Task PublishToMembersAsync(AppDbContext db, string tenantKey, Guid conversationId, ChatEvent evt)
    {
        try
        {
            var memberIds = await db.ChatParticipants.AsNoTracking()
                .Where(p => p.ConversationId == conversationId)
                .Select(p => p.UserId)
                .ToListAsync();
            notifier.Publish(tenantKey, memberIds, evt);
        }
        catch (Exception ex)
        {
            // Dữ liệu đã lưu; người nhận sẽ thấy khi tải lại.
            logger.LogWarning(ex, "Không publish được sự kiện chat cho {ConversationId}", conversationId);
        }
    }

    private static async Task<ChatConversation> EnsureMemberAsync(
        AppDbContext db, ChatCaller caller, Guid conversationId, bool tracking = false)
    {
        var query = from c in db.ChatConversations
                    join p in db.ChatParticipants on c.Id equals p.ConversationId
                    where c.Id == conversationId && p.UserId == caller.UserId
                    select c;
        if (!tracking)
            query = query.AsNoTracking();

        return await query.SingleOrDefaultAsync() ?? throw new ChatException("chat_not_member");
    }

    /// <summary>Kiểm tra thành viên và trả về mốc "xoá phía mình" của caller.</summary>
    private static async Task<DateTime?> GetClearedAtAsync(AppDbContext db, ChatCaller caller, Guid conversationId)
    {
        var row = await db.ChatParticipants.AsNoTracking()
            .Where(p => p.ConversationId == conversationId && p.UserId == caller.UserId)
            .Select(p => new { p.ClearedAt })
            .SingleOrDefaultAsync()
            ?? throw new ChatException("chat_not_member");
        return row.ClearedAt;
    }

    private static IQueryable<ChatMessageRecord> VisibleMessages(AppDbContext db, Guid conversationId, DateTime? clearedAt)
    {
        var query = db.ChatMessages.AsNoTracking().Where(m => m.ConversationId == conversationId);
        return clearedAt is { } cleared ? query.Where(m => m.CreatedAt > cleared) : query;
    }

    private static async Task<ChatConversation> EnsureGroupOwnerAsync(AppDbContext db, ChatCaller caller, Guid conversationId)
    {
        var row = await (
                from c in db.ChatConversations
                join p in db.ChatParticipants on c.Id equals p.ConversationId
                where c.Id == conversationId && p.UserId == caller.UserId
                select new { Conversation = c, p.Role })
            .SingleOrDefaultAsync()
            ?? throw new ChatException("chat_not_member");

        if (row.Conversation.Type != ChatConversationType.Group)
            throw new ChatException("chat_group_only");
        if (row.Role != ChatParticipantRole.Owner)
            throw new ChatException("chat_owner_only");
        return row.Conversation;
    }

    // Không dùng ids.Contains(...) trong query: EF Core 8 dịch thành OPENJSON, không chạy trên SQL Server 2014.
    // Bảng user nhỏ nên tải danh sách id để kiểm tra.
    private static async Task EnsureUsersExistAsync(AppDbContext db, IReadOnlyCollection<Guid> userIds)
    {
        var known = (await db.UserList.AsNoTracking().Select(u => u.UsrId).ToListAsync()).ToHashSet();
        if (!userIds.All(known.Contains))
            throw new ChatException("chat_user_not_found");
    }

    private static IQueryable<ChatMessageRecord> UnreadQuery(AppDbContext db, Guid userId) =>
        from m in db.ChatMessages
        join p in db.ChatParticipants on m.ConversationId equals p.ConversationId
        where p.UserId == userId &&
              m.SenderUserId != userId &&
              m.Kind != ChatMessageKind.System &&
              m.DeletedAt == null &&
              (p.LastReadAt == null || m.CreatedAt > p.LastReadAt) &&
              (p.ClearedAt == null || m.CreatedAt > p.ClearedAt)
        select m;

    private static IQueryable<ChatMessageDto> ProjectWithSender(AppDbContext db, IQueryable<ChatMessageRecord> messages) =>
        from m in messages
        join u in db.UserList on m.SenderUserId equals u.UsrId into users
        from u in users.DefaultIfEmpty()
        join r in db.ChatMessages on m.ReplyToMessageId equals (Guid?)r.Id into replies
        from r in replies.DefaultIfEmpty()
        join ru in db.UserList on r.SenderUserId equals ru.UsrId into replyUsers
        from ru in replyUsers.DefaultIfEmpty()
        select new ChatMessageDto(
            m.Id,
            m.ConversationId,
            m.SenderUserId,
            u == null ? "?" : (u.Name ?? u.Usr ?? "?"),
            m.Kind,
            m.Text,
            m.AttachmentName,
            m.AttachmentContentType,
            m.AttachmentSizeBytes,
            m.CreatedAt,
            m.DeletedAt != null,
            m.IsForwarded,
            m.PinnedAt,
            r == null
                ? null
                : new ChatReplyPreview(
                    r.Id,
                    ru == null ? "?" : (ru.Name ?? ru.Usr ?? "?"),
                    r.Text,
                    r.AttachmentName,
                    r.DeletedAt != null),
            m.EditedAt);

    private static async Task<Guid?> FindDirectAsync(AppDbContext db, string directKey) =>
        await db.ChatConversations.AsNoTracking()
            .Where(c => c.DirectKey == directKey)
            .Select(c => (Guid?)c.Id)
            .SingleOrDefaultAsync();

    private static ChatParticipant NewParticipant(Guid conversationId, Guid userId, ChatParticipantRole role, DateTime now) =>
        new() { ConversationId = conversationId, UserId = userId, Role = role, JoinedAt = now };

    private static string DirectKey(Guid a, Guid b) =>
        a.CompareTo(b) < 0 ? $"{a:N}_{b:N}" : $"{b:N}_{a:N}";

    private static string DisplayName(string? name, string? usr) =>
        !string.IsNullOrWhiteSpace(name) ? name! : !string.IsNullOrWhiteSpace(usr) ? usr! : "?";

    private static string Preview(string text)
    {
        var singleLine = text.ReplaceLineEndings(" ");
        return singleLine.Length > PreviewLength ? singleLine[..(PreviewLength - 1)] + "…" : singleLine;
    }
}
