using Altairis.Backoffice.Domain.Hotels;
using Altairis.Backoffice.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Altairis.Backoffice.Infrastructure.Persistence;

internal sealed class DatabaseInitializer(BackofficeDbContext dbContext, HotelSeeder hotelSeeder, PortfolioSeeder portfolioSeeder)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (await dbContext.Hotels.AnyAsync(cancellationToken)) return;

        // One transaction, so a start that fails halfway leaves an empty database for the next start to fill.
        await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        List<Hotel> hotels = await hotelSeeder.SeedAsync(cancellationToken);
        await portfolioSeeder.SeedAsync(hotels.Take(SampleHotelGenerator.PortfolioHotelCount), cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // Autovacuum refreshes planner statistics only later, and searches right after a bulk load need them.
        await dbContext.Database.ExecuteSqlRawAsync("ANALYZE;", cancellationToken);
    }
}
