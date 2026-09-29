using Microsoft.AspNetCore.Components.Authorization;

namespace NVOAMASIS.Services.Accounting
{
    /// <summary>
    /// Kiểm tra quyền thao tác (Thêm / Sửa / Xóa / Duyệt) theo mã quyền menu (bảng Permissions, màn hình 1.6).
    /// Dùng ở các màn hình chỉ được mở bằng quyền Xem nhưng có nút ghi dữ liệu.
    /// </summary>
    public static class AccountingPermission
    {
        public const string Add = "Add";
        public const string Edit = "Edit";
        public const string Delete = "Delete";
        public const string Approve = "Approve";

        public static async Task<bool> CanAsync(this SharedServices ssv, AuthenticationStateProvider auth, string func, string menuName)
        {
            var state = await auth.GetAuthenticationStateAsync();
            var user = state.User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(user)) return false;
            return await ssv.CheckPermission(user, func, menuName);
        }

        public static string Denied(string func, string menuName) =>
            $"Bạn không có quyền {Vi(func)} (mã quyền {menuName}). Nhờ quản trị cấp quyền tại 1.6 Phân quyền.";

        private static string Vi(string func) => func switch
        {
            Add => "Thêm",
            Edit => "Sửa",
            Delete => "Xóa",
            Approve => "Duyệt",
            _ => func
        };
    }
}
