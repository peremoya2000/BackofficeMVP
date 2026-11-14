using System.Net;
using System.Net.Http.Json;
using Altairis.Backoffice.Application.Common;
using Altairis.Backoffice.Contracts.Bookings;
using Altairis.Backoffice.Contracts.Dashboard;
using Altairis.Backoffice.Contracts.Inventory;

namespace Altairis.Backoffice.Api.IntegrationTests.Dashboard;

public sealed class DashboardApiTests(BackofficeApiFactory factory)
{
    private const decimal NIGHT_PRICE = 100m;
    private const int ALLOTMENT = 5;

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private static DateOnly Today => OperationsCalendar.Today(TimeProvider.System);

    [Fact]
    public async Task Summary_CountsTheConfirmedArrivalsDeparturesAndStaysOfTheDay()
    {
        // Past the sample inventory and the other tests' dates, so only this test's bookings touch it.
        DateOnly day = Today.AddDays(200);
        (_, Guid roomTypeId) = await factory.CreateHotelWithRoomTypeAsync();
        await LoadInventoryAsync(roomTypeId, day.AddDays(-1), day);
        await CreateBookingAsync(roomTypeId, day.AddDays(-1), day);
        await CreateBookingAsync(roomTypeId, day.AddDays(-1), day.AddDays(1), rooms: 2);
        await CreateBookingAsync(roomTypeId, day, day.AddDays(1));
        BookingResponse cancelled = await CreateBookingAsync(roomTypeId, day, day.AddDays(1));
        await CancelAsync(cancelled.Id);

        DashboardSummaryResponse summary = await GetSummaryAsync(day);

        Assert.Equal(
            new DashboardSummaryResponse(day, Arrivals: 1, Departures: 1, StaysInProgress: 2, NewBookings: 0, Cancellations: 0, BookedRooms: 3, TotalRooms: ALLOTMENT),
            summary);
    }

    [Fact]
    public async Task Summary_OfToday_CountsTheBookingsMadeAndCancelledToday()
    {
        DateOnly checkIn = Today.AddDays(205);
        (_, Guid roomTypeId) = await factory.CreateHotelWithRoomTypeAsync();
        await LoadInventoryAsync(roomTypeId, checkIn, checkIn);
        DashboardSummaryResponse before = await GetSummaryAsync(Today);

        await CreateBookingAsync(roomTypeId, checkIn, checkIn.AddDays(1));
        BookingResponse cancelled = await CreateBookingAsync(roomTypeId, checkIn, checkIn.AddDays(1));
        await CancelAsync(cancelled.Id);
        DashboardSummaryResponse after = await GetSummaryAsync(Today);

        // Other test classes book in parallel, so the counts can only be checked from below.
        Assert.True(after.NewBookings >= before.NewBookings + 2);
        Assert.True(after.Cancellations >= before.Cancellations + 1);
    }

    [Fact]
    public async Task Occupancy_ReturnsEveryNightOfTheRange()
    {
        DateOnly night = Today.AddDays(210);
        (_, Guid roomTypeId) = await factory.CreateHotelWithRoomTypeAsync();
        await LoadInventoryAsync(roomTypeId, night, night.AddDays(1));
        await CreateBookingAsync(roomTypeId, night, night.AddDays(1), rooms: 2);

        IReadOnlyList<OccupancyDayResponse>? occupancy = await _client.GetFromJsonAsync<IReadOnlyList<OccupancyDayResponse>>(
            OccupancyUrl(night.AddDays(-1), night.AddDays(1)),
            Cancellation);

        Assert.Equal(
            [new(night.AddDays(-1), 0, 0), new(night, 2, ALLOTMENT), new(night.AddDays(1), 0, ALLOTMENT)],
            occupancy);
    }

    [Theory]
    [InlineData(-1, "dashboard.range_inverted")]
    [InlineData(92, "dashboard.range_too_long")]
    public async Task Occupancy_WithAnInvalidRange_Returns400(int extraDays, string expectedCode)
    {
        HttpResponseMessage response = await _client.GetAsync(OccupancyUrl(Today, Today.AddDays(extraDays)), Cancellation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedCode, await response.ReadErrorCodeAsync());
    }

    private static string OccupancyUrl(DateOnly from, DateOnly to) =>
        $"/api/dashboard/occupancy?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";

    private async Task<DashboardSummaryResponse> GetSummaryAsync(DateOnly date)
    {
        DashboardSummaryResponse? summary = await _client.GetFromJsonAsync<DashboardSummaryResponse>(
            $"/api/dashboard/summary?date={date:yyyy-MM-dd}",
            Cancellation);
        Assert.NotNull(summary);
        return summary;
    }

    private async Task LoadInventoryAsync(Guid roomTypeId, DateOnly from, DateOnly to)
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/room-types/{roomTypeId}/inventory",
            new UpdateInventoryRangeRequest(from, to, TotalRooms: ALLOTMENT, Price: NIGHT_PRICE),
            Cancellation);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<BookingResponse> CreateBookingAsync(Guid roomTypeId, DateOnly checkIn, DateOnly checkOut, int rooms = 1)
    {
        int nights = checkOut.DayNumber - checkIn.DayNumber;
        CreateBookingRequest request = new(
            roomTypeId,
            checkIn,
            checkOut,
            rooms,
            Adults: rooms,
            Children: 0,
            LeadGuestName: "Ana García",
            Channel: "Agency",
            ClientName: "Viajes Sol y Mar",
            QuotedTotal: NIGHT_PRICE * nights * rooms);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/bookings", request, Cancellation);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        BookingResponse? booking = await response.Content.ReadFromJsonAsync<BookingResponse>(Cancellation);
        Assert.NotNull(booking);
        return booking;
    }

    private async Task CancelAsync(Guid bookingId)
    {
        HttpResponseMessage response = await _client.PostAsync($"/api/bookings/{bookingId}/cancel", null, Cancellation);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
