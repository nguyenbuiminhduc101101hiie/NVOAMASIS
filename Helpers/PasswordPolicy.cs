using System.Text.RegularExpressions;

namespace NVOAMASIS.Helpers;

public static class PasswordPolicy
{
    /// <summary>Mật khẩu: hơn 8 ký tự, ít nhất 1 chữ hoa, 1 chữ thường, 1 số.</summary>
    public static bool IsValid(string? password, out string? errorMessage)
    {
        foreach (var message in GetValidationErrors(password))
        {
            errorMessage = message;
            return false;
        }

        errorMessage = null;
        return true;
    }

    public static IEnumerable<string> GetValidationErrors(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            yield return "Nhập password.";
            yield break;
        }

        if (password.Length <= 8)
            yield return "Password phải có hơn 8 ký tự.";

        if (!Regex.IsMatch(password, @"[A-Z]"))
            yield return "Password phải có ít nhất 1 chữ hoa (A-Z).";

        if (!Regex.IsMatch(password, @"[a-z]"))
            yield return "Password phải có ít nhất 1 chữ thường (a-z).";

        if (!Regex.IsMatch(password, @"[0-9]"))
            yield return "Password phải có ít nhất 1 chữ số (0-9).";
    }
}
