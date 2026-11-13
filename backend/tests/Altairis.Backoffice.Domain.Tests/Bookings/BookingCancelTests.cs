using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;
using Altairis.Backoffice.Domain.Inventory;
using Altairis.Backoffice.Domain.Tests.Inventory;

namespace Altairis.Backoffice.Domain.Tests.Bookings;

public class BookingCancelTests
{
    private static readonly DateTime CancelledAtUtc = new(2026, 11, 21, 10, 0, 0, DateTimeKind.Utc);

    private readonly List<InventoryDay> _inventory;
    private readonly Booking _booking;

    public BookingCancelTests()
    {
        RoomType roomType = BookingTestData.CreateRoomType();
        _inventory = InventoryTestData.ForStay(roomType.Id, BookingTestData.Stay);
        _booking = BookingTestData.Create(roomType, _inventory, rooms: 2).Value;
    }

    [Fact]
    public void Cancel_BeforeCheckIn_ReleasesEveryNightAndMarksCancelled()
    {
        Result result = _booking.Cancel(_inventory, BookingTestData.Today, CancelledAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(BookingStatus.Cancelled, _booking.Status);
        Assert.Equal(CancelledAtUtc, _booking.CancelledAtUtc);
        Assert.All(_inventory, day => Assert.Equal(0, day.BookedRooms));
    }

    [Fact]
    public void Cancel_OnCheckInDay_Succeeds()
    {
        Result result = _booking.Cancel(_inventory, BookingTestData.Stay.CheckIn, CancelledAtUtc);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Cancel_AfterCheckIn_ReturnsCheckInPassedAndKeepsInventory()
    {
        Result result = _booking.Cancel(_inventory, BookingTestData.Stay.CheckIn.AddDays(1), CancelledAtUtc);

        Assert.Equal(BookingErrors.CheckInPassed, result.Error);
        Assert.Equal(BookingStatus.Confirmed, _booking.Status);
        Assert.All(_inventory, day => Assert.Equal(2, day.BookedRooms));
    }

    [Fact]
    public void Cancel_Twice_ReturnsAlreadyCancelled()
    {
        _booking.Cancel(_inventory, BookingTestData.Today, CancelledAtUtc);

        Result result = _booking.Cancel(_inventory, BookingTestData.Today, CancelledAtUtc);

        Assert.Equal(BookingErrors.AlreadyCancelled, result.Error);
    }
}
