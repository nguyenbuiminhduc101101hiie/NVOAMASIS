using Microsoft.Data.SqlClient;

namespace NVOAMASIS.Services.MultiTenant;

public static class TenantConnectionStringBuilder
{
    public static string Build(
        string server,
        string database,
        string userId,
        string password,
        bool trustServerCertificate = true)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            UserID = userId,
            Password = password,
            TrustServerCertificate = trustServerCertificate,
            MultipleActiveResultSets = true,
            PersistSecurityInfo = true
        };
        return builder.ConnectionString;
    }

    public static string BuildMaster(string server, string userId, string password) =>
        Build(server, "master", userId, password);
}
