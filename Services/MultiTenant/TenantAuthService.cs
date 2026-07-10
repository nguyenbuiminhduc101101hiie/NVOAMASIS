using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;

namespace NVOAMASIS.Services.MultiTenant;

public class TenantAuthService(
    TenantDatabaseProvisioningService provisioning,
    ITenantContext tenantContext)
{
    public async Task<LoginResponse> LoginAsync(
        string databaseName,
        string sqlUserId,
        string sqlPassword,
        string appUserName,
        string appPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
            return new LoginResponse(false, "Nhập tên database.");
        if (string.IsNullOrWhiteSpace(sqlUserId) || string.IsNullOrWhiteSpace(sqlPassword))
            return new LoginResponse(false, "Nhập user và password SQL Server.");
        if (string.IsNullOrWhiteSpace(appUserName) || string.IsNullOrWhiteSpace(appPassword))
            return new LoginResponse(false, "Nhập user và password ứng dụng.");

        var tenant = await provisioning.ResolveOrRegisterLegacyTenantAsync(
            databaseName, sqlUserId, sqlPassword, cancellationToken);
        if (tenant == null)
            return new LoginResponse(false,
                "Không đăng nhập được: kiểm tra tên database (vd. nvoamasis), SQL user/password và user ứng dụng.");

        var canConnect = await provisioning.TestSqlConnectionAsync(
            tenant.ServerName, tenant.DatabaseName, tenant.SqlUserId, tenant.SqlPassword, cancellationToken);
        if (!canConnect)
            return new LoginResponse(false, "Không kết nối được tới database tenant.");

        var tenantConn = TenantConnectionStringBuilder.Build(
            tenant.ServerName, tenant.DatabaseName, tenant.SqlUserId, tenant.SqlPassword);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(tenantConn)
            .Options;

        await using var db = new AppDbContext(options);
        var appUser = await db.UserList.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Usr != null && x.Usr.ToLower() == appUserName.Trim().ToLower(), cancellationToken);

        if (appUser == null)
            return new LoginResponse(false, "User ứng dụng không tồn tại trong database này.");
        if (appPassword != appUser.Pass_viettel)
            return new LoginResponse(false, "User/Password ứng dụng không đúng.");

        tenantContext.SetTenant(tenant.TenantId, tenant.DatabaseName, tenantConn);

        var authUser = new AuthUser
        {
            Usr = appUser.Usr,
            Name = appUser.Name,
            UsrId = appUser.UsrId,
            Email = appUser.Email,
            Manager2 = appUser.Manager2,
            Department = appUser.Department,
            Send_OTP_login = appUser.Send_OTP_login
        };

        return new LoginResponse(true, "Đăng nhập thành công.", authUser, tenant.TenantId, tenant.DatabaseName);
    }

    public Task<BoolandMessReponse> RegisterTenantAsync(
        string databaseName,
        string sqlUserId,
        string sqlPassword,
        AuthUser adminUser,
        CancellationToken cancellationToken = default) =>
        provisioning.ProvisionNewTenantAsync(databaseName, sqlUserId, sqlPassword, adminUser, cancellationToken);
}
