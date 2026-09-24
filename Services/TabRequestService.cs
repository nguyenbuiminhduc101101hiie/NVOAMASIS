using Microsoft.AspNetCore.Components;

namespace NVOAMASIS.Services;

/// <summary>
/// Cho phép một trang/dialog bất kỳ yêu cầu MainLayout mở (hoặc chuyển tới) một tab, và báo cho tab đang mở tải lại dữ liệu.
/// Đăng ký Scoped: mỗi kết nối (circuit) có một instance dùng chung cho MainLayout và các trang.
/// </summary>
public class TabRequestService
{
    /// <summary>MainLayout đăng ký để mở tab.</summary>
    public event Func<string, string, RenderFragment, Task>? OpenRequested;

    /// <summary>Trang Chứng từ kế toán đăng ký để tải lại khi có chứng từ mới.</summary>
    public event Func<Task>? VouchersChanged;

    /// <summary>Mở tab (hoặc chuyển tới tab đã có cùng key). Trả về false nếu chưa có nơi nào xử lý.</summary>
    public async Task<bool> OpenAsync(string key, string title, RenderFragment content)
    {
        var handler = OpenRequested;
        if (handler == null)
            return false;

        await handler(key, title, content);
        return true;
    }

    public Task NotifyVouchersChangedAsync()
    {
        var handler = VouchersChanged;
        return handler == null ? Task.CompletedTask : handler();
    }
}
