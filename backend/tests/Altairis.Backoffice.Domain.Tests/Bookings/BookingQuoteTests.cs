using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;
using Altairis.Backoffice.Domain.Inventory;
using Altairis.Backoffice.Domain.Tests.Inventory;

namespace Altairis.Backoffice.Domain.Tests.Bookings;

public class BookingQuoteTests
{
    private readonly RoomType _roomType;
    private readonly List<InventoryDay> _inventory;

    public BookingQuoteTests()
    {
        _roomType = BookingTestData.CreateRoomType();
        _inventory = InventoryTestData.ForStay(_roomType.Id, BookingTestData.Stay);
    }

    [Fact]
    public void Quote_WithAvailableNights_ReturnsSumOfNightlyPricesTimesRooms()
    {
        _inventory[1].Update(5, 120m, salesClosed: false);

        Result<Money> result = BookingTestData.Quote(_roomType, _inventory, rooms: 2);

        Assert.Equal(new Money(640m, "EUR"), result.Value);
    }

    [Fact]
    public void Quote_DoesNotReserveInventory()
    {
        BookingTestData.Quote(_roomType, _inventory, rooms: 2);

        Assert.All(_inventory, day => Assert.Equal(0, day.BookedRooms));
    }

    [Fact]
    public void Quote_WithZeroRooms_ReturnsRoomsInvalid()
    {
        Result<Money> result = BookingTestData.Quote(_roomType, _inventory, rooms: 0);

        Assert.Equal(BookingErrors.RoomsInvalid, result.Error);
    }

    [Fact]
    public void Quote_OverMaxNights_ReturnsTooManyNights()
    {
        StayPeriod longStay = StayPeriod.Create(BookingTestData.Today, BookingTestData.Today.AddDays(Booking.MAX_NIGHTS + 1)).Value;
        List<InventoryDay> inventory = InventoryTestData.ForStay(_roomType.Id, longStay);

        Result<Money> result = BookingTestData.Quote(_roomType, inventory, stay: longStay);

        Assert.Equal(BookingErrors.TooManyNights, result.Error);
    }

    [Fact]
    public void Quote_WithExactlyMaxNights_Succeeds()
    {
        StayPeriod longStay = StayPeriod.Create(BookingTestData.Today, BookingTestData.Today.AddDays(Booking.MAX_NIGHTS)).Value;
        List<InventoryDay> inventory = InventoryTestData.ForStay(_roomType.Id, longStay);

        Result<Money> result = BookingTestData.Quote(_roomType, inventory, stay: longStay);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Quote_WithCheckInBeforeToday_ReturnsCheckInInPast()
    {
        Result<Money> result = BookingTestData.Quote(_roomType, _inventory, today: BookingTestData.Stay.CheckIn.AddDays(1));

        Assert.Equal(BookingErrors.CheckInInPast, result.Error);
    }

    [Fact]
    public void Quote_WithCheckInToday_Succeeds()
    {
        Result<Money> result = BookingTestData.Quote(_roomType, _inventory, today: BookingTestData.Stay.CheckIn);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Quote_WithMissingNight_ReturnsNoInventoryForThatNight()
    {
        _inventory.RemoveAt(1);

        Result<Money> result = BookingTestData.Quote(_roomType, _inventory);

        Assert.Equal(BookingErrors.NoInventory(new DateOnly(2026, 11, 23)), result.Error);
    }

    [Fact]
    public void Quote_WithSalesClosedNight_ReturnsSalesClosedForThatNight()
    {
        _inventory[2].Update(5, 100m, salesClosed: true);

        Result<Money> result = BookingTestData.Quote(_roomType, _inventory);

        Assert.Equal(InventoryDayErrors.SalesClosed(new DateOnly(2026, 11, 24)), result.Error);
    }

    [Fact]
    public void Quote_WithNotEnoughRooms_ReturnsNotEnoughRoomsForFirstFailingNight()
    {
        List<InventoryDay> inventory = InventoryTestData.ForStay(_roomType.Id, BookingTestData.Stay, totalRooms: 1);

        Result<Money> result = BookingTestData.Quote(_roomType, inventory, rooms: 2);

        Assert.Equal(InventoryDayErrors.NotEnoughRooms(new DateOnly(2026, 11, 22), 1), result.Error);
    }
}
