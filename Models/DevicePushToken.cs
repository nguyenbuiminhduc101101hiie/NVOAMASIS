using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models;

/// <summary>FCM/APNs device token đăng ký từ Flutter.</summary>
public class DevicePushToken
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = "";
    public string? Platform { get; set; } // android | ios
    public DateTime UpdatedAtUtc { get; set; }
}
