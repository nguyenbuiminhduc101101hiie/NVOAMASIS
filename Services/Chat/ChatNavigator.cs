namespace NVOAMASIS.Services.Chat;

/// <summary>
/// Trạng thái chat theo circuit: tab Chat có đang hiển thị không, hội thoại nào đang mở,
/// và yêu cầu mở hội thoại (từ snackbar ở MainLayout).
/// DynamicTabs dùng KeepPanelsAlive nên trang Chat vẫn sống khi tab khác đang hiển thị.
/// </summary>
public sealed class ChatNavigator
{
    public const string TabKey = "chat";

    public bool IsChatTabActive { get; private set; }

    /// <summary>Do ChatIndex cập nhật.</summary>
    public Guid? ActiveConversationId { get; set; }

    public Guid? PendingConversationId { get; private set; }

    /// <summary>Tab Chat được hiện/ẩn hoặc có yêu cầu mở hội thoại.</summary>
    public event Action? Changed;

    public bool IsViewing(Guid conversationId) =>
        IsChatTabActive && ActiveConversationId == conversationId;

    public void SetActiveTab(string? key)
    {
        var active = key == TabKey;
        if (active == IsChatTabActive)
            return;
        IsChatTabActive = active;
        Changed?.Invoke();
    }

    public void RequestOpen(Guid conversationId)
    {
        PendingConversationId = conversationId;
        Changed?.Invoke();
    }

    public Guid? TakePending()
    {
        var pending = PendingConversationId;
        PendingConversationId = null;
        return pending;
    }
}
