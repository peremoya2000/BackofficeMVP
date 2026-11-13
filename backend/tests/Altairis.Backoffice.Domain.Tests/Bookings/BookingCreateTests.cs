using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;
using Altairis.Backoffice.Domain.Inventory;
using Altairis.Backoffice.Domain.Tests.Inventory;

namespace Altairis.Backoffice.Domain.Tests.Bookings;

public class BookingCreateTests
{
    private readonly RoomType _roomType;
    private readonly List<InventoryDay> _inventory;

    public BookingCreateTests()
    {
        _roomType = BookingTestData.CreateRoomType(maxOccupancy: 2);
        _inventory = InventoryTestData.ForStay(_roomType.Id, BookingTestData.Stay, totalRooms: 5, price: 100m);
    }

    [Fact]
    public void Create_WithValidData_ReturnsConfirmedBooking()
    {
        Result<Booking> result = BookingTestData.Create(_roomType, _inventory, rooms: 1);

        Assert.True(result.IsSuccess);
        Booking booking = result.Value;
        Assert.Equal(7, booking.Id.Version);
        Assert.Matches("^ALT-[A-HJ-NP-Z2-9]{6}$", booking.Reference);
        Assert.Equal(Booking.REFERENCE_LENGTH, booking.Reference.Length);
        Assert.Equal(_roomType.HotelId, booking.HotelId);
        Assert.Equal(_roomType.Id, booking.RoomTypeId);
        Assert.Equal(BookingTestData.Stay, booking.Stay);
        Assert.Equal(new Money(300m, "EUR"), booking.TotalPrice);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Equal(BookingTestData.NowUtc, booking.CreatedAtUtc);
        Assert.Null(booking.CancelledAtUtc);
    }

    [Fact]
    public void Create_WithValidData_ReservesEveryNight()
    {
        BookingTestData.Create(_roomType, _inventory, rooms: 2);

        Assert.All(_inventory, day => Assert.Equal(2, day.BookedRooms));
    }

    [Fact]
    public void Create_WithPaddedText_TrimsAndTurnsBlankOptionalsIntoNull()
    {
        Result<Booking> result = BookingTestData.Create(
            _roomType,
            _inventory,
            leadGuestName: "  Ana García ",
            notes: "   ",
            clientName: " Viajes Sol y Mar  ",
            clientReference: " ");

        Booking booking = result.Value;
        Assert.Equal("Ana García", booking.LeadGuestName);
        Assert.Null(booking.Notes);
        Assert.Equal("Viajes Sol y Mar", booking.ClientName);
        Assert.Null(booking.ClientReference);
    }

    [Fact]
    public void Create_WhenQuoteFails_ReturnsQuoteErrorAndReservesNothing()
    {
        _inventory[2].Update(5, 100m, salesClosed: true);

        Result<Booking> result = BookingTestData.Create(_roomType, _inventory);

        Assert.Equal(InventoryDayErrors.SalesClosed(_inventory[2].Date), result.Error);
        Assert.All(_inventory, day => Assert.Equal(0, day.BookedRooms));
    }

    [Fact]
    public void Create_WithStaleQuotedTotal_ReturnsPriceChangedAndReservesNothing()
    {
        Result<Booking> result = BookingTestData.Create(_roomType, _inventory, quotedTotal: new Money(250m, "EUR"));

        Assert.Equal(BookingErrors.PriceChanged, result.Error);
        Assert.All(_inventory, day => Assert.Equal(0, day.BookedRooms));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankLeadGuestName_ReturnsLeadGuestNameRequired(string leadGuestName)
    {
        Result<Booking> result = BookingTestData.Create(_roomType, _inventory, leadGuestName: leadGuestName);

        Assert.Equal(BookingErrors.LeadGuestNameRequired, result.Error);
    }

    [Fact]
    public void Create_WithLeadGuestNameOverMaxLength_ReturnsLeadGuestNameTooLong()
    {
        Result<Booking> result = BookingTestData.Create(
            _roomType,
            _inventory,
            leadGuestName: new string('a', Booking.LEAD_GUEST_NAME_MAX_LENGTH + 1));

        Assert.Equal(BookingErrors.LeadGuestNameTooLong, result.Error);
    }

    [Fact]
    public void Create_WithNotesOverMaxLength_ReturnsNotesTooLong()
    {
        Result<Booking> result = BookingTestData.Create(_roomType, _inventory, notes: new string('a', Booking.NOTES_MAX_LENGTH + 1));

        Assert.Equal(BookingErrors.NotesTooLong, result.Error);
    }

    [Fact]
    public void Create_WithUndefinedChannel_ReturnsChannelInvalid()
    {
        Result<Booking> result = BookingTestData.Create(_roomType, _inventory, channel: (BookingChannel)99);

        Assert.Equal(BookingErrors.ChannelInvalid, result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankClientName_ReturnsClientNameRequired(string clientName)
    {
        Result<Booking> result = BookingTestData.Create(_roomType, _inventory, clientName: clientName);

        Assert.Equal(BookingErrors.ClientNameRequired, result.Error);
    }

    [Fact]
    public void Create_WithClientNameOverMaxLength_ReturnsClientNameTooLong()
    {
        Result<Booking> result = BookingTestData.Create(
            _roomType,
            _inventory,
            clientName: new string('a', Booking.CLIENT_NAME_MAX_LENGTH + 1));

        Assert.Equal(BookingErrors.ClientNameTooLong, result.Error);
    }

    [Fact]
    public void Create_WithClientReferenceOverMaxLength_ReturnsClientReferenceTooLong()
    {
        Result<Booking> result = BookingTestData.Create(
            _roomType,
            _inventory,
            clientReference: new string('a', Booking.CLIENT_REFERENCE_MAX_LENGTH + 1));

        Assert.Equal(BookingErrors.ClientReferenceTooLong, result.Error);
    }

    [Fact]
    public void Create_WithNegativeChildren_ReturnsChildrenNegative()
    {
        Result<Booking> result = BookingTestData.Create(_roomType, _inventory, children: -1);

        Assert.Equal(BookingErrors.ChildrenNegative, result.Error);
    }

    [Fact]
    public void Create_WithFewerAdultsThanRooms_ReturnsNotEnoughAdults()
    {
        Result<Booking> result = BookingTestData.Create(_roomType, _inventory, rooms: 2, adults: 1);

        Assert.Equal(BookingErrors.NotEnoughAdults, result.Error);
    }

    [Fact]
    public void Create_WithMoreGuestsThanRoomsFit_ReturnsTooManyGuests()
    {
        Result<Booking> result = BookingTestData.Create(_roomType, _inventory, rooms: 1, adults: 2, children: 1);

        Assert.Equal(BookingErrors.TooManyGuests(1, 2), result.Error);
    }

    [Fact]
    public void Create_WithRoomsFilledToCapacity_Succeeds()
    {
        Result<Booking> result = BookingTestData.Create(_roomType, _inventory, rooms: 2, adults: 2, children: 2);

        Assert.True(result.IsSuccess);
    }
}
