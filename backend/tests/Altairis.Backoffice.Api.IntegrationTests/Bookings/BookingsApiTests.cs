using System.Data;
using System.Net;
using System.Net.Http.Json;
using Altairis.Backoffice.Application.Common;
using Altairis.Backoffice.Contracts.Bookings;
using Altairis.Backoffice.Contracts.Common;
using Altairis.Backoffice.Contracts.Inventory;
using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Inventory;
using Altairis.Backoffice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Altairis.Backoffice.Api.IntegrationTests.Bookings;

public sealed class BookingsApiTests(BackofficeApiFactory factory)
{
    private const string BOOKINGS_URL = "/api/bookings";

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private static DateOnly Today => OperationsCalendar.Today(TimeProvider.System);

    [Fact]
    public async Task Quote_AddsThePriceOfEveryNightForEveryRoom()
    {
        (_, Guid roomTypeId) = await factory.CreateHotelWithRoomTypeAsync();
        DateOnly checkIn = Today.AddDays(10);
        await LoadInventoryAsync(roomTypeId, checkIn, checkIn.AddDays(1), price: 100m);
        await LoadInventoryAsync(roomTypeId, checkIn.AddDays(2), checkIn.AddDays(2), price: 150m);

        QuoteResponse? quote = await _client.GetFromJsonAsync<QuoteResponse>(
            QuoteUrl(roomTypeId, checkIn, checkIn.AddDays(3), rooms: 2),
            Cancellation);

        Assert.Equal(new QuoteResponse((100m + 100m + 150m) * 2, "EUR"), quote);
    }

    [Fact]
    public async Task Quote_WhenANightIsClosedForSale_Returns409WithTheReason()
    {
        (_, Guid roomTypeId) = await factory.CreateHotelWithRoomTypeAsync();
        DateOnly checkIn = Today.AddDays(10);
        await LoadInventoryAsync(roomTypeId, checkIn, checkIn.AddDays(2));
        await PutInventoryAsync(roomTypeId, new UpdateInventoryRangeRequest(checkIn.AddDays(1), checkIn.AddDays(1), SalesClosed: true));

        HttpResponseMessage response = await _client.GetAsync(QuoteUrl(roomTypeId, checkIn, checkIn.AddDays(3), rooms: 1), Cancellation);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("inventory_day.sales_closed", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task CreateThenCancel_ReservesTheStayNightsThenReleasesThem()
    {
        (Guid hotelId, Guid roomTypeId) = await factory.CreateHotelWithRoomTypeAsync();
        DateOnly checkIn = Today.AddDays(10);
        DateOnly checkOut = checkIn.AddDays(2);
        await LoadInventoryAsync(roomTypeId, checkIn, checkOut, price: 100m);

        HttpResponseMessage created = await _client.PostAsJsonAsync(
            BOOKINGS_URL,
            NewBookingRequest(roomTypeId, checkIn, checkOut, quotedTotal: 400m, rooms: 2),
            Cancellation);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        BookingResponse? booking = await created.Content.ReadFromJsonAsync<BookingResponse>(Cancellation);
        Assert.NotNull(booking);
        Assert.Matches("^ALT-[A-HJ-NP-Z2-9]{6}$", booking.Reference);
        Assert.Equal("Confirmed", booking.Status);
        Assert.Equal("Doble estándar", booking.RoomTypeName);
        Assert.Equal(400m, booking.TotalPrice);
        int[] bookedRooms = await BookedRoomsAsync(hotelId, checkIn, checkOut);
        // The check-out night is not booked.
        Assert.Equal([2, 2, 0], bookedRooms);

        HttpResponseMessage cancelled = await _client.PostAsync($"{BOOKINGS_URL}/{booking.Id}/cancel", null, Cancellation);

        Assert.Equal(HttpStatusCode.NoContent, cancelled.StatusCode);
        bookedRooms = await BookedRoomsAsync(hotelId, checkIn, checkOut);
        Assert.Equal([0, 0, 0], bookedRooms);
        BookingResponse? fetched = await _client.GetFromJsonAsync<BookingResponse>(created.Headers.Location, Cancellation);
        Assert.Equal("Cancelled", fetched?.Status);
    }

    [Fact]
    public async Task Create_WhenThePriceChangedSinceTheQuote_Returns409AndReservesNothing()
    {
        (Guid hotelId, Guid roomTypeId) = await factory.CreateHotelWithRoomTypeAsync();
        DateOnly checkIn = Today.AddDays(10);
        await LoadInventoryAsync(roomTypeId, checkIn, checkIn.AddDays(1), price: 120m);

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            BOOKINGS_URL,
            NewBookingRequest(roomTypeId, checkIn, checkIn.AddDays(2), quotedTotal: 200m),
            Cancellation);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("booking.price_changed", await response.ReadErrorCodeAsync());
        int[] bookedRooms = await BookedRoomsAsync(hotelId, checkIn, checkIn.AddDays(1));
        Assert.Equal([0, 0], bookedRooms);
    }

    [Fact]
    public async Task Cancel_WhenAlreadyCancelled_Returns409()
    {
        (_, Guid roomTypeId) = await factory.CreateHotelWithRoomTypeAsync();
        DateOnly checkIn = Today.AddDays(10);
        await LoadInventoryAsync(roomTypeId, checkIn, checkIn, price: 100m);
        BookingResponse booking = await CreateBookingAsync(NewBookingRequest(roomTypeId, checkIn, checkIn.AddDays(1), quotedTotal: 100m));
        await _client.PostAsync($"{BOOKINGS_URL}/{booking.Id}/cancel", null, Cancellation);

        HttpResponseMessage response = await _client.PostAsync($"{BOOKINGS_URL}/{booking.Id}/cancel", null, Cancellation);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("booking.already_cancelled", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task BookedRooms_AlwaysEqualsTheRoomsOfConfirmedBookings()
    {
        (_, Guid roomTypeId) = await factory.CreateHotelWithRoomTypeAsync();
        DateOnly checkIn = Today.AddDays(10);
        await LoadInventoryAsync(roomTypeId, checkIn, checkIn.AddDays(3), price: 100m);
        await CreateBookingAsync(NewBookingRequest(roomTypeId, checkIn, checkIn.AddDays(3), quotedTotal: 300m));
        BookingResponse cancelled = await CreateBookingAsync(
            NewBookingRequest(roomTypeId, checkIn.AddDays(1), checkIn.AddDays(4), quotedTotal: 600m, rooms: 2));
        await _client.PostAsync($"{BOOKINGS_URL}/{cancelled.Id}/cancel", null, Cancellation);

        using IServiceScope scope = factory.Services.CreateScope();
        BackofficeDbContext dbContext = scope.ServiceProvider.GetRequiredService<BackofficeDbContext>();
        // Both reads see one snapshot, even if another test books in between.
        await using IDbContextTransaction snapshot = await dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, Cancellation);
        List<InventoryDay> nights = await dbContext.InventoryDays.AsNoTracking().ToListAsync(Cancellation);
        List<Booking> confirmedBookings = await dbContext.Bookings
            .AsNoTracking()
            .Where(booking => booking.Status == BookingStatus.Confirmed)
            .ToListAsync(Cancellation);

        Dictionary<(Guid RoomTypeId, DateOnly Date), int> bookedRoomsByNight = confirmedBookings
            .SelectMany(booking => booking.Stay.EachNight().Select(night => (Key: (booking.RoomTypeId, night), booking.Rooms)))
            .GroupBy(entry => entry.Key, entry => entry.Rooms)
            .ToDictionary(group => group.Key, group => group.Sum());

        Assert.Contains(nights, night => night.BookedRooms > 0);
        Assert.All(nights, night => Assert.Equal(bookedRoomsByNight.GetValueOrDefault((night.RoomTypeId, night.Date)), night.BookedRooms));
    }

    [Fact]
    public async Task Search_ArrivalsOfADay_SkipsOtherDaysAndCancelledBookings()
    {
        (_, Guid roomTypeId) = await factory.CreateHotelWithRoomTypeAsync();
        DateOnly arrivalDay = Today.AddDays(10);
        await LoadInventoryAsync(roomTypeId, arrivalDay, arrivalDay.AddDays(1), price: 100m);

        string guestName = $"Ana {NewToken()}";
        BookingResponse arriving = await CreateBookingAsync(
            NewBookingRequest(roomTypeId, arrivalDay, arrivalDay.AddDays(1), quotedTotal: 100m, leadGuestName: guestName));
        await CreateBookingAsync(
            NewBookingRequest(roomTypeId, arrivalDay.AddDays(1), arrivalDay.AddDays(2), quotedTotal: 100m, leadGuestName: guestName));

        BookingResponse cancelled = await CreateBookingAsync(
            NewBookingRequest(roomTypeId, arrivalDay, arrivalDay.AddDays(1), quotedTotal: 100m, leadGuestName: guestName));
        await _client.PostAsync($"{BOOKINGS_URL}/{cancelled.Id}/cancel", null, Cancellation);

        PagedResponse<BookingListItemResponse> page = await SearchAsync(
            $"search={guestName}&checkIn={arrivalDay:yyyy-MM-dd}&status=Confirmed");

        BookingListItemResponse match = Assert.Single(page.Items);
        Assert.Equal(arriving.Id, match.Id);
    }

    [Fact]
    public async Task Search_ByReference_FindsThatBooking()
    {
        (_, Guid roomTypeId) = await factory.CreateHotelWithRoomTypeAsync();
        DateOnly checkIn = Today.AddDays(10);
        await LoadInventoryAsync(roomTypeId, checkIn, checkIn.AddDays(1), price: 100m);

        BookingResponse booking = await CreateBookingAsync(
            NewBookingRequest(roomTypeId, checkIn, checkIn.AddDays(1), quotedTotal: 100m));

        PagedResponse<BookingListItemResponse> page = await SearchAsync($"search={booking.Reference.ToLowerInvariant()}");

        BookingListItemResponse match = Assert.Single(page.Items);
        Assert.Equal(booking.Id, match.Id);
        Assert.Equal(booking.HotelName, match.HotelName);
        Assert.Equal("Confirmed", match.Status);
    }

    [Fact]
    public async Task Search_SortsByCheckInDescending()
    {
        (_, Guid roomTypeId) = await factory.CreateHotelWithRoomTypeAsync();
        DateOnly checkIn = Today.AddDays(10);
        await LoadInventoryAsync(roomTypeId, checkIn, checkIn.AddDays(1), price: 100m);
        string guestName = $"Ana {NewToken()}";
        BookingResponse earlier = await CreateBookingAsync(
            NewBookingRequest(roomTypeId, checkIn, checkIn.AddDays(1), quotedTotal: 100m, leadGuestName: guestName));
        BookingResponse later = await CreateBookingAsync(
            NewBookingRequest(roomTypeId, checkIn.AddDays(1), checkIn.AddDays(2), quotedTotal: 100m, leadGuestName: guestName));

        PagedResponse<BookingListItemResponse> page = await SearchAsync($"search={guestName}&sort=-checkIn");

        Assert.Equal([later.Id, earlier.Id], page.Items.Select(booking => booking.Id));
    }

    [Theory]
    [InlineData("status=Pending")]
    [InlineData("sort=leadGuestName")]
    [InlineData("checkIn=not-a-date")]
    public async Task Search_WithInvalidQuery_Returns400(string query)
    {
        HttpResponseMessage response = await _client.GetAsync($"{BOOKINGS_URL}?{query}", Cancellation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static CreateBookingRequest NewBookingRequest(
        Guid roomTypeId,
        DateOnly checkIn,
        DateOnly checkOut,
        decimal quotedTotal,
        int rooms = 1,
        string leadGuestName = "Ana García") =>
        new(
            roomTypeId,
            checkIn,
            checkOut,
            rooms,
            Adults: rooms,
            Children: 0,
            LeadGuestName: leadGuestName,
            Channel: "Agency",
            ClientName: "Viajes Sol y Mar",
            QuotedTotal: quotedTotal);

    private static string QuoteUrl(Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, int rooms) =>
        $"/api/room-types/{roomTypeId}/quote?checkIn={checkIn:yyyy-MM-dd}&checkOut={checkOut:yyyy-MM-dd}&rooms={rooms}";

    private Task LoadInventoryAsync(Guid roomTypeId, DateOnly from, DateOnly to, decimal price = 100m) =>
        PutInventoryAsync(roomTypeId, new UpdateInventoryRangeRequest(from, to, TotalRooms: 5, Price: price));

    private async Task PutInventoryAsync(Guid roomTypeId, UpdateInventoryRangeRequest request)
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/room-types/{roomTypeId}/inventory", request, Cancellation);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(BOOKINGS_URL, request, Cancellation);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        BookingResponse? booking = await response.Content.ReadFromJsonAsync<BookingResponse>(Cancellation);
        Assert.NotNull(booking);
        return booking;
    }

    private async Task<int[]> BookedRoomsAsync(Guid hotelId, DateOnly from, DateOnly to)
    {
        InventoryCalendarResponse? calendar = await _client.GetFromJsonAsync<InventoryCalendarResponse>(
            $"/api/hotels/{hotelId}/inventory?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}",
            Cancellation);
        Assert.NotNull(calendar);
        return Assert.Single(calendar.RoomTypes).Days.Select(day => day.BookedRooms).ToArray();
    }

    private static string NewToken() => Guid.NewGuid().ToString("N")[..12];

    private async Task<PagedResponse<BookingListItemResponse>> SearchAsync(string query)
    {
        PagedResponse<BookingListItemResponse>? page = await _client.GetFromJsonAsync<PagedResponse<BookingListItemResponse>>(
            $"{BOOKINGS_URL}?{query}",
            Cancellation);
        Assert.NotNull(page);
        return page;
    }
}
