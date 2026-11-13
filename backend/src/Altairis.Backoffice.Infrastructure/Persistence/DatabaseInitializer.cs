using Altairis.Backoffice.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Backoffice.Infrastructure.Persistence;

internal sealed class DatabaseInitializer(BackofficeDbContext dbContext, HotelSeeder hotelSeeder)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (await dbContext.Hotels.AnyAsync(cancellationToken)) return;

        await hotelSeeder.SeedAsync(cancellationToken);
    }
}
