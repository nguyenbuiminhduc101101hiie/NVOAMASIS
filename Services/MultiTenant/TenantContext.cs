using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;

namespace NVOAMASIS.Services.MultiTenant;

public class TenantContext(
    IHttpContextAccessor httpContextAccessor,
    IServiceScopeFactory scopeFactory) : ITenantContext
{
    private Guid? _tenantId;
    private string? _databaseName;
    private string? _connectionString;
    private bool _initialized;

    public Guid? TenantId => _tenantId;
    public string? DatabaseName => _databaseName;
    public string? ConnectionString => _connectionString;

    public void SetTenant(Guid tenantId, string databaseName, string connectionString)
    {
        _tenantId = tenantId;
        _databaseName = databaseName;
        _connectionString = connectionString;
        _initialized = true;
    }

    public void Clear()
    {
        _tenantId = null;
        _databaseName = null;
        _connectionString = null;
        _initialized = false;
    }

    public void EnsureInitializedFromHttpContext()
    {
        if (_initialized) return;

        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true) return;

        var tenantIdClaim = user.FindFirst(TenantClaimTypes.TenantId)?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId)) return;

        using var scope = scopeFactory.CreateScope();
        var registry = scope.ServiceProvider.GetRequiredService<RegistryDbContext>();
        var row = registry.TenantDatabases.AsNoTracking()
            .FirstOrDefault(x => x.TenantId == tenantId && x.IsActive);
        if (row == null) return;

        _tenantId = row.TenantId;
        _databaseName = row.DatabaseName;
        _connectionString = TenantConnectionStringBuilder.Build(
            row.ServerName, row.DatabaseName, row.SqlUserId, row.SqlPassword);
        _initialized = true;
    }
}
