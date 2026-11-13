using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;

namespace Altairis.Backoffice.Domain.Tests.Hotels;

internal static class HotelTestData
{
    public static readonly DateTime CreatedAtUtc = new(2026, 11, 13, 9, 0, 0, DateTimeKind.Utc);

    public static Result<Hotel> Create(
        string name = "Hotel Mar Azul",
        int stars = 4,
        string address = "Passeig Marítim 12",
        string city = "Palma",
        string countryCode = "ES",
        string? contactEmail = null,
        string? contactPhone = null)
    {
        return Hotel.Create(name, stars, address, city, countryCode, contactEmail, contactPhone, CreatedAtUtc);
    }

    public static Hotel CreateValid() => Create().Value;
}
