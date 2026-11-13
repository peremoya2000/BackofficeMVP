using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altairis.Backoffice.Infrastructure.Persistence.Configurations;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.Property(booking => booking.Id).ValueGeneratedNever();
        builder.Property(booking => booking.Reference).HasMaxLength(Booking.REFERENCE_LENGTH);
        builder.Property(booking => booking.LeadGuestName).HasMaxLength(Booking.LEAD_GUEST_NAME_MAX_LENGTH);
        builder.Property(booking => booking.Notes).HasMaxLength(Booking.NOTES_MAX_LENGTH);
        builder.Property(booking => booking.ClientName).HasMaxLength(Booking.CLIENT_NAME_MAX_LENGTH);
        builder.Property(booking => booking.ClientReference).HasMaxLength(Booking.CLIENT_REFERENCE_MAX_LENGTH);

        builder.ComplexProperty(booking => booking.Stay, stay =>
        {
            // Get-only properties aren't mapped by convention; naming them lets EF bind StayPeriod's private constructor.
            stay.Property(period => period.CheckIn);
            stay.Property(period => period.CheckOut);
        });
        builder.ComplexProperty(booking => booking.TotalPrice, totalPrice =>
        {
            totalPrice.Property(money => money.Currency).HasMaxLength(Money.CURRENCY_CODE_LENGTH);
        });

        builder.HasOne<Hotel>()
            .WithMany()
            .HasForeignKey(booking => booking.HotelId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<RoomType>()
            .WithMany()
            .HasForeignKey(booking => booking.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(booking => booking.Reference).IsUnique();
        builder.HasIndex(booking => booking.CreatedAtUtc);
    }
}
