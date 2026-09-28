namespace NVOAMASIS.Services.Hr
{
    /// <summary>
    /// Lưu giấy tờ nhân sự trên đĩa, NGOÀI wwwroot (không truy cập trực tiếp qua URL tĩnh):
    /// {Hr:StorageRoot | ContentRoot/App_Data/hr}/{tenantKey}/{employeeId}/{documentId}_{safeName}
    /// Tải về qua HrDocumentController (kiểm tra quyền HR_Employee).
    /// </summary>
    public sealed class HrFileStorage
    {
        public const long MaxFileBytes = 20L * 1024 * 1024;

        public static readonly string[] AllowedExtensions =
        {
            ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".zip", ".rar"
        };

        private readonly string _root;
        private readonly IConfiguration _configuration;

        public HrFileStorage(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            var configured = configuration["Hr:StorageRoot"];
            _root = Path.GetFullPath(string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(env.ContentRootPath, "App_Data", "hr")
                : Path.Combine(env.ContentRootPath, configured));
        }

        /// <summary>Thư mục theo tenant (giống ChatIdentity): claim TenantId hoặc "default".</summary>
        public string GetTenantKey(System.Security.Claims.ClaimsPrincipal? principal)
        {
            if (!_configuration.GetValue("MultiTenant:Enabled", true))
                return "default";
            var raw = principal?.FindFirst(NVOAMASIS.Services.MultiTenant.TenantClaimTypes.TenantId)?.Value;
            return Guid.TryParse(raw, out var tenantId) ? tenantId.ToString("N") : "default";
        }

        public static bool IsAllowed(string fileName) =>
            AllowedExtensions.Contains(Path.GetExtension(fileName ?? "").ToLowerInvariant());

        /// <returns>Đường dẫn tương đối lưu vào HrDocument.FilePath.</returns>
        public async Task<string> SaveAsync(string tenantKey, Guid employeeId, Guid documentId,
            string fileName, Stream content, CancellationToken cancellationToken = default)
        {
            var relativePath = Path.Combine(
                SafeSegment(tenantKey),
                employeeId.ToString("N"),
                $"{documentId:N}_{SafeFileName(fileName)}");
            var fullPath = ResolveFullPath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            try
            {
                await using var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write,
                    FileShare.None, 81920, useAsync: true);
                var buffer = new byte[81920];
                long total = 0;
                int read;
                while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    total += read;
                    if (total > MaxFileBytes)
                        throw new InvalidOperationException("hr_doc_too_large");
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

        public Stream OpenRead(string relativePath) =>
            new FileStream(ResolveFullPath(relativePath), FileMode.Open, FileAccess.Read, FileShare.Read,
                81920, useAsync: true);

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
                // File mồ côi không ảnh hưởng dữ liệu.
            }
        }

        public void TryDeleteEmployeeFolder(string tenantKey, Guid employeeId)
        {
            try
            {
                var dir = ResolveFullPath(Path.Combine(SafeSegment(tenantKey), employeeId.ToString("N")));
                if (Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
            catch
            {
                // bỏ qua
            }
        }

        private string ResolveFullPath(string relativePath)
        {
            var fullPath = Path.GetFullPath(Path.Combine(_root, relativePath));
            if (!fullPath.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                throw new UnauthorizedAccessException("hr_doc_path_invalid");
            return fullPath;
        }

        private static string SafeSegment(string value)
        {
            var cleaned = new string((value ?? "").Where(char.IsLetterOrDigit).ToArray());
            return cleaned.Length == 0 ? "default" : cleaned;
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
}
