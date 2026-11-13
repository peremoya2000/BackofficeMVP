using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;
using Altairis.Backoffice.Domain.Inventory;
using Altairis.Backoffice.Domain.Tests.Hotels;

namespace Altairis.Backoffice.Domain.Tests.Bookings;

internal static class BookingTestData
{
    public static readonly DateOnly Today = new(2026, 11, 20);
    public static readonly DateTime NowUtc = new(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc);
    public static readonly StayPeriod Stay = StayPeriod.Create(new DateOnly(2026, 11, 22), new DateOnly(2026, 11, 25)).Value;

    public static RoomType CreateRoomType(int maxOccupancy = 2)
    {
        return HotelTestData.CreateValid().AddRoomType("Doble Superior", RoomCategory.Double, maxOccupancy).Value;
    }

    public static Result<Money> Quote(
        RoomType roomType,
        IReadOnlyCollection<InventoryDay> inventory,
        StayPeriod? stay = null,
        int rooms = 1,
        DateOnly? today = null)
    {
        return Booking.Quote(roomType.Id, stay ?? Stay, rooms, inventory, Hotel.MVP_CURRENCY, today ?? Today);
    }

    public static Result<Booking> Create(
        RoomType roomType,
        IReadOnlyCollection<InventoryDay> inventory,
        int rooms = 1,
        int adults = 2,
        int children = 0,
        string leadGuestName = "Ana García",
        string? notes = null,
        BookingChannel channel = BookingChannel.Agency,
        string clientName = "Viajes Sol y Mar",
        string? clientReference = null,
        Money? quotedTotal = null)
    {
        Result<Money> quote = Quote(roomType, inventory, rooms: rooms);
        Money total = quotedTotal ?? (quote.IsSuccess ? quote.Value : Money.Zero(Hotel.MVP_CURRENCY));

        return Booking.Create(
            roomType,
            Stay,
            rooms,
            adults,
            children,
            leadGuestName,
            notes,
            channel,
            clientName,
            clientReference,
            inventory,
            Hotel.MVP_CURRENCY,
            total,
            Today,
            NowUtc);
    }
}
