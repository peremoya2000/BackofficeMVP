using Altairis.Backoffice.Application.Common;
using Altairis.Backoffice.Infrastructure.Persistence;
using Altairis.Backoffice.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Altairis.Backoffice.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Our nested collections (the inventory grid) are small, so one query beats several; EF warns until this is explicit.
        services.AddDbContext<BackofficeDbContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery))
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IBackofficeDbContext>(provider => provider.GetRequiredService<BackofficeDbContext>());

        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<HotelSeeder>();
        services.AddScoped<PortfolioSeeder>();

        return services;
    }
}
