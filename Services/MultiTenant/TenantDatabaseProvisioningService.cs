using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;

namespace NVOAMASIS.Services.MultiTenant;

public class TenantDatabaseProvisioningService(
    IConfiguration configuration,
    RegistryDbContext registryDb,
    ILogger<TenantDatabaseProvisioningService> logger)
{
    public async Task<BoolandMessReponse> ProvisionNewTenantAsync(
        string databaseName,
        string sqlUserId,
        string sqlPassword,
        AuthUser adminUser,
        CancellationToken cancellationToken = default)
    {
        databaseName = databaseName.Trim();
        if (string.IsNullOrWhiteSpace(databaseName))
            return new BoolandMessReponse(false, "Tên database không được để trống.");

        if (!IsValidDatabaseName(databaseName))
            return new BoolandMessReponse(false, "Tên database chỉ được chứa chữ, số và dấu gạch dưới.");

        sqlUserId = sqlUserId.Trim();
        if (string.IsNullOrWhiteSpace(sqlUserId))
            return new BoolandMessReponse(false, "Nhập SQL user để đăng nhập database.");
        if (string.IsNullOrWhiteSpace(sqlPassword))
            return new BoolandMessReponse(false, "Nhập SQL password.");
        if (!IsValidSqlLoginName(sqlUserId))
            return new BoolandMessReponse(false, "SQL user chỉ được chứa chữ, số và dấu gạch dưới (không dùng sa).");

        var server = configuration["MultiTenant:Server"] ?? "logisticssoftware.vn";
        var templateDb = configuration["MultiTenant:TemplateDatabase"] ?? "nvoamasis";
        var configuredBackupFolder = configuration["MultiTenant:BackupFolder"];

        if (await registryDb.TenantDatabases.AnyAsync(x => x.DatabaseName == databaseName, cancellationToken))
            return new BoolandMessReponse(false, "Database này đã được đăng ký trong hệ thống.");

        var saUser = configuration["MultiTenant:ProvisionerUser"] ?? "sa";
        var saPass = configuration["MultiTenant:ProvisionerPassword"]
            ?? throw new InvalidOperationException("MultiTenant:ProvisionerPassword is required.");

        var masterConn = TenantConnectionStringBuilder.BuildMaster(server, saUser, saPass);

        try
        {
            await using (var conn = new SqlConnection(masterConn))
            {
                await conn.OpenAsync(cancellationToken);

                if (await DatabaseExistsAsync(conn, databaseName, cancellationToken))
                    return new BoolandMessReponse(false, "Database đã tồn tại trên SQL Server.");

                var backupFolder = await ResolveBackupFolderAsync(
                    conn, configuredBackupFolder, cancellationToken);
                var backupFile = Path.Combine(
                    backupFolder, $"{templateDb}_clone_{Guid.NewGuid():N}.bak");
                logger.LogInformation(
                    "Tenant backup using folder on SQL Server: {BackupFolder}", backupFolder);

                await CopyDatabaseViaBackupRestoreAsync(
                    conn, templateDb, databaseName, backupFile, cancellationToken);

                try { File.Delete(backupFile); } catch { /* best effort */ }

                await CreateSqlLoginAndGrantDbOwnerAsync(
                    conn, databaseName, sqlUserId, sqlPassword, cancellationToken);
            }

            var canConnect = await TestSqlConnectionAsync(
                server, databaseName, sqlUserId, sqlPassword, cancellationToken);
            if (!canConnect)
                return new BoolandMessReponse(false,
                    "Đã tạo database nhưng không kết nối được bằng SQL user vừa cấp. Kiểm tra lại user/password.");

            var tenant = new TenantDatabaseRegistry
            {
                TenantId = Guid.NewGuid(),
                DatabaseName = databaseName,
                ServerName = server,
                SqlUserId = sqlUserId.Trim(),
                SqlPassword = sqlPassword,
                DisplayName = databaseName,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByAppUser = adminUser.Usr ?? adminUser.Name,
                IsActive = true
            };
            registryDb.TenantDatabases.Add(tenant);
            await registryDb.SaveChangesAsync(cancellationToken);

            var tenantConn = TenantConnectionStringBuilder.Build(
                server, databaseName, sqlUserId, sqlPassword);

            await CreateInitialAppUserAsync(tenantConn, adminUser, cancellationToken);

            return new BoolandMessReponse(true,
                $"Đã tạo database [{databaseName}], cấp quyền db_owner cho SQL user [{sqlUserId}] và tài khoản ứng dụng.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Provision tenant database failed for {DatabaseName}", databaseName);
            return new BoolandMessReponse(false, $"Không thể tạo database: {ex.Message}");
        }
    }

    public async Task<TenantDatabaseRegistry?> ResolveTenantAsync(
        string databaseName,
        string sqlUserId,
        string sqlPassword,
        CancellationToken cancellationToken = default)
    {
        databaseName = databaseName.Trim();
        sqlUserId = sqlUserId.Trim();

        var row = await registryDb.TenantDatabases.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.DatabaseName == databaseName &&
                x.SqlUserId == sqlUserId &&
                x.IsActive,
                cancellationToken);

        if (row == null) return null;
        if (!string.Equals(row.SqlPassword, sqlPassword, StringComparison.Ordinal))
            return null;

        return row;
    }

    public async Task<bool> TestSqlConnectionAsync(
        string server,
        string database,
        string sqlUserId,
        string sqlPassword,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connStr = TenantConnectionStringBuilder.Build(server, database, sqlUserId, sqlPassword);
            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidDatabaseName(string name) =>
        name.All(c => char.IsLetterOrDigit(c) || c == '_');

    private static bool IsValidSqlLoginName(string name) =>
        !name.Equals("sa", StringComparison.OrdinalIgnoreCase) &&
        name.All(c => char.IsLetterOrDigit(c) || c == '_');

    /// <summary>
    /// Dùng quyền sa: tạo LOGIN trên server, USER trong database tenant, gán role db_owner (full quyền trên DB đó).
    /// </summary>
    private static async Task CreateSqlLoginAndGrantDbOwnerAsync(
        SqlConnection masterConn,
        string databaseName,
        string sqlLogin,
        string sqlPassword,
        CancellationToken cancellationToken)
    {
        var bracketLogin = BracketIdentifier(sqlLogin);
        var escapedPassword = sqlPassword.Replace("'", "''");
        var bracketDb = BracketIdentifier(databaseName);

        await using (var loginCmd = masterConn.CreateCommand())
        {
            loginCmd.CommandText =
                $"""
                 IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @loginName)
                     CREATE LOGIN {bracketLogin} WITH PASSWORD = N'{escapedPassword}',
                         CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF, DEFAULT_DATABASE = {bracketDb};
                 ELSE
                     ALTER LOGIN {bracketLogin} WITH PASSWORD = N'{escapedPassword}',
                         CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF, DEFAULT_DATABASE = {bracketDb};
                 """;
            loginCmd.Parameters.AddWithValue("@loginName", sqlLogin);
            await loginCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var useDbCmd = masterConn.CreateCommand())
        {
            useDbCmd.CommandText = $"USE {bracketDb};";
            await useDbCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var userCmd = masterConn.CreateCommand())
        {
            userCmd.CommandText =
                $"""
                 USE {bracketDb};

                 IF EXISTS (
                     SELECT 1 FROM sys.database_principals
                     WHERE name = @loginName AND type IN ('S', 'U') AND name <> 'dbo')
                 BEGIN
                     DECLARE @dropSql NVARCHAR(MAX) = N'DROP USER ' + QUOTENAME(@loginName);
                     EXEC (@dropSql);
                 END

                 IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @loginName)
                     CREATE USER {bracketLogin} FOR LOGIN {bracketLogin};

                 IF NOT EXISTS (
                     SELECT 1 FROM sys.database_role_members rm
                     INNER JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id
                     INNER JOIN sys.database_principals m ON rm.member_principal_id = m.principal_id
                     WHERE r.name = N'db_owner' AND m.name = @loginName)
                     ALTER ROLE [db_owner] ADD MEMBER {bracketLogin};
                 """;
            userCmd.Parameters.AddWithValue("@loginName", sqlLogin);
            await userCmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static string BracketIdentifier(string name) =>
        "[" + name.Replace("]", "]]") + "]";

    /// <summary>
    /// Thư mục backup phải tồn tại trên máy chủ SQL (không phải máy chạy web app).
    /// </summary>
    private static async Task<string> ResolveBackupFolderAsync(
        SqlConnection masterConn,
        string? configuredFolder,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(configuredFolder))
            return NormalizeFolderPath(configuredFolder);

        return await GetDefaultBackupPathFromServerAsync(masterConn, cancellationToken);
    }

    private static async Task<string> GetDefaultBackupPathFromServerAsync(
        SqlConnection conn,
        CancellationToken cancellationToken)
    {
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText =
                """
                SELECT NULLIF(LTRIM(RTRIM(CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(512)))), '')
                """;
            var path = (string?)(await cmd.ExecuteScalarAsync(cancellationToken));
            if (!string.IsNullOrWhiteSpace(path))
                return NormalizeFolderPath(path);
        }

        await using (var regCmd = conn.CreateCommand())
        {
            regCmd.CommandText =
                """
                EXEC master.dbo.xp_instance_regread
                    N'HKEY_LOCAL_MACHINE',
                    N'Software\Microsoft\MSSQLServer\MSSQLServer',
                    N'BackupDirectory';
                """;
            await using var reader = await regCmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var path = reader["Data"]?.ToString();
                if (!string.IsNullOrWhiteSpace(path))
                    return NormalizeFolderPath(path);
            }
        }

        await using (var dataCmd = conn.CreateCommand())
        {
            dataCmd.CommandText =
                """
                SELECT NULLIF(LTRIM(RTRIM(CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR(512)))), '')
                """;
            var path = (string?)(await dataCmd.ExecuteScalarAsync(cancellationToken));
            if (!string.IsNullOrWhiteSpace(path))
                return NormalizeFolderPath(path);
        }

        throw new InvalidOperationException(
            "Không đọc được thư mục backup mặc định từ SQL Server. Cấu hình MultiTenant:BackupFolder trong appsettings (đường dẫn trên máy chủ SQL).");
    }

    private static string NormalizeFolderPath(string path)
    {
        path = path.Trim().TrimEnd('\\', '/');
        return path + Path.DirectorySeparatorChar;
    }

    private static async Task<bool> DatabaseExistsAsync(
        SqlConnection conn, string databaseName, CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM sys.databases WHERE name = @name";
        cmd.Parameters.AddWithValue("@name", databaseName);
        var count = (int)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0);
        return count > 0;
    }

    private static async Task CopyDatabaseViaBackupRestoreAsync(
        SqlConnection masterConn,
        string sourceDatabase,
        string targetDatabase,
        string backupFile,
        CancellationToken cancellationToken)
    {
        var escapedBackup = backupFile.Replace("'", "''");
        var escapedSource = sourceDatabase.Replace("]", "]]");
        var escapedTarget = targetDatabase.Replace("]", "]]");

        await using (var backupCmd = masterConn.CreateCommand())
        {
            backupCmd.CommandTimeout = 0;
            backupCmd.CommandText =
                $"BACKUP DATABASE [{escapedSource}] TO DISK = N'{escapedBackup}' WITH COPY_ONLY, INIT, STATS = 5";
            await backupCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        var fileMoves = new List<string>();
        await using (var listCmd = masterConn.CreateCommand())
        {
            listCmd.CommandText = $"RESTORE FILELISTONLY FROM DISK = N'{escapedBackup}'";
            await using var reader = await listCmd.ExecuteReaderAsync(cancellationToken);
            var logicalNames = new List<(string Logical, string Type)>();
            while (await reader.ReadAsync(cancellationToken))
            {
                logicalNames.Add((reader.GetString(0), reader.GetString(2)));
            }
            reader.Close();

            var dataIndex = 0;
            var logIndex = 0;
            foreach (var (logical, type) in logicalNames)
            {
                var escapedLogical = logical.Replace("]", "]]");
                if (type.Equals("D", StringComparison.OrdinalIgnoreCase))
                {
                    var physical = await GetDefaultDataPathAsync(masterConn, cancellationToken);
                    fileMoves.Add($"MOVE N'{escapedLogical}' TO N'{physical}{escapedTarget}_{dataIndex}.mdf'");
                    dataIndex++;
                }
                else if (type.Equals("L", StringComparison.OrdinalIgnoreCase))
                {
                    var physical = await GetDefaultLogPathAsync(masterConn, cancellationToken);
                    fileMoves.Add($"MOVE N'{escapedLogical}' TO N'{physical}{escapedTarget}_{logIndex}.ldf'");
                    logIndex++;
                }
            }
        }

        var moveClause = fileMoves.Count > 0 ? ", " + string.Join(", ", fileMoves) : "";
        await using var restoreCmd = masterConn.CreateCommand();
        restoreCmd.CommandTimeout = 0;
        restoreCmd.CommandText =
            $"""
             RESTORE DATABASE [{escapedTarget}]
             FROM DISK = N'{escapedBackup}'
             WITH REPLACE, RECOVERY{moveClause}, STATS = 5
             """;
        await restoreCmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<string> GetDefaultDataPathAsync(
        SqlConnection conn, CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR(512))";
        var path = (string?)(await cmd.ExecuteScalarAsync(cancellationToken));
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("Cannot resolve SQL Server default data path.");
        return path.EndsWith('\\') ? path : path + "\\";
    }

    private static async Task<string> GetDefaultLogPathAsync(
        SqlConnection conn, CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "SELECT CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS NVARCHAR(512))";
        var path = (string?)(await cmd.ExecuteScalarAsync(cancellationToken));
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("Cannot resolve SQL Server default log path.");
        return path.EndsWith('\\') ? path : path + "\\";
    }

    private static async Task CreateInitialAppUserAsync(
        string tenantConnectionString,
        AuthUser adminUser,
        CancellationToken cancellationToken)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(tenantConnectionString)
            .Options;

        await using var db = new AppDbContext(options);
        db.ChangeTracker.Clear();

        if (string.IsNullOrWhiteSpace(adminUser.Name))
            throw new InvalidOperationException("Tên người dùng ứng dụng là bắt buộc.");

        var exists = await db.UserList.AnyAsync(
            x => x.Usr != null && x.Usr.ToLower() == adminUser.Name.Trim().ToLower(),
            cancellationToken);
        if (exists)
            throw new InvalidOperationException("Tên đăng nhập ứng dụng đã tồn tại trong database mới.");

        adminUser.UsrId = Guid.NewGuid();
        adminUser.Usr = adminUser.Name.Trim();
        adminUser.NickName = adminUser.Name.Trim();
        adminUser.Manager2 ??= "";
        adminUser.Department ??= "ADMIN";
        adminUser.Branch ??= "SGN";
        adminUser.CompanyCode ??= "AMSS";

        db.UserList.Add(adminUser);

        var dept = adminUser.Department?.ToLower() ?? "admin";
        var templates = await db.PermissionTemplate
            .Where(x => x.Dept != null && (x.Dept.ToLower() == dept || x.Dept == "ALL"))
            .ToListAsync(cancellationToken);

        var permissions = new List<Permission_M>();
        foreach (var item in templates.Where(x => x.Dept?.Equals("ALL", StringComparison.OrdinalIgnoreCase) != true))
        {
            permissions.Add(new Permission_M
            {
                PermissionId = Guid.NewGuid(),
                MenuId = item.MenuId,
                MenuName = item.MenuName,
                Add = item.canAdd,
                See = item.canView,
                Edit = item.canEdit,
                Del = item.canDelete,
                Approve = item.canApprove,
                UserName = adminUser.Usr
            });
        }

        var perAll = templates.FirstOrDefault(x => x.Dept == "ALL");
        if (perAll != null)
        {
            var menus = await db.MenuNames.ToListAsync(cancellationToken);
            foreach (var menu in menus)
            {
                permissions.Add(new Permission_M
                {
                    PermissionId = Guid.NewGuid(),
                    MenuId = menu.MenuID,
                    MenuName = menu.MenuName,
                    Add = perAll.canAdd,
                    See = perAll.canView,
                    Edit = perAll.canEdit,
                    Del = perAll.canDelete,
                    Approve = perAll.canApprove,
                    UserName = adminUser.Usr
                });
            }
        }

        if (permissions.Count > 0)
            db.Permissions.AddRange(permissions);

        await db.SaveChangesAsync(cancellationToken);
    }
}
