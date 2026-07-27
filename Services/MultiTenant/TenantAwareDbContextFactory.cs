using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;

namespace NVOAMASIS.Services.MultiTenant;

/// <summary>
/// Uses the current tenant connection from claims/registry, or falls back to DefaultConnection.
/// </summary>
public class TenantAwareDbContextFactory(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory) : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext()
    {
        var connectionString = ResolveConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new AppDbContext(options);
    }

    private string ResolveConnectionString()
    {
        if (!configuration.GetValue("MultiTenant:Enabled", true))
        {
            return configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            using var scope = scopeFactory.CreateScope();
            var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
            tenantContext.EnsureInitializedFromHttpContext();
            if (!string.IsNullOrEmpty(tenantContext.ConnectionString))
                return tenantContext.ConnectionString;
        }

        return configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
    }
}
