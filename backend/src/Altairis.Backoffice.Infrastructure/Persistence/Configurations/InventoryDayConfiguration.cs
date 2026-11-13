using Altairis.Backoffice.Domain.Hotels;
using Altairis.Backoffice.Domain.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altairis.Backoffice.Infrastructure.Persistence.Configurations;

internal sealed class InventoryDayConfiguration : IEntityTypeConfiguration<InventoryDay>
{
    public void Configure(EntityTypeBuilder<InventoryDay> builder)
    {
        builder.HasKey(day => new { day.RoomTypeId, day.Date });

        builder.HasOne<RoomType>()
            .WithMany()
            .HasForeignKey(day => day.RoomTypeId);

        // Npgsql maps a uint row version to Postgres's xmin system column, so no real column is added.
        builder.Property<uint>("Version").IsRowVersion();
    }
}
