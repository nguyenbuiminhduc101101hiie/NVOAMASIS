using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NVOAMASIS.Accounting.B09.Options;
using NVOAMASIS.Accounting.B09.Seed;
using NVOAMASIS.Accounting.B09.Services;

namespace NVOAMASIS.Accounting.B09.Extensions;

public static class B09ServiceCollectionExtensions
{
    public static IServiceCollection AddB09FinancialStatements(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<B09Options>(configuration.GetSection(B09Options.SectionName));
        services.AddScoped<B09TemplateSeeder>();
        services.AddScoped<IB09LedgerDataProvider, SqlB09LedgerDataProvider>();
        services.AddScoped<IB09FinancialStatementService, B09FinancialStatementService>();
        return services;
    }
}
