using Altairis.Backoffice.Application.Common;
using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Hotels;
using Altairis.Backoffice.Domain.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Backoffice.Infrastructure.Persistence;

public sealed class BackofficeDbContext(DbContextOptions<BackofficeDbContext> options) : DbContext(options), IBackofficeDbContext
{
    public DbSet<Hotel> Hotels => Set<Hotel>();

    public DbSet<RoomType> RoomTypes => Set<RoomType>();

    public DbSet<InventoryDay> InventoryDays => Set<InventoryDay>();

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasDbFunction(() => SearchText.Unaccent(default!)).HasName("immutable_unaccent");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BackofficeDbContext).Assembly);
    }
}
