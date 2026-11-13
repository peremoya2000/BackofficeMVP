using System.Net;
using System.Net.Http.Json;
using Altairis.Backoffice.Application.Common;
using Altairis.Backoffice.Contracts.Inventory;
using Altairis.Backoffice.Domain.Hotels;
using Microsoft.Extensions.DependencyInjection;

namespace Altairis.Backoffice.Api.IntegrationTests.Inventory;

public sealed class InventoryApiTests(BackofficeApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private static DateOnly Today => OperationsCalendar.Today(TimeProvider.System);

    [Fact]
    public async Task UpdateRange_CreatesNightsThenChangesOnlyTheFieldsSent()
    {
        (Guid hotelId, Guid roomTypeId) = await CreateHotelWithRoomTypeAsync();
        DateOnly from = Today.AddDays(10);
        DateOnly to = from.AddDays(2);

        await UpdateRangeAsync(roomTypeId, new UpdateInventoryRangeRequest(from, to, TotalRooms: 5, Price: 100m));
        await UpdateRangeAsync(roomTypeId, new UpdateInventoryRangeRequest(from.AddDays(1), to, SalesClosed: true));

        InventoryCalendarResponse calendar = await GetCalendarAsync(hotelId, from.AddDays(-1), to.AddDays(1));
        RoomTypeInventoryResponse roomType = Assert.Single(calendar.RoomTypes);
        InventoryDayResponse[] expected =
        [
            new(from, TotalRooms: 5, BookedRooms: 0, Price: 100m, SalesClosed: false),
            new(from.AddDays(1), TotalRooms: 5, BookedRooms: 0, Price: 100m, SalesClosed: true),
            new(to, TotalRooms: 5, BookedRooms: 0, Price: 100m, SalesClosed: true),
        ];
        Assert.Equal(expected, roomType.Days);
    }

    [Fact]
    public async Task UpdateRange_WhenANewNightHasNoPrice_Returns400AndSavesNoNight()
    {
        (Guid hotelId, Guid roomTypeId) = await CreateHotelWithRoomTypeAsync();
        DateOnly from = Today.AddDays(10);
        await UpdateRangeAsync(roomTypeId, new UpdateInventoryRangeRequest(from, from, TotalRooms: 5, Price: 100m));

        HttpResponseMessage response = await PutAsync(roomTypeId, new UpdateInventoryRangeRequest(from, from.AddDays(1), TotalRooms: 3));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("inventory.new_night_incomplete", await response.ReadErrorCodeAsync());
        InventoryCalendarResponse calendar = await GetCalendarAsync(hotelId, from, from.AddDays(1));
        InventoryDayResponse night = Assert.Single(Assert.Single(calendar.RoomTypes).Days);
        Assert.Equal(5, night.TotalRooms);
    }

    [Fact]
    public async Task UpdateRange_ForPastNights_Returns400()
    {
        (_, Guid roomTypeId) = await CreateHotelWithRoomTypeAsync();

        HttpResponseMessage response = await PutAsync(
            roomTypeId,
            new UpdateInventoryRangeRequest(Today.AddDays(-1), Today, TotalRooms: 5, Price: 100m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("inventory.past_nights", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task UpdateRange_WhenRoomTypeDoesNotExist_Returns404()
    {
        HttpResponseMessage response = await PutAsync(
            Guid.CreateVersion7(),
            new UpdateInventoryRangeRequest(Today, Today, TotalRooms: 5, Price: 100m));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("room_type.not_found", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task GetCalendar_WhenHotelDoesNotExist_Returns404()
    {
        HttpResponseMessage response = await _client.GetAsync(
            $"/api/hotels/{Guid.CreateVersion7()}/inventory?from=2026-01-01&to=2026-01-31",
            Cancellation);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("hotel.not_found", await response.ReadErrorCodeAsync());
    }

    [Theory]
    [InlineData("from=2026-01-10&to=2026-01-01")]
    [InlineData("from=2026-01-01&to=2026-03-04")]
    [InlineData("from=2026-01-01")]
    public async Task GetCalendar_WithInvalidRange_Returns400(string query)
    {
        (Guid hotelId, _) = await CreateHotelWithRoomTypeAsync();

        HttpResponseMessage response = await _client.GetAsync($"/api/hotels/{hotelId}/inventory?{query}", Cancellation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<(Guid HotelId, Guid RoomTypeId)> CreateHotelWithRoomTypeAsync()
    {
        Hotel hotel = Hotel.Create("Hotel inventario", 3, "Calle de prueba 1", "Palma", "ES", null, null, DateTime.UtcNow).Value;
        RoomType roomType = hotel.AddRoomType("Doble estándar", RoomCategory.Double, 2).Value;

        using IServiceScope scope = factory.Services.CreateScope();
        IBackofficeDbContext dbContext = scope.ServiceProvider.GetRequiredService<IBackofficeDbContext>();
        dbContext.Hotels.Add(hotel);
        await dbContext.SaveChangesAsync(Cancellation);

        return (hotel.Id, roomType.Id);
    }

    private Task<HttpResponseMessage> PutAsync(Guid roomTypeId, UpdateInventoryRangeRequest request) =>
        _client.PutAsJsonAsync($"/api/room-types/{roomTypeId}/inventory", request, Cancellation);

    private async Task UpdateRangeAsync(Guid roomTypeId, UpdateInventoryRangeRequest request)
    {
        HttpResponseMessage response = await PutAsync(roomTypeId, request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<InventoryCalendarResponse> GetCalendarAsync(Guid hotelId, DateOnly from, DateOnly to)
    {
        InventoryCalendarResponse? calendar = await _client.GetFromJsonAsync<InventoryCalendarResponse>(
            $"/api/hotels/{hotelId}/inventory?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}",
            Cancellation);
        Assert.NotNull(calendar);
        return calendar;
    }
}
