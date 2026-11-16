using System.Net;
using System.Net.Http.Json;
using Altairis.Backoffice.Contracts.RoomTypes;

namespace Altairis.Backoffice.Api.IntegrationTests.RoomTypes;

public sealed class RoomTypesApiTests(BackofficeApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Create_WithValidRequest_Returns201AndTheHotelListsIt()
    {
        (Guid hotelId, _) = await factory.CreateHotelWithRoomTypeAsync();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            RoomTypesUrl(hotelId),
            new CreateRoomTypeRequest("  Suite Junior  ", "Suite", 3),
            Cancellation);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        RoomTypeResponse? created = await response.Content.ReadFromJsonAsync<RoomTypeResponse>(Cancellation);
        Assert.NotNull(created);
        Assert.Equal(new RoomTypeResponse(created.Id, hotelId, "Suite Junior", "Suite", 3), created);

        RoomTypeResponse? fetched = await _client.GetFromJsonAsync<RoomTypeResponse>(response.Headers.Location, Cancellation);
        Assert.Equal(created, fetched);

        List<RoomTypeResponse>? listed = await _client.GetFromJsonAsync<List<RoomTypeResponse>>(RoomTypesUrl(hotelId), Cancellation);
        Assert.NotNull(listed);
        Assert.Equal(["Doble estándar", "Suite Junior"], listed.Select(roomType => roomType.Name));
    }

    [Fact]
    public async Task Create_WithANameTheHotelAlreadyUses_Returns409()
    {
        (Guid hotelId, _) = await factory.CreateHotelWithRoomTypeAsync();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            RoomTypesUrl(hotelId),
            new CreateRoomTypeRequest("doble estándar", "Suite", 3),
            Cancellation);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("room_type.duplicate_name", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task Create_WhenManyRequestsRaceForTheSameName_CreatesOnlyOne()
    {
        const int REQUESTS = 10;
        (Guid hotelId, _) = await factory.CreateHotelWithRoomTypeAsync();
        CreateRoomTypeRequest request = new("Suite Junior", "Suite", 3);

        HttpResponseMessage[] responses = await Task.WhenAll(
            Enumerable.Range(0, REQUESTS).Select(_ => _client.PostAsJsonAsync(RoomTypesUrl(hotelId), request, Cancellation)));

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Equal(REQUESTS - 1, responses.Count(response => response.StatusCode == HttpStatusCode.Conflict));
    }

    [Fact]
    public async Task Create_WithUnknownCategory_Returns400()
    {
        (Guid hotelId, _) = await factory.CreateHotelWithRoomTypeAsync();

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            RoomTypesUrl(hotelId),
            new CreateRoomTypeRequest("Suite Junior", "Unknown category red", 3),
            Cancellation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("room_type.category_invalid", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task List_WhenHotelDoesNotExist_Returns404()
    {
        HttpResponseMessage response = await _client.GetAsync(RoomTypesUrl(Guid.CreateVersion7()), Cancellation);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("hotel.not_found", await response.ReadErrorCodeAsync());
    }

    private static string RoomTypesUrl(Guid hotelId) => $"/api/hotels/{hotelId}/room-types";
}