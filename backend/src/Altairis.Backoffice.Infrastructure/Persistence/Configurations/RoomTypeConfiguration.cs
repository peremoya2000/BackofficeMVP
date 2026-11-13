using Altairis.Backoffice.Domain.Hotels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altairis.Backoffice.Infrastructure.Persistence.Configurations;

internal sealed class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        // The domain assigns Ids; if EF thought it generated them, a room type added to a loaded hotel would be saved as an UPDATE.
        builder.Property(roomType => roomType.Id).ValueGeneratedNever();
        builder.Property(roomType => roomType.Name).HasMaxLength(RoomType.NAME_MAX_LENGTH);
    }
}
