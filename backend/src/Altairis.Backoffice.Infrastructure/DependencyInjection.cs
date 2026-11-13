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
        services.AddDbContext<BackofficeDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IBackofficeDbContext>(provider => provider.GetRequiredService<BackofficeDbContext>());

        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<HotelSeeder>();
        services.AddScoped<PortfolioSeeder>();

        return services;
    }
}
