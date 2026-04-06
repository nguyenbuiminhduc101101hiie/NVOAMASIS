// Services/HistoryLogService.cs (Blazor Client)
using System.Text.Json;
using NVOAMASIS.Models;

public class HistoryLogService
{
    private readonly HttpClient _http;

    public HistoryLogService(HttpClient http)
    {
        _http = http;
    }

    public async Task LogAsync(string userName, string action, string entityName, Guid? entityId,string? No, object? changes = null)
    {
        var log = new
        {
            UserName = userName,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            No=No,
            Changes = changes != null ? JsonSerializer.Serialize(changes) : null,
            Timestamp = DateTime.Now
        };

        await _http.PostAsJsonAsync("api/HistoryLog", log);
    }

    //// ✅ Hàm gọi API GET để lấy toàn bộ log
    //public async Task<List<HistoryLog>?> GetAllLogsAsync()
    //{
    //    var response = await _http.GetAsync("api/HistoryLog");
    //    if (!response.IsSuccessStatusCode)
    //        return null;

    //    var json = await response.Content.ReadAsStringAsync();
    //    return JsonSerializer.Deserialize<List<HistoryLog>>(json, new JsonSerializerOptions
    //    {
    //        PropertyNameCaseInsensitive = true
    //    });
    //}

}
