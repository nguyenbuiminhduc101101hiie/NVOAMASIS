namespace NVOAMASIS.Services.Chat;

/// <summary>
/// Lưu file đính kèm chat trên đĩa, ngoài wwwroot:
/// {Chat:StorageRoot | ContentRoot/App_Data/chat}/{tenantKey}/{conversationId}/{messageId}_{safeName}
/// </summary>
public sealed class ChatFileStorage
{
    public const long MaxAttachmentBytes = 25L * 1024 * 1024;

    private readonly string _root;

    public ChatFileStorage(IConfiguration configuration, IWebHostEnvironment env)
    {
        var configured = configuration["Chat:StorageRoot"];
        _root = Path.GetFullPath(string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.ContentRootPath, "App_Data", "chat")
            : Path.Combine(env.ContentRootPath, configured));
    }

    /// <returns>Đường dẫn tương đối để lưu vào ChatMessages.AttachmentPath.</returns>
    public async Task<string> SaveAsync(
        string tenantKey,
        Guid conversationId,
        Guid messageId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken)
    {
        var relativePath = Path.Combine(
            SafeSegment(tenantKey),
            conversationId.ToString("N"),
            $"{messageId:N}_{SafeFileName(fileName)}");
        var fullPath = ResolveFullPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        try
        {
            await using var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            var buffer = new byte[81920];
            long total = 0;
            int read;
            while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
            {
                total += read;
                if (total > MaxAttachmentBytes)
                    throw new ArgumentException("chat_attachment_too_large");
                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }
        }
        catch
        {
            TryDelete(relativePath);
            throw;
        }

        return relativePath;
    }

    /// <summary>Sao chép file sang tin mới (chuyển tiếp) — bản sao độc lập với bản gốc khi thu hồi/giải tán.</summary>
    public async Task<string> CopyAsync(
        string sourceRelativePath,
        string tenantKey,
        Guid conversationId,
        Guid messageId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        await using var source = OpenRead(sourceRelativePath);
        return await SaveAsync(tenantKey, conversationId, messageId, fileName, source, cancellationToken);
    }

    public Stream OpenRead(string relativePath) =>
        new FileStream(ResolveFullPath(relativePath), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);

    public void TryDelete(string relativePath)
    {
        try
        {
            var fullPath = ResolveFullPath(relativePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch
        {
            // Bỏ qua: file mồ côi không ảnh hưởng dữ liệu chat.
        }
    }

    /// <summary>Xoá toàn bộ file của một hội thoại (giải tán nhóm).</summary>
    public void TryDeleteConversation(string tenantKey, Guid conversationId)
    {
        try
        {
            var directory = ResolveFullPath(Path.Combine(SafeSegment(tenantKey), conversationId.ToString("N")));
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
        catch
        {
            // Bỏ qua: thư mục mồ côi không ảnh hưởng dữ liệu chat.
        }
    }

    private string ResolveFullPath(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root, relativePath));
        if (!fullPath.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("chat_attachment_path_invalid");
        return fullPath;
    }

    private static string SafeSegment(string value)
    {
        var cleaned = new string(value.Where(char.IsLetterOrDigit).ToArray());
        return cleaned.Length == 0 ? ChatIdentity.DefaultTenantKey : cleaned;
    }

    private static string SafeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName ?? "");
        var invalid = Path.GetInvalidFileNameChars();
        name = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
        if (name.Length == 0)
            name = "file";
        return name.Length > 150 ? name[^150..] : name;
    }
}
