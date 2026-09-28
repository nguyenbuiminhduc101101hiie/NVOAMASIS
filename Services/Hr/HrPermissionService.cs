using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models.Hr;

namespace NVOAMASIS.Services.Hr
{
    /// <summary>
    /// Đọc quyền HR từ bảng Permissions (cùng cơ chế SharedServices.CheckPermission),
    /// nhưng không phụ thuộc NavigationManager/IJSRuntime nên dùng được cả trong Controller.
    /// </summary>
    public sealed class HrPermissionService(IDbContextFactory<AppDbContext> dbFactory)
    {
        public async Task<HrPerm> GetAsync(string? username, string menuName)
        {
            var map = await GetManyAsync(username, menuName);
            return map.TryGetValue(menuName, out var p) ? p : HrPerm.None;
        }

        public async Task<Dictionary<string, HrPerm>> GetManyAsync(string? username, params string[] menuNames)
        {
            var result = menuNames.Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x, _ => HrPerm.None, StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(username) || menuNames.Length == 0)
                return result;

            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var rows = await db.Permissions
                    .AsNoTracking()
                    .Where(x => x.UserName == username && x.MenuName != null && menuNames.Contains(x.MenuName))
                    .Select(x => new { x.MenuName, x.See, x.Add, x.Edit, x.Del, x.Approve })
                    .ToListAsync();

                foreach (var r in rows)
                {
                    result[r.MenuName!] = new HrPerm(
                        r.See == true, r.Add == true, r.Edit == true, r.Del == true, r.Approve == true);
                }
            }
            catch
            {
                // Lỗi đọc quyền → coi như không có quyền.
            }

            return result;
        }
    }
}
