using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using NVOAMASIS.Models.Hr;
using NVOAMASIS.Resources;
using NVOAMASIS.Services;
using NVOAMASIS.Services.Hr;
using System.Security.Claims;

namespace NVOAMASIS.Components.Hr
{
    /// <summary>Base cho các trang/panel/dialog module Nhân sự: user hiện tại, quyền, thông báo.</summary>
    public abstract class HrPageBase : ComponentBase
    {
        [Inject] protected AccountService Asv { get; set; } = default!;
        [Inject] protected HrPermissionService PermSvc { get; set; } = default!;
        [Inject] protected ISnackbar Snackbar { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected IStringLocalizer<SharedResource> Localizer { get; set; } = default!;

        protected string Usr { get; private set; } = string.Empty;
        protected ClaimsPrincipal? CurrentUser { get; private set; }
        /// <summary>UserList.UsrId của người đang đăng nhập (claim Sid).</summary>
        protected Guid? CurrentUserId { get; private set; }

        /// <summary>Gọi đầu OnInitializedAsync của trang con.</summary>
        protected async Task InitUserAsync()
        {
            var auth = await Asv.GetAuth();
            CurrentUser = auth.User;
            Usr = auth.User.Identity?.Name ?? string.Empty;
            CurrentUserId = MobileJwtTokenService.GetUserId(auth.User);
        }

        protected string L(string key) => Localizer[key].Value;

        /// <summary>Dịch nếu là khóa hr_*, ngược lại giữ nguyên.</summary>
        protected string LMaybe(string? value) =>
            string.IsNullOrEmpty(value) ? string.Empty
            : value.StartsWith("hr_", StringComparison.Ordinal) ? Localizer[value].Value
            : value;

        protected void Notify(string message, Severity severity)
        {
            Snackbar.Configuration.SnackbarVariant = Variant.Filled;
            Snackbar.Configuration.MaxDisplayedSnackbars = 3;
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;
            Snackbar.Add(message, severity);
        }

        protected void Notify(HrResult result)
        {
            var msg = L(result.MessageKey);
            if (!string.IsNullOrWhiteSpace(result.Detail))
                msg = result.Success ? string.Format(msg, result.Detail) : $"{msg}: {result.Detail}";
            Notify(msg, result.Success ? Severity.Success : Severity.Warning);
        }

        protected void NoPermission(string action) =>
            Notify(L($"NoPermission_{action}"), Severity.Warning);

        protected static string D(DateTime? d) => d.HasValue ? d.Value.ToString("dd/MM/yyyy") : string.Empty;
        protected static string N(decimal? v) => v.HasValue ? v.Value.ToString("N0") : string.Empty;

        protected static Color StatusColor(int status) => status switch
        {
            HrEmployeeStatus.Probation => Color.Warning,
            HrEmployeeStatus.Active => Color.Success,
            HrEmployeeStatus.Suspended => Color.Info,
            HrEmployeeStatus.Resigned => Color.Default,
            _ => Color.Default
        };
    }
}
