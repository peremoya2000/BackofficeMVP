using System.Net;
using System.Net.Http.Json;
using Altairis.Backoffice.Contracts.Common;
using Altairis.Backoffice.Contracts.Hotels;

namespace Altairis.Backoffice.Api.IntegrationTests.Hotels;

public sealed class HotelsApiTests(BackofficeApiFactory factory)
{
    private const string HOTELS_URL = "/api/hotels";
    private const int SEEDED_HOTEL_COUNT = 50_000;

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Create_WithValidRequest_Returns201AndTheHotelCanBeFetched()
    {
        CreateHotelRequest request = NewHotelRequest(name: "  Hotel Integración  ", countryCode: "es");

        HttpResponseMessage response = await _client.PostAsJsonAsync(HOTELS_URL, request, Cancellation);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        HotelResponse? created = await response.Content.ReadFromJsonAsync<HotelResponse>(Cancellation);
        Assert.NotNull(created);
        Assert.Equal("Hotel Integración", created.Name);
        Assert.Equal("ES", created.CountryCode);

        HotelResponse? fetched = await _client.GetFromJsonAsync<HotelResponse>(response.Headers.Location, Cancellation);
        Assert.NotNull(fetched);
        // Postgres keeps microseconds, .NET keeps 100 ns ticks.
        Assert.Equal(created.CreatedAtUtc, fetched.CreatedAtUtc, TimeSpan.FromMicroseconds(1));
        Assert.Equal(created, fetched with { CreatedAtUtc = created.CreatedAtUtc });
    }

    [Fact]
    public async Task Create_WithInvalidCountry_Returns400WithDomainErrorCode()
    {
        CreateHotelRequest request = NewHotelRequest(countryCode: "XX");

        HttpResponseMessage response = await _client.PostAsJsonAsync(HOTELS_URL, request, Cancellation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("hotel.country_code_invalid", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task GetById_WhenHotelDoesNotExist_Returns404WithErrorCode()
    {
        HttpResponseMessage response = await _client.GetAsync($"{HOTELS_URL}/{Guid.CreateVersion7()}", Cancellation);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("hotel.not_found", await response.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task Search_MatchesNameOrCityIgnoringCase()
    {
        string token = NewToken();
        HotelResponse byName = await CreateHotelAsync(NewHotelRequest(name: $"Hotel {token}"));
        HotelResponse byCity = await CreateHotelAsync(NewHotelRequest(city: $"Ciudad {token}"));

        PagedResponse<HotelListItemResponse> page = await SearchAsync($"search={token.ToUpperInvariant()}");

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(new[] { byName.Id, byCity.Id }.Order(), page.Items.Select(hotel => hotel.Id).Order());
    }

    [Theory]
    [InlineData("Ávila", "avila")]
    [InlineData("Avila", "ÁVILA")]
    public async Task Search_MatchesNameOrCityIgnoringAccents(string stored, string searched)
    {
        string token = NewToken();
        HotelResponse byName = await CreateHotelAsync(NewHotelRequest(name: $"Hotel {stored} {token}"));
        HotelResponse byCity = await CreateHotelAsync(NewHotelRequest(city: $"{stored} {token}"));

        PagedResponse<HotelListItemResponse> page = await SearchAsync($"search={searched} {token}");

        Assert.Equal(new[] { byName.Id, byCity.Id }.Order(), page.Items.Select(hotel => hotel.Id).Order());
    }

    [Theory]
    [InlineData("_")]
    [InlineData("%")]
    [InlineData(@"\")]
    public async Task Search_TreatsLikeSpecialCharactersAsLiteralText(string special)
    {
        string token = NewToken();
        HotelResponse literal = await CreateHotelAsync(NewHotelRequest(name: $"Hotel {token}{special}A"));
        await CreateHotelAsync(NewHotelRequest(name: $"Hotel {token}ZA"));

        PagedResponse<HotelListItemResponse> page = await SearchAsync($"search={Uri.EscapeDataString($"{token}{special}a")}");

        HotelListItemResponse match = Assert.Single(page.Items);
        Assert.Equal(literal.Id, match.Id);
    }

    [Fact]
    public async Task Search_FiltersByCountryIgnoringCaseAndByStars()
    {
        string token = NewToken();
        HotelResponse match = await CreateHotelAsync(NewHotelRequest(name: $"Hotel {token}", countryCode: "PT", stars: 4));
        await CreateHotelAsync(NewHotelRequest(name: $"Hotel {token}", countryCode: "ES", stars: 4));
        await CreateHotelAsync(NewHotelRequest(name: $"Hotel {token}", countryCode: "PT", stars: 3));

        PagedResponse<HotelListItemResponse> page = await SearchAsync($"search={token}&countryCode=pt&stars=4");

        HotelListItemResponse found = Assert.Single(page.Items);
        Assert.Equal(match.Id, found.Id);
    }

    [Fact]
    public async Task Search_PagesThroughSeededHotelsInSortOrder()
    {
        PagedResponse<HotelListItemResponse> first = await SearchAsync("sort=-stars&pageSize=5&page=1");
        PagedResponse<HotelListItemResponse> second = await SearchAsync("sort=-stars&pageSize=5&page=2");

        Assert.True(first.TotalCount >= SEEDED_HOTEL_COUNT);
        Assert.Equal(5, first.Items.Count);
        Assert.Equal(5, second.Items.Count);
        Assert.Empty(first.Items.Select(hotel => hotel.Id).Intersect(second.Items.Select(hotel => hotel.Id)));
        Assert.All(first.Items.Concat(second.Items), hotel => Assert.Equal(5, hotel.Stars));
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=101")]
    [InlineData("sort=address")]
    public async Task Search_WithInvalidQuery_Returns400(string query)
    {
        HttpResponseMessage response = await _client.GetAsync($"{HOTELS_URL}?{query}", Cancellation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static CreateHotelRequest NewHotelRequest(
        string name = "Hotel de prueba",
        string city = "Palma",
        string countryCode = "ES",
        int stars = 3) =>
        new(name, stars, Address: "Calle de prueba 1", city, countryCode, ContactEmail: null, ContactPhone: null);

    private static string NewToken() => Guid.NewGuid().ToString("N")[..12];

    private async Task<HotelResponse> CreateHotelAsync(CreateHotelRequest request)
    {
        HttpResponseMessage response = await _client.PostAsJsonAsync(HOTELS_URL, request, Cancellation);
        response.EnsureSuccessStatusCode();
        HotelResponse? created = await response.Content.ReadFromJsonAsync<HotelResponse>(Cancellation);
        Assert.NotNull(created);
        return created;
    }

    private async Task<PagedResponse<HotelListItemResponse>> SearchAsync(string query)
    {
        PagedResponse<HotelListItemResponse>? page = await _client.GetFromJsonAsync<PagedResponse<HotelListItemResponse>>(
            $"{HOTELS_URL}?{query}",
            Cancellation);
        Assert.NotNull(page);
        return page;
    }
}
