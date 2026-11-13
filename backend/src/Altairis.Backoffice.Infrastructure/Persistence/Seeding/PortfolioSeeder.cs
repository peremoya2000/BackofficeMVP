using System.Diagnostics;
using Altairis.Backoffice.Domain.Hotels;
using Microsoft.Extensions.Logging;

namespace Altairis.Backoffice.Infrastructure.Persistence.Seeding;

internal sealed class PortfolioSeeder(BackofficeDbContext dbContext, TimeProvider timeProvider, ILogger<PortfolioSeeder> logger)
{
    public async Task SeedAsync(IEnumerable<Hotel> hotels, CancellationToken cancellationToken)
    {
        long startedAt = Stopwatch.GetTimestamp();
        SamplePortfolio portfolio = SamplePortfolioGenerator.Generate(hotels, timeProvider.GetUtcNow().UtcDateTime);

        dbContext.InventoryDays.AddRange(portfolio.Inventory);
        dbContext.Bookings.AddRange(portfolio.Bookings);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded {InventoryDayCount} inventory days and {BookingCount} bookings in {ElapsedMs:N0} ms.",
            portfolio.Inventory.Count,
            portfolio.Bookings.Count,
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
    }
}
