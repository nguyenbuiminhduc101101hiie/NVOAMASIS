using NVOAMASIS.Models.Chat;

namespace NVOAMASIS.Services.Chat;

public abstract record ChatEvent(Guid ConversationId);

public sealed record ChatMessageReceived(ChatMessageDto Message) : ChatEvent(Message.ConversationId);

public sealed record ChatReadUpdated(Guid ConversationId, Guid UserId, DateTime LastReadAt) : ChatEvent(ConversationId);

public sealed record ChatTyping(Guid ConversationId, Guid UserId, string UserName) : ChatEvent(ConversationId);

/// <summary>Tạo nhóm, đổi tên, thêm/xoá thành viên — người nhận nên tải lại danh sách hội thoại.</summary>
public sealed record ChatConversationChanged(Guid ConversationId) : ChatEvent(ConversationId);

public sealed record ChatMessageRecalled(Guid ConversationId, Guid MessageId) : ChatEvent(ConversationId);

/// <summary>User tự xoá hội thoại phía mình — chỉ gửi cho chính user đó (các tab khác của họ).</summary>
public sealed record ChatConversationHidden(Guid ConversationId) : ChatEvent(ConversationId);

/// <summary>Danh sách tin ghim của hội thoại thay đổi — tải lại thanh ghim.</summary>
public sealed record ChatPinsChanged(Guid ConversationId) : ChatEvent(ConversationId);

/// <summary>Nhóm bị giải tán — hội thoại không còn tồn tại.</summary>
public sealed record ChatConversationDeleted(Guid ConversationId) : ChatEvent(ConversationId);

/// <summary>
/// Pub/sub in-process cho chat. Component Blazor Server subscribe theo (tenant, user);
/// ChatService publish sau mỗi thao tác ghi thành công.
/// Chỉ đúng khi app chạy 1 instance — scale-out cần thay bằng SignalR + backplane.
/// </summary>
public sealed class ChatNotifier(ILogger<ChatNotifier> logger)
{
    private readonly object _gate = new();
    private readonly Dictionary<(string Tenant, Guid UserId), List<Func<ChatEvent, Task>>> _handlers = new();

    public IDisposable Subscribe(string tenantKey, Guid userId, Func<ChatEvent, Task> handler)
    {
        var key = (tenantKey, userId);
        lock (_gate)
        {
            if (!_handlers.TryGetValue(key, out var list))
            {
                list = new List<Func<ChatEvent, Task>>();
                _handlers[key] = list;
            }
            list.Add(handler);
        }

        return new Subscription(() =>
        {
            lock (_gate)
            {
                if (_handlers.TryGetValue(key, out var list))
                {
                    list.Remove(handler);
                    if (list.Count == 0)
                        _handlers.Remove(key);
                }
            }
        });
    }

    /// <summary>Không chờ handler: mỗi handler tự dispatch vào circuit của nó, lỗi của một circuit không ảnh hưởng người gửi.</summary>
    public void Publish(string tenantKey, IEnumerable<Guid> userIds, ChatEvent evt)
    {
        var targets = new List<Func<ChatEvent, Task>>();
        lock (_gate)
        {
            foreach (var userId in userIds.Distinct())
            {
                if (_handlers.TryGetValue((tenantKey, userId), out var list))
                    targets.AddRange(list);
            }
        }

        foreach (var handler in targets)
            _ = InvokeSafeAsync(handler, evt);
    }

    private async Task InvokeSafeAsync(Func<ChatEvent, Task> handler, ChatEvent evt)
    {
        try
        {
            await handler(evt);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Chat listener lỗi khi xử lý {EventType}", evt.GetType().Name);
        }
    }

    private sealed class Subscription(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;

        public void Dispose() => Interlocked.Exchange(ref _dispose, null)?.Invoke();
    }
}
