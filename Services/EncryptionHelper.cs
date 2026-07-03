using System.Security.Cryptography;
using System.Text;

namespace NVOAMASIS.Services
{
    public static class EncryptionHelper
    {
        private static readonly string Key = "gsKeyEncryptionLogisticssoftwareQWE!@#123";

        private static byte[] GetAesKey()
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(Key));
        }

        public static string Encrypt(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText ?? "";

            using var aes = Aes.Create();
            aes.Key = GetAesKey();
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var cipher = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            var full = aes.IV.Concat(cipher).ToArray();

            return Convert.ToBase64String(full);
        }

        public static string Decrypt(string? cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText ?? "";

            var full = Convert.FromBase64String(cipherText);
            var iv = full.Take(16).ToArray();
            var cipher = full.Skip(16).ToArray();

            using var aes = Aes.Create();
            aes.Key = GetAesKey();
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        // Chuẩn hóa text để dùng cho LIKE
        public static string Normalize(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text ?? "";

            text = text.ToLowerInvariant();
            text = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in text)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                    != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
