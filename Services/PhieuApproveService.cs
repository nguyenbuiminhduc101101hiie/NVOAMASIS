using System.Net;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using NVOAMASIS.Services.MultiTenant;

namespace NVOAMASIS.Services;

public class PhieuApproveService(
    AppDbContext db,
    GlobalServices gsv,
    IHttpContextAccessor httpContextAccessor,
    ITenantContext tenantContext,
    NotificationService notificationService,
    FcmPushService fcmPushService)
{
    public const string LoaiThu = "Thu";
    public const string LoaiChi = "Chi";
    public const string NotiPrefix = "[PHIEU_APPROVE|";

    public static bool TryParsePhieuApproveNotification(string? message, out string loai, out string token)
    {
        loai = "";
        token = "";
        if (string.IsNullOrWhiteSpace(message) || !message.StartsWith(NotiPrefix, StringComparison.Ordinal))
            return false;

        // [PHIEU_APPROVE|Thu|{token}] rest...
        var end = message.IndexOf(']');
        if (end <= NotiPrefix.Length) return false;
        var payload = message.Substring(NotiPrefix.Length, end - NotiPrefix.Length);
        var parts = payload.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2) return false;
        if (parts[0] is not (LoaiThu or LoaiChi)) return false;
        loai = parts[0];
        token = parts[1];
        return !string.IsNullOrWhiteSpace(token);
    }

    public static string BuildNotificationMessage(string loai, string token, string soPhieu, string requestedBy)
    {
        var kind = loai == LoaiThu ? "phiếu thu" : "phiếu chi";
        return $"{NotiPrefix}{loai}|{token}] Yêu cầu duyệt {kind} {soPhieu} từ {requestedBy}. Click để Approve/Deny.";
    }

    public async Task<BoolandMessReponse> SendApprovalRequestAsync(string loai, Guid phieuId, string requestedBy, string? baseUrl = null)
    {
        try
        {
            db.ChangeTracker.Clear();
            var smtpReady = await IsSmtpConfiguredAsync();

            if (loai == LoaiThu)
            {
                var item = await db.Phieuthu.AsNoTracking().FirstOrDefaultAsync(x => x.PhieuthuID == phieuId);
                if (item == null)
                    return new BoolandMessReponse(false, "Không tìm thấy phiếu thu.");
                if (item.Approve == true)
                    return new BoolandMessReponse(false, "Phiếu thu đã được Approve. Hãy Bỏ Approve trước nếu cần gửi lại.");

                var approvers = await GetApproversAsync(LoaiThu);
                if (approvers.Count == 0)
                    return new BoolandMessReponse(false, "Không có user nào được gán Duyệt phiếu thu.");

                var token = Guid.NewGuid().ToString("N");
                var nowText = DateTime.Now.ToString("dd/MMM/yyyy");
                await db.Phieuthu
                    .Where(x => x.PhieuthuID == phieuId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.ApproveToken, token)
                        .SetProperty(x => x.Approve, (bool?)null)
                        .SetProperty(x => x.ApproveBy, (string?)null)
                        .SetProperty(x => x.ApproveByUserId, (Guid?)null)
                        .SetProperty(x => x.ApproveDate, (DateTime?)null)
                        .SetProperty(x => x.Remarks, (string?)null)
                        .SetProperty(x => x.Trangthai, "ChoDuyet")
                        .SetProperty(x => x.UserUpdate, requestedBy)
                        .SetProperty(x => x.DateUpdate, nowText));

                item.ApproveToken = token;
                await SendInAppNotificationsAsync(LoaiThu, token, item.SoPhieuthu ?? "", requestedBy, approvers);

                BoolandMessReponse? mailRs = null;
                if (smtpReady)
                    mailRs = await SendRequestEmailsAsync(LoaiThu, token, BuildThuSummary(item), approvers, requestedBy, baseUrl);

                return BuildSendResult(approvers.Count, mailRs, smtpReady);
            }
            else
            {
                var item = await db.Phieuchi.AsNoTracking().FirstOrDefaultAsync(x => x.PhieuchiID == phieuId);
                if (item == null)
                    return new BoolandMessReponse(false, "Không tìm thấy phiếu chi.");
                if (item.Approve == true)
                    return new BoolandMessReponse(false, "Phiếu chi đã được Approve. Hãy Bỏ Approve trước nếu cần gửi lại.");

                var approvers = await GetApproversAsync(LoaiChi);
                if (approvers.Count == 0)
                    return new BoolandMessReponse(false, "Không có user nào được gán Duyệt phiếu chi.");

                var token = Guid.NewGuid().ToString("N");
                var nowText = DateTime.Now.ToString("dd/MMM/yyyy");
                await db.Phieuchi
                    .Where(x => x.PhieuchiID == phieuId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.ApproveToken, token)
                        .SetProperty(x => x.Approve, (bool?)null)
                        .SetProperty(x => x.ApproveBy, (string?)null)
                        .SetProperty(x => x.ApproveByUserId, (Guid?)null)
                        .SetProperty(x => x.ApproveDate, (DateTime?)null)
                        .SetProperty(x => x.Remarks, (string?)null)
                        .SetProperty(x => x.Trangthai, "ChoDuyet")
                        .SetProperty(x => x.Useupdate, requestedBy)
                        .SetProperty(x => x.DateUpdate, nowText));

                item.ApproveToken = token;
                await SendInAppNotificationsAsync(LoaiChi, token, item.Sophieuchi ?? "", requestedBy, approvers);

                BoolandMessReponse? mailRs = null;
                if (smtpReady)
                    mailRs = await SendRequestEmailsAsync(LoaiChi, token, BuildChiSummary(item), approvers, requestedBy, baseUrl);

                return BuildSendResult(approvers.Count, mailRs, smtpReady);
            }
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Gửi yêu cầu duyệt thất bại: " + ex.Message);
        }
    }

    private async Task<bool> IsSmtpConfiguredAsync()
    {
        var company = await db.CompanyInfomation.AsNoTracking().FirstOrDefaultAsync();
        return company != null
            && !string.IsNullOrWhiteSpace(company.Email_GuiTB)
            && !string.IsNullOrWhiteSpace(company.PasswordEmail_GuiTB)
            && !string.IsNullOrWhiteSpace(company.SmtpServer)
            && company.SmtpPort.HasValue;
    }

    private static BoolandMessReponse BuildSendResult(int approverCount, BoolandMessReponse? mailRs, bool smtpReady)
    {
        if (!smtpReady)
            return new BoolandMessReponse(true,
                $"Đã gửi thông báo trong hệ thống tới {approverCount} người. (Chưa gửi email vì thiếu SMTP)");

        if (mailRs == null)
            return new BoolandMessReponse(true, $"Đã gửi thông báo trong hệ thống tới {approverCount} người.");

        if (mailRs.Flag)
            return new BoolandMessReponse(true,
                $"Đã gửi thông báo + email duyệt tới {approverCount} người.");

        return new BoolandMessReponse(true,
            $"Đã gửi thông báo trong hệ thống tới {approverCount} người. Email: {mailRs.Message}");
    }

    private async Task SendInAppNotificationsAsync(string loai, string token, string soPhieu, string requestedBy, List<AuthUser> approvers)
    {
        var msg = BuildNotificationMessage(loai, token, soPhieu, requestedBy);
        var ids = approvers.Select(a => a.UsrId.ToString()).Distinct().ToArray();
        if (ids.Length > 0)
            await notificationService.SendNotificationQuietAsync(ids, msg);

        // Push FCM HTTP v1 tới Flutter (cần Fcm.Enabled + service account + device token)
        try
        {
            var kind = loai == LoaiThu ? "phiếu thu" : "phiếu chi";
            var title = $"Yêu cầu duyệt {kind}";
            var body = string.IsNullOrWhiteSpace(soPhieu)
                ? $"Từ {requestedBy}"
                : $"{soPhieu} từ {requestedBy}";
            await fcmPushService.SendPhieuApproveAsync(
                approvers.Select(a => a.UsrId),
                loai,
                token,
                title,
                body);
        }
        catch (Exception ex)
        {
            // Không chặn flow duyệt nếu FCM lỗi
            System.Diagnostics.Debug.WriteLine("FCM push skipped/failed: " + ex.Message);
        }
    }

    /// <summary>Chi tiết phiếu để mở dialog duyệt trong app.</summary>
    public async Task<PhieuApproveInAppModel?> GetInAppDetailAsync(string token, Guid currentUserId)
    {
        db.ChangeTracker.Clear();
        var snapshot = await LoadSnapshotAsync(db, token);
        if (snapshot == null) return null;

        var user = await db.UserList.AsNoTracking().FirstOrDefaultAsync(x => x.UsrId == currentUserId);
        var allowed = user != null && IsUserAllowed(user, snapshot.Loai);
        var decided = IsAlreadyDecided(snapshot);

        return new PhieuApproveInAppModel
        {
            Loai = snapshot.Loai,
            PhieuId = snapshot.PhieuId,
            Token = token,
            SoPhieu = snapshot.SoPhieu,
            DetailHtml = snapshot.DetailHtml,
            Approve = snapshot.Approve,
            ApproveBy = snapshot.ApproveBy,
            ApproveDate = snapshot.ApproveDate,
            Remarks = snapshot.Remarks,
            AlreadyDecided = decided,
            CanDecide = allowed && !decided
        };
    }

    public async Task<BoolandMessReponse> DecideInAppAsync(string token, Guid userId, bool approve, string? remarks)
    {
        try
        {
            db.ChangeTracker.Clear();
            var snapshot = await LoadSnapshotAsync(db, token, track: true);
            if (snapshot == null)
                return new BoolandMessReponse(false, "Không tìm thấy yêu cầu duyệt (token không hợp lệ).");

            if (IsAlreadyDecided(snapshot))
            {
                var status = snapshot.Approve == true ? "Approve" : "Deny";
                return new BoolandMessReponse(false,
                    $"Phiếu đã được {status} bởi {snapshot.ApproveBy}. Không thể xử lý thêm.");
            }

            var approver = await db.UserList.AsNoTracking().FirstOrDefaultAsync(x => x.UsrId == userId);
            if (approver == null || !IsUserAllowed(approver, snapshot.Loai))
                return new BoolandMessReponse(false, "Bạn không có quyền duyệt loại phiếu này.");

            if (!approve && string.IsNullOrWhiteSpace(remarks))
                return new BoolandMessReponse(false, "Khi Deny bắt buộc nhập lý do.");

            var actorName = approver.Name ?? approver.Usr ?? approver.Email ?? userId.ToString();
            var now = DateTime.Now;

            if (snapshot.Loai == LoaiThu)
            {
                var item = await db.Phieuthu.FirstAsync(x => x.PhieuthuID == snapshot.PhieuId);
                if (IsAlreadyDecided(item.Approve, item.ApproveBy, item.ApproveDate))
                    return new BoolandMessReponse(false, $"Phiếu thu đã được xử lý bởi {item.ApproveBy}.");

                item.Approve = approve;
                item.ApproveBy = actorName;
                item.ApproveByUserId = userId;
                item.ApproveDate = now;
                item.Remarks = approve ? null : remarks!.Trim();
                item.Trangthai = approve ? "DaDuyet" : "TuChoi";
                item.UserUpdate = actorName;
                item.DateUpdate = now.ToString("dd/MMM/yyyy");
                await db.SaveChangesAsync();
                await NotifyWatchersAndOtherApproversAsync(db, LoaiThu, BuildThuSummary(item), approve, actorName, item.Remarks, userId);
                return new BoolandMessReponse(true, approve ? "Đã Approve phiếu thu." : "Đã Deny phiếu thu.");
            }
            else
            {
                var item = await db.Phieuchi.FirstAsync(x => x.PhieuchiID == snapshot.PhieuId);
                if (IsAlreadyDecided(item.Approve, item.ApproveBy, item.ApproveDate))
                    return new BoolandMessReponse(false, $"Phiếu chi đã được xử lý bởi {item.ApproveBy}.");

                item.Approve = approve;
                item.ApproveBy = actorName;
                item.ApproveByUserId = userId;
                item.ApproveDate = now;
                item.Remarks = approve ? null : remarks!.Trim();
                item.Trangthai = approve ? "DaDuyet" : "TuChoi";
                item.Useupdate = actorName;
                item.DateUpdate = now.ToString("dd/MMM/yyyy");
                await db.SaveChangesAsync();
                await NotifyWatchersAndOtherApproversAsync(db, LoaiChi, BuildChiSummary(item), approve, actorName, item.Remarks, userId);
                return new BoolandMessReponse(true, approve ? "Đã Approve phiếu chi." : "Đã Deny phiếu chi.");
            }
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Xử lý duyệt thất bại: " + ex.Message);
        }
    }

    private async Task<BoolandMessReponse> EnsureSmtpConfiguredAsync()
    {
        if (await IsSmtpConfiguredAsync())
            return new BoolandMessReponse(true, "OK");
        return new BoolandMessReponse(false,
            "Thiếu cấu hình SMTP trong Company Information (Email gửi TB / Mật khẩu / SMTP Server / SMTP Port). Vui lòng cấu hình trước khi gửi duyệt.");
    }

    /// <summary>
    /// Duyệt trực tiếp trên 10.1 / 10.2 (không qua email / thông báo):
    /// chỉ user có quyền duyệt loại phiếu (Duyet_Phieu_Thu / Duyet_Phieu_Chi), ghi người duyệt + ngày giờ.
    /// </summary>
    public async Task<BoolandMessReponse> ApproveDirectAsync(string loai, Guid phieuId, Guid currentUserId)
    {
        try
        {
            db.ChangeTracker.Clear();
            if (currentUserId == Guid.Empty)
                return new BoolandMessReponse(false, "Không xác định được user hiện tại.");
            var approver = await db.UserList.AsNoTracking().FirstOrDefaultAsync(x => x.UsrId == currentUserId);
            if (approver == null || !IsUserAllowed(approver, loai))
                return new BoolandMessReponse(false, "Bạn không có quyền duyệt loại phiếu này.");

            var actorName = approver.Name ?? approver.Usr ?? approver.Email ?? currentUserId.ToString();
            var now = DateTime.Now;

            if (loai == LoaiThu)
            {
                var item = await db.Phieuthu.FirstOrDefaultAsync(x => x.PhieuthuID == phieuId);
                if (item == null)
                    return new BoolandMessReponse(false, "Không tìm thấy phiếu thu.");
                if (item.Approve == true)
                    return new BoolandMessReponse(false, $"Phiếu thu đã được duyệt bởi {item.ApproveBy} lúc {item.ApproveDate:dd/MM/yyyy HH:mm}.");

                item.Approve = true;
                item.ApproveBy = actorName;
                item.ApproveByUserId = currentUserId;
                item.ApproveDate = now;
                item.Trangthai = "DaDuyet";
                item.UserUpdate = actorName;
                item.DateUpdate = now.ToString("dd/MMM/yyyy");
                await db.SaveChangesAsync();
                await NotifyWatchersAndOtherApproversAsync(db, LoaiThu, BuildThuSummary(item), true, actorName, null, currentUserId);
                return new BoolandMessReponse(true, $"Đã duyệt phiếu thu {item.SoPhieuthu} ({actorName}, {now:dd/MM/yyyy HH:mm}).");
            }
            else
            {
                var item = await db.Phieuchi.FirstOrDefaultAsync(x => x.PhieuchiID == phieuId);
                if (item == null)
                    return new BoolandMessReponse(false, "Không tìm thấy phiếu chi.");
                if (item.Approve == true)
                    return new BoolandMessReponse(false, $"Phiếu chi đã được duyệt bởi {item.ApproveBy} lúc {item.ApproveDate:dd/MM/yyyy HH:mm}.");

                item.Approve = true;
                item.ApproveBy = actorName;
                item.ApproveByUserId = currentUserId;
                item.ApproveDate = now;
                item.Trangthai = "DaDuyet";
                item.Useupdate = actorName;
                item.DateUpdate = now.ToString("dd/MMM/yyyy");
                await db.SaveChangesAsync();
                await NotifyWatchersAndOtherApproversAsync(db, LoaiChi, BuildChiSummary(item), true, actorName, null, currentUserId);
                return new BoolandMessReponse(true, $"Đã duyệt phiếu chi {item.Sophieuchi} ({actorName}, {now:dd/MM/yyyy HH:mm}).");
            }
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Duyệt thất bại: " + ex.Message);
        }
    }

    /// <summary>Người dùng hiện tại có quyền duyệt loại phiếu này không (cờ Duyet_Phieu_Thu / Duyet_Phieu_Chi).</summary>
    public async Task<bool> CanApproveAsync(string loai, Guid userId)
    {
        if (userId == Guid.Empty) return false;
        var user = await db.UserList.AsNoTracking().FirstOrDefaultAsync(x => x.UsrId == userId);
        return user != null && IsUserAllowed(user, loai);
    }

    /// <summary>
    /// Chỉ user đã Approve (ApproveByUserId) mới được bỏ Approve.
    /// </summary>
    public async Task<BoolandMessReponse> UnapproveAsync(string loai, Guid phieuId, Guid currentUserId, string currentUserName)
    {
        try
        {
            db.ChangeTracker.Clear();
            if (currentUserId == Guid.Empty)
                return new BoolandMessReponse(false, "Không xác định được user hiện tại.");

            if (loai == LoaiThu)
            {
                var item = await db.Phieuthu.AsNoTracking().FirstOrDefaultAsync(x => x.PhieuthuID == phieuId);
                if (item == null)
                    return new BoolandMessReponse(false, "Không tìm thấy phiếu thu.");
                if (item.Approve != true)
                    return new BoolandMessReponse(false, "Phiếu thu chưa ở trạng thái Approve.");
                if (item.ApproveByUserId == null || item.ApproveByUserId != currentUserId)
                    return new BoolandMessReponse(false,
                        $"Chỉ user đã duyệt ({item.ApproveBy ?? "N/A"}) mới được Bỏ Approve.");

                var nowText = DateTime.Now.ToString("dd/MMM/yyyy");
                await db.Phieuthu
                    .Where(x => x.PhieuthuID == phieuId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.Approve, (bool?)null)
                        .SetProperty(x => x.ApproveBy, (string?)null)
                        .SetProperty(x => x.ApproveByUserId, (Guid?)null)
                        .SetProperty(x => x.ApproveDate, (DateTime?)null)
                        .SetProperty(x => x.ApproveToken, (string?)null)
                        .SetProperty(x => x.Trangthai, (string?)null)
                        .SetProperty(x => x.UserUpdate, currentUserName)
                        .SetProperty(x => x.DateUpdate, nowText));

                return new BoolandMessReponse(true, "Đã bỏ Approve phiếu thu.");
            }
            else
            {
                var item = await db.Phieuchi.AsNoTracking().FirstOrDefaultAsync(x => x.PhieuchiID == phieuId);
                if (item == null)
                    return new BoolandMessReponse(false, "Không tìm thấy phiếu chi.");
                if (item.Approve != true)
                    return new BoolandMessReponse(false, "Phiếu chi chưa ở trạng thái Approve.");
                if (item.ApproveByUserId == null || item.ApproveByUserId != currentUserId)
                    return new BoolandMessReponse(false,
                        $"Chỉ user đã duyệt ({item.ApproveBy ?? "N/A"}) mới được Bỏ Approve.");

                var nowText = DateTime.Now.ToString("dd/MMM/yyyy");
                await db.Phieuchi
                    .Where(x => x.PhieuchiID == phieuId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.Approve, (bool?)null)
                        .SetProperty(x => x.ApproveBy, (string?)null)
                        .SetProperty(x => x.ApproveByUserId, (Guid?)null)
                        .SetProperty(x => x.ApproveDate, (DateTime?)null)
                        .SetProperty(x => x.ApproveToken, (string?)null)
                        .SetProperty(x => x.Trangthai, (string?)null)
                        .SetProperty(x => x.Useupdate, currentUserName)
                        .SetProperty(x => x.DateUpdate, nowText));

                return new BoolandMessReponse(true, "Đã bỏ Approve phiếu chi.");
            }
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Bỏ Approve thất bại: " + ex.Message);
        }
    }

    public async Task<string> BuildDecisionPageHtmlAsync(string token, Guid? userId)
    {
        var snapshot = await LoadSnapshotAsync(db, token);
        if (snapshot == null)
            return HtmlResult("Không tìm thấy", "Link duyệt không hợp lệ hoặc đã hết hiệu lực.", isError: true);

        if (IsAlreadyDecided(snapshot))
        {
            var status = snapshot.Approve == true ? "Approve" : "Deny";
            return HtmlResult(
                "Đã xử lý",
                $"Phiếu này đã được <b>{status}</b> bởi <b>{WebUtility.HtmlEncode(snapshot.ApproveBy)}</b>" +
                (snapshot.ApproveDate.HasValue ? $" lúc {snapshot.ApproveDate:dd/MM/yyyy HH:mm}" : "") +
                (snapshot.Approve == false && !string.IsNullOrWhiteSpace(snapshot.Remarks)
                    ? $"<br/>Lý do Deny: {WebUtility.HtmlEncode(snapshot.Remarks)}"
                    : "") +
                "<br/><br/>Bạn không thể Approve/Deny thêm.",
                isError: false);
        }

        if (userId == null || userId == Guid.Empty)
            return HtmlResult("Thiếu thông tin", "Link không có thông tin người duyệt. Vui lòng mở đúng link trong email.", isError: true);

        var approver = await db.UserList.AsNoTracking().FirstOrDefaultAsync(x => x.UsrId == userId.Value);
        if (approver == null || !IsUserAllowed(approver, snapshot.Loai))
            return HtmlResult("Không có quyền", "Bạn không có quyền duyệt loại phiếu này.", isError: true);

        var actionUrl = $"{GetBaseUrl()}/api/phieu-approve/{token}/decide";
        var title = snapshot.Loai == LoaiThu ? "Duyệt phiếu thu" : "Duyệt phiếu chi";
        var sb = new StringBuilder();
        sb.Append($@"<!DOCTYPE html><html><head><meta charset='utf-8'/><meta name='viewport' content='width=device-width,initial-scale=1'/>
<title>{title}</title>
<style>
body{{font-family:Segoe UI,Arial,sans-serif;background:#f1f5f9;margin:0;padding:24px;color:#0f172a}}
.card{{max-width:640px;margin:0 auto;background:#fff;border-radius:12px;padding:24px;box-shadow:0 8px 24px rgba(15,23,42,.08)}}
h1{{font-size:20px;margin:0 0 16px}}
table{{width:100%;border-collapse:collapse;margin:12px 0 20px}}
td{{padding:8px 6px;border-bottom:1px solid #e2e8f0;font-size:14px;vertical-align:top}}
td.k{{width:38%;color:#64748b;font-weight:600}}
.actions{{display:flex;gap:12px;flex-wrap:wrap;margin-top:8px}}
button,.btn{{appearance:none;border:0;border-radius:8px;padding:12px 18px;font-weight:700;cursor:pointer;text-decoration:none;display:inline-block}}
.approve{{background:#059669;color:#fff}}
.deny{{background:#dc2626;color:#fff}}
textarea{{width:100%;min-height:90px;border:1px solid #cbd5e1;border-radius:8px;padding:10px;font:inherit;box-sizing:border-box}}
.hint{{font-size:13px;color:#64748b;margin-top:8px}}
</style></head><body><div class='card'>");
        sb.Append($"<h1>{title}</h1>");
        sb.Append($"<div class='hint'>Người nhận: {WebUtility.HtmlEncode(approver.Name ?? approver.Usr)} ({WebUtility.HtmlEncode(approver.Email)})</div>");
        sb.Append(snapshot.DetailHtml);
        sb.Append($@"
<form method='post' action='{actionUrl}'>
  <input type='hidden' name='uid' value='{userId:N}' />
  <input type='hidden' name='action' id='actionField' value='approve' />
  <label style='display:block;font-weight:700;margin:12px 0 6px'>Lý do Deny (bắt buộc khi Deny)</label>
  <textarea name='remarks' id='remarks' placeholder='Nhập lý do từ chối...'></textarea>
  <div class='actions'>
    <button type='submit' class='approve' onclick=""document.getElementById('actionField').value='approve'; return true;"">Approve</button>
    <button type='submit' class='deny' onclick=""
      document.getElementById('actionField').value='deny';
      var r=document.getElementById('remarks').value.trim();
      if(!r){{alert('Vui lòng nhập lý do Deny.'); return false;}}
      return true;
    "">Deny</button>
  </div>
</form></div></body></html>");
        return sb.ToString();
    }

    public async Task<string> ProcessDecisionHtmlAsync(string token, Guid userId, string action, string? remarks)
    {
        var snapshot = await LoadSnapshotAsync(db, token, track: true);
        if (snapshot == null)
            return HtmlResult("Không tìm thấy", "Link duyệt không hợp lệ hoặc đã hết hiệu lực.", isError: true);

        if (IsAlreadyDecided(snapshot))
        {
            var status = snapshot.Approve == true ? "Approve" : "Deny";
            return HtmlResult(
                "Đã xử lý",
                $"Phiếu này đã được <b>{status}</b> bởi <b>{WebUtility.HtmlEncode(snapshot.ApproveBy)}</b>. Bạn không thể xử lý thêm.",
                isError: false);
        }

        var approver = await db.UserList.AsNoTracking().FirstOrDefaultAsync(x => x.UsrId == userId);
        if (approver == null || !IsUserAllowed(approver, snapshot.Loai))
            return HtmlResult("Không có quyền", "Bạn không có quyền duyệt loại phiếu này.", isError: true);

        var isApprove = string.Equals(action, "approve", StringComparison.OrdinalIgnoreCase);
        if (!isApprove && string.IsNullOrWhiteSpace(remarks))
            return HtmlResult("Thiếu lý do", "Khi Deny bắt buộc nhập lý do.", isError: true);

        var actorName = approver.Name ?? approver.Usr ?? approver.Email ?? userId.ToString();
        var now = DateTime.Now;

        if (snapshot.Loai == LoaiThu)
        {
            var item = await db.Phieuthu.FirstAsync(x => x.PhieuthuID == snapshot.PhieuId);
            if (IsAlreadyDecided(item.Approve, item.ApproveBy, item.ApproveDate))
                return HtmlResult("Đã xử lý", $"Phiếu thu đã được xử lý bởi <b>{WebUtility.HtmlEncode(item.ApproveBy)}</b>.", isError: false);

            item.Approve = isApprove;
            item.ApproveBy = actorName;
            item.ApproveByUserId = userId;
            item.ApproveDate = now;
            item.Remarks = isApprove ? null : remarks!.Trim();
            item.Trangthai = isApprove ? "DaDuyet" : "TuChoi";
            item.UserUpdate = actorName;
            item.DateUpdate = now.ToString("dd/MMM/yyyy");
            await db.SaveChangesAsync();

            await NotifyWatchersAndOtherApproversAsync(db, LoaiThu, BuildThuSummary(item), isApprove, actorName, item.Remarks, userId);
            return HtmlResult(
                isApprove ? "Đã Approve" : "Đã Deny",
                $"Bạn đã {(isApprove ? "Approve" : "Deny")} phiếu thu <b>{WebUtility.HtmlEncode(item.SoPhieuthu)}</b>.",
                isError: false);
        }
        else
        {
            var item = await db.Phieuchi.FirstAsync(x => x.PhieuchiID == snapshot.PhieuId);
            if (IsAlreadyDecided(item.Approve, item.ApproveBy, item.ApproveDate))
                return HtmlResult("Đã xử lý", $"Phiếu chi đã được xử lý bởi <b>{WebUtility.HtmlEncode(item.ApproveBy)}</b>.", isError: false);

            item.Approve = isApprove;
            item.ApproveBy = actorName;
            item.ApproveByUserId = userId;
            item.ApproveDate = now;
            item.Remarks = isApprove ? null : remarks!.Trim();
            item.Trangthai = isApprove ? "DaDuyet" : "TuChoi";
            item.Useupdate = actorName;
            item.DateUpdate = now.ToString("dd/MMM/yyyy");
            await db.SaveChangesAsync();

            await NotifyWatchersAndOtherApproversAsync(db, LoaiChi, BuildChiSummary(item), isApprove, actorName, item.Remarks, userId);
            return HtmlResult(
                isApprove ? "Đã Approve" : "Đã Deny",
                $"Bạn đã {(isApprove ? "Approve" : "Deny")} phiếu chi <b>{WebUtility.HtmlEncode(item.Sophieuchi)}</b>.",
                isError: false);
        }
    }

    private async Task NotifyWatchersAndOtherApproversAsync(
        AppDbContext ctx,
        string loai,
        string summaryHtml,
        bool isApprove,
        string actorName,
        string? denyReason,
        Guid actorUserId)
    {
        var statusText = isApprove ? "APPROVE" : "DENY";
        var subject = loai == LoaiThu
            ? $"[Phiếu thu] Kết quả duyệt: {statusText} bởi {actorName}"
            : $"[Phiếu chi] Kết quả duyệt: {statusText} bởi {actorName}";

        var body = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#0f172a'>
  <p>Phiếu đã được <b style='color:{(isApprove ? "#059669" : "#dc2626")}'>{statusText}</b> bởi <b>{WebUtility.HtmlEncode(actorName)}</b>.</p>
  {(isApprove ? "" : $"<p>Lý do Deny: <b>{WebUtility.HtmlEncode(denyReason)}</b></p>")}
  {summaryHtml}
</div>";

        var watchers = await GetWatcherEmailsAsync(ctx);
        if (watchers.Count > 0)
            await SendMailWithContextAsync(ctx, watchers, new List<string>(), subject, body);

        // Đồng bộ in-app: báo các approver còn lại
        var otherUsers = (await GetApproversWithContextAsync(ctx, loai))
            .Where(u => u.UsrId != actorUserId)
            .ToList();

        if (otherUsers.Count > 0)
        {
            var syncSubject = loai == LoaiThu
                ? $"[Phiếu thu] Đã được {statusText} — không cần xử lý thêm"
                : $"[Phiếu chi] Đã được {statusText} — không cần xử lý thêm";
            var syncBody = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#0f172a'>
  <p>Phiếu bạn nhận yêu cầu duyệt đã được <b>{statusText}</b> bởi <b>{WebUtility.HtmlEncode(actorName)}</b>.</p>
  <p>Bạn <b>không cần</b> Approve/Deny nữa. Link trong email cũ sẽ hiển thị thông báo đã xử lý.</p>
  {summaryHtml}
</div>";
            var otherEmails = otherUsers
                .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                .Select(u => u.Email!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (otherEmails.Count > 0)
                await SendMailWithContextAsync(ctx, otherEmails, new List<string>(), syncSubject, syncBody);

            var kind = loai == LoaiThu ? "phiếu thu" : "phiếu chi";
            var inAppMsg = $"[{kind.ToUpperInvariant()}] Đã được {statusText} bởi {actorName}. Bạn không cần duyệt thêm.";
            foreach (var u in otherUsers)
            {
                ctx.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    SenderUserId = actorUserId,
                    ReceiverUserId = u.UsrId,
                    Message = inAppMsg,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });
            }
            await ctx.SaveChangesAsync();
        }
    }

    private async Task<BoolandMessReponse> SendRequestEmailsAsync(
        string loai,
        string token,
        string summaryHtml,
        List<AuthUser> approvers,
        string requestedBy,
        string? baseUrl)
    {
        var title = loai == LoaiThu ? "Yêu cầu duyệt phiếu thu" : "Yêu cầu duyệt phiếu chi";
        var errors = new List<string>();
        var sent = 0;
        var root = NormalizeBaseUrl(baseUrl) ?? GetBaseUrl();
        if (string.IsNullOrWhiteSpace(root))
            return new BoolandMessReponse(false, "Không lấy được Base URL để tạo link duyệt trong email.");

        foreach (var user in approvers)
        {
            var email = user.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                errors.Add($"{user.Name ?? user.Usr}: email không hợp lệ ({email})");
                continue;
            }

            var link = $"{root}/api/phieu-approve/{token}?uid={user.UsrId:N}";
            var body = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#0f172a'>
  <p>Xin chào <b>{WebUtility.HtmlEncode(user.Name ?? user.Usr)}</b>,</p>
  <p><b>{WebUtility.HtmlEncode(requestedBy)}</b> gửi yêu cầu duyệt phiếu.</p>
  {summaryHtml}
  <p style='margin-top:18px'>
    <a href='{link}' style='background:#0284c7;color:#fff;padding:12px 18px;border-radius:8px;text-decoration:none;font-weight:700'>
      Mở trang Approve / Deny
    </a>
  </p>
  <p style='color:#64748b;font-size:12px;margin-top:12px'>Nếu một người đã Approve/Deny, những người còn lại sẽ không duyệt được nữa.</p>
  <p style='color:#94a3b8;font-size:11px'>Link: {WebUtility.HtmlEncode(link)}</p>
</div>";

            var rs = await gsv.SendMail(new List<string> { email }, new List<string>(), title, body);
            if (rs.Flag) sent++;
            else errors.Add($"{email}: {rs.Message}");
        }

        if (sent == 0)
            return new BoolandMessReponse(false, "Không gửi được email nào. " + string.Join("; ", errors));
        if (errors.Count > 0)
            return new BoolandMessReponse(true, $"Đã gửi {sent}/{approvers.Count} email. Lỗi: {string.Join("; ", errors)}");
        return new BoolandMessReponse(true, $"Đã gửi {sent} email.");
    }

    private async Task<List<AuthUser>> GetApproversAsync(string loai) =>
        await GetApproversWithContextAsync(db, loai);

    private static async Task<List<AuthUser>> GetApproversWithContextAsync(AppDbContext ctx, string loai)
    {
        var q = ctx.UserList.AsNoTracking().AsQueryable();
        if (loai == LoaiThu)
            q = q.Where(u => u.Duyet_Phieu_Thu == true);
        else
            q = q.Where(u => u.Duyet_Phieu_Chi == true);
        return await q.OrderBy(u => u.Name).ToListAsync();
    }

    private static bool IsUserAllowed(AuthUser user, string loai) =>
        loai == LoaiThu ? user.Duyet_Phieu_Thu == true : user.Duyet_Phieu_Chi == true;

    private static async Task<List<string>> GetWatcherEmailsAsync(AppDbContext ctx)
    {
        var company = await ctx.CompanyInfomation.AsNoTracking().FirstOrDefaultAsync();
        if (string.IsNullOrWhiteSpace(company?.ListEmail_nhanTB_Approve_Thu_Chi))
            return new List<string>();

        return company.ListEmail_nhanTB_Approve_Thu_Chi
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<PhieuSnapshot?> LoadSnapshotAsync(AppDbContext ctx, string token, bool track = false)
    {
        ctx.ChangeTracker.Clear();
        if (track)
        {
            var thu = await ctx.Phieuthu.FirstOrDefaultAsync(x => x.ApproveToken == token);
            if (thu != null)
            {
                return new PhieuSnapshot
                {
                    Loai = LoaiThu,
                    PhieuId = thu.PhieuthuID,
                    SoPhieu = thu.SoPhieuthu,
                    Approve = thu.Approve,
                    ApproveBy = thu.ApproveBy,
                    ApproveDate = thu.ApproveDate,
                    Remarks = thu.Remarks,
                    DetailHtml = BuildThuSummary(thu)
                };
            }

            var chi = await ctx.Phieuchi.FirstOrDefaultAsync(x => x.ApproveToken == token);
            if (chi != null)
            {
                return new PhieuSnapshot
                {
                    Loai = LoaiChi,
                    PhieuId = chi.PhieuchiID,
                    SoPhieu = chi.Sophieuchi,
                    Approve = chi.Approve,
                    ApproveBy = chi.ApproveBy,
                    ApproveDate = chi.ApproveDate,
                    Remarks = chi.Remarks,
                    DetailHtml = BuildChiSummary(chi)
                };
            }
            return null;
        }

        var thuRo = await ctx.Phieuthu.AsNoTracking().FirstOrDefaultAsync(x => x.ApproveToken == token);
        if (thuRo != null)
        {
            return new PhieuSnapshot
            {
                Loai = LoaiThu,
                PhieuId = thuRo.PhieuthuID,
                SoPhieu = thuRo.SoPhieuthu,
                Approve = thuRo.Approve,
                ApproveBy = thuRo.ApproveBy,
                ApproveDate = thuRo.ApproveDate,
                Remarks = thuRo.Remarks,
                DetailHtml = BuildThuSummary(thuRo)
            };
        }

        var chiRo = await ctx.Phieuchi.AsNoTracking().FirstOrDefaultAsync(x => x.ApproveToken == token);
        if (chiRo == null) return null;
        return new PhieuSnapshot
        {
            Loai = LoaiChi,
            PhieuId = chiRo.PhieuchiID,
            SoPhieu = chiRo.Sophieuchi,
            Approve = chiRo.Approve,
            ApproveBy = chiRo.ApproveBy,
            ApproveDate = chiRo.ApproveDate,
            Remarks = chiRo.Remarks,
            DetailHtml = BuildChiSummary(chiRo)
        };
    }

    private static bool IsAlreadyDecided(PhieuSnapshot s) =>
        IsAlreadyDecided(s.Approve, s.ApproveBy, s.ApproveDate);

    private static bool IsAlreadyDecided(bool? approve, string? by, DateTime? date) =>
        date.HasValue || (!string.IsNullOrWhiteSpace(by) && approve.HasValue);

    private static string BuildThuSummary(M_PhieuThu x) => $@"
<table>
  <tr><td class='k'>Số phiếu thu</td><td>{WebUtility.HtmlEncode(x.SoPhieuthu)}</td></tr>
  <tr><td class='k'>Ngày</td><td>{(x.Ngay.HasValue ? x.Ngay.Value.ToString("dd/MM/yyyy") : "")}</td></tr>
  <tr><td class='k'>Branch</td><td>{WebUtility.HtmlEncode(x.Branch)}</td></tr>
  <tr><td class='k'>Khách hàng</td><td>{WebUtility.HtmlEncode(x.Customername)}</td></tr>
  <tr><td class='k'>Nội dung</td><td>{WebUtility.HtmlEncode(x.Noidung)}</td></tr>
  <tr><td class='k'>Số tiền</td><td>{x.Sotien:N2} {WebUtility.HtmlEncode(x.Currency)}</td></tr>
  <tr><td class='k'>PTTT</td><td>{WebUtility.HtmlEncode(x.PTTT)}</td></tr>
  <tr><td class='k'>Bank</td><td>{WebUtility.HtmlEncode(x.Bank)}</td></tr>
  <tr><td class='k'>Người nộp</td><td>{WebUtility.HtmlEncode(x.Nguoinoptien)}</td></tr>
</table>";

    private static string BuildChiSummary(M_PhieuChi x) => $@"
<table>
  <tr><td class='k'>Số phiếu chi</td><td>{WebUtility.HtmlEncode(x.Sophieuchi)}</td></tr>
  <tr><td class='k'>Ngày</td><td>{(x.Ngay.HasValue ? x.Ngay.Value.ToString("dd/MM/yyyy") : "")}</td></tr>
  <tr><td class='k'>Branch</td><td>{WebUtility.HtmlEncode(x.Branch)}</td></tr>
  <tr><td class='k'>Nội dung</td><td>{WebUtility.HtmlEncode(x.Noidung)}</td></tr>
  <tr><td class='k'>Số tiền</td><td>{x.Sotien:N2} {WebUtility.HtmlEncode(x.Currency)}</td></tr>
  <tr><td class='k'>PTTT</td><td>{WebUtility.HtmlEncode(x.PTTT)}</td></tr>
  <tr><td class='k'>Bank</td><td>{WebUtility.HtmlEncode(x.Bank)}</td></tr>
  <tr><td class='k'>Người nhận</td><td>{WebUtility.HtmlEncode(x.Nguoinoptien)}</td></tr>
  <tr><td class='k'>HBL</td><td>{WebUtility.HtmlEncode(x.Hbl)}</td></tr>
</table>";

    private static string HtmlResult(string title, string message, bool isError) => $@"
<!DOCTYPE html><html><head><meta charset='utf-8'/><meta name='viewport' content='width=device-width,initial-scale=1'/>
<title>{WebUtility.HtmlEncode(title)}</title>
<style>
body{{font-family:Segoe UI,Arial,sans-serif;background:#f1f5f9;margin:0;padding:24px}}
.card{{max-width:560px;margin:40px auto;background:#fff;border-radius:12px;padding:24px;box-shadow:0 8px 24px rgba(15,23,42,.08);border-left:6px solid {(isError ? "#dc2626" : "#059669")}}}
h1{{margin:0 0 12px;font-size:20px}}
p{{line-height:1.5;color:#334155}}
</style></head><body><div class='card'><h1>{WebUtility.HtmlEncode(title)}</h1><p>{message}</p></div></body></html>";

    private string GetBaseUrl()
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request == null) return "";
        return $"{request.Scheme}://{request.Host}{request.PathBase}".TrimEnd('/');
    }

    private static string? NormalizeBaseUrl(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return null;
        return baseUrl.Trim().TrimEnd('/');
    }

    private async Task<BoolandMessReponse> SendMailWithContextAsync(AppDbContext ctx, List<string> to, List<string> toCc, string subject, string body)
    {
        try
        {
            var companyInfo = await ctx.CompanyInfomation.AsNoTracking().FirstOrDefaultAsync();
            if (companyInfo == null)
                return new BoolandMessReponse(false, "Missing company info.");

            // Reuse GlobalServices encryption path by temporarily using gsv when same tenant;
            // for anonymous tenant DB, decrypt via same helper through gsv.SendMail is not available.
            // Call gsv only when connection matches current tenant; otherwise send directly.
            if (tenantContext.TenantId != null &&
                string.Equals(tenantContext.ConnectionString, ctx.Database.GetConnectionString(), StringComparison.OrdinalIgnoreCase))
            {
                return await gsv.SendMail(to, toCc, subject, body);
            }

            var senderEmail = companyInfo.Email_GuiTB?.Trim();
            var senderPassword = DecryptPassword(companyInfo.PasswordEmail_GuiTB)?.Trim().Replace(" ", "");
            var smtpServer = companyInfo.SmtpServer?.Trim();
            var smtpPort = companyInfo.SmtpPort;
            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword) ||
                string.IsNullOrWhiteSpace(smtpServer) || !smtpPort.HasValue)
                return new BoolandMessReponse(false, "Missing SMTP configuration.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("", senderEmail));
            foreach (var mail in to.Where(x => !string.IsNullOrWhiteSpace(x)))
                message.To.Add(MailboxAddress.Parse(mail.Trim()));
            foreach (var mail in toCc.Where(x => !string.IsNullOrWhiteSpace(x)))
                message.Cc.Add(MailboxAddress.Parse(mail.Trim()));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (_, _, _, _) => true;
            var secure = smtpPort.Value == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            await client.ConnectAsync(smtpServer, smtpPort.Value, secure);
            await client.AuthenticateAsync(senderEmail, senderPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return new BoolandMessReponse(true, "Send mail successfully!");
        }
        catch (Exception ex)
        {
            return new BoolandMessReponse(false, "Send mail failed: " + ex.Message);
        }
    }

    private static string? DecryptPassword(string? encrypted)
    {
        if (string.IsNullOrWhiteSpace(encrypted)) return encrypted;
        try { return EncryptionHelper.Decrypt(encrypted); }
        catch { return encrypted; }
    }

    private sealed class PhieuSnapshot
    {
        public string Loai { get; set; } = "";
        public Guid PhieuId { get; set; }
        public string? SoPhieu { get; set; }
        public bool? Approve { get; set; }
        public string? ApproveBy { get; set; }
        public DateTime? ApproveDate { get; set; }
        public string? Remarks { get; set; }
        public string DetailHtml { get; set; } = "";
    }
}

public class PhieuApproveInAppModel
{
    public string Loai { get; set; } = "";
    public Guid PhieuId { get; set; }
    public string Token { get; set; } = "";
    public string? SoPhieu { get; set; }
    public string DetailHtml { get; set; } = "";
    public bool? Approve { get; set; }
    public string? ApproveBy { get; set; }
    public DateTime? ApproveDate { get; set; }
    public string? Remarks { get; set; }
    public bool AlreadyDecided { get; set; }
    public bool CanDecide { get; set; }
}
