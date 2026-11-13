using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altairis.Backoffice.Infrastructure.Persistence.Configurations;

internal sealed class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.Property(hotel => hotel.Id).ValueGeneratedNever();
        builder.Property(hotel => hotel.Name).HasMaxLength(Hotel.NAME_MAX_LENGTH);
        builder.Property(hotel => hotel.Address).HasMaxLength(Hotel.ADDRESS_MAX_LENGTH);
        builder.Property(hotel => hotel.City).HasMaxLength(Hotel.CITY_MAX_LENGTH);
        builder.Property(hotel => hotel.CountryCode).HasMaxLength(IsoCountryCodes.CODE_LENGTH);
        builder.Property(hotel => hotel.Currency).HasMaxLength(Money.CURRENCY_CODE_LENGTH);
        builder.Property(hotel => hotel.ContactEmail).HasMaxLength(Hotel.CONTACT_EMAIL_MAX_LENGTH);
        builder.Property(hotel => hotel.ContactPhone).HasMaxLength(Hotel.CONTACT_PHONE_MAX_LENGTH);

        builder.HasMany(hotel => hotel.RoomTypes)
            .WithOne()
            .HasForeignKey(roomType => roomType.HotelId);

        builder.HasIndex(hotel => hotel.Name).IsTrigramIndex();
        builder.HasIndex(hotel => hotel.City).IsTrigramIndex();
        builder.HasIndex(hotel => hotel.CountryCode);
    }
}
