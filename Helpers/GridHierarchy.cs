using System.Reflection;
using MudBlazor;

namespace NVOAMASIS.Helpers;

/// <summary>
/// Mở / đóng dòng con (HierarchyColumn) của MudDataGrid bằng code.
/// MudBlazor 6.12 chưa có API public cho việc này (ToggleHierarchyVisibilityAsync là internal),
/// nên gọi qua reflection; nếu phiên bản MudBlazor khác không còn các thành viên này thì bỏ qua, không lỗi.
/// </summary>
public static class GridHierarchy
{
    const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static async Task SetExpandedAsync<T>(MudDataGrid<T>? grid, IEnumerable<T> items, bool expanded)
    {
        if (grid == null)
            return;
        try
        {
            var type = grid.GetType();
            var toggle = type.GetMethod("ToggleHierarchyVisibilityAsync", Flags);
            var open = type.GetField("_openHierarchies", Flags)?.GetValue(grid) as HashSet<T>;
            if (toggle == null || open == null)
                return;

            foreach (var item in items.ToList())
            {
                if (item == null || open.Contains(item) == expanded)
                    continue;
                if (toggle.Invoke(grid, new object[] { item }) is Task task)
                    await task;
            }
        }
        catch
        {
            // Không mở được thì giữ nguyên giao diện cũ (người dùng vẫn bấm mở tay được)
        }
    }
}
