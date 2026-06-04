using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Models;

namespace NVOAMASIS.Data;

public class RegistryDbContext(DbContextOptions<RegistryDbContext> options) : DbContext(options)
{
    public DbSet<TenantDatabaseRegistry> TenantDatabases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantDatabaseRegistry>(e =>
        {
            e.ToTable("TenantDatabaseRegistry");
            e.HasIndex(x => x.DatabaseName).IsUnique();
        });
    }
}
