namespace NVOAMASIS.Services.MultiTenant;

public interface ITenantContext
{
    Guid? TenantId { get; }
    string? DatabaseName { get; }
    string? ConnectionString { get; }
    void SetTenant(Guid tenantId, string databaseName, string connectionString);
    void Clear();
    void EnsureInitializedFromHttpContext();
}
