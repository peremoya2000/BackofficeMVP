using System.Diagnostics;
using System.Globalization;
using System.Text;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;

namespace Altairis.Backoffice.Infrastructure.Persistence.Seeding;

internal static class SampleHotelGenerator
{
    // Fixed so every fresh database gets the same hotels, which keeps the demo script repeatable.
    private const int RANDOM_SEED = 20261113;
    private const int MAX_HOTEL_AGE_DAYS = 3 * 365;
    private const int MAX_STREET_NUMBER = 200;
    private const int MAX_ROOM_TYPES_PER_HOTEL = 4;
    private const double CONTACT_EMAIL_RATIO = 0.8;
    private const double CONTACT_PHONE_RATIO = 0.9;

    private static readonly SampleCity[] Cities =
    [
        new("Madrid", "ES", "+34"), new("Barcelona", "ES", "+34"), new("Sevilla", "ES", "+34"),
        new("Valencia", "ES", "+34"), new("Málaga", "ES", "+34"), new("Palma", "ES", "+34"),
        new("Granada", "ES", "+34"), new("Bilbao", "ES", "+34"), new("San Sebastián", "ES", "+34"),
        new("Alicante", "ES", "+34"), new("Benidorm", "ES", "+34"), new("Marbella", "ES", "+34"),
        new("Santiago de Compostela", "ES", "+34"), new("Salamanca", "ES", "+34"), new("Toledo", "ES", "+34"),
        new("Córdoba", "ES", "+34"), new("Cádiz", "ES", "+34"), new("Ibiza", "ES", "+34"),
        new("Santa Cruz de Tenerife", "ES", "+34"), new("Las Palmas de Gran Canaria", "ES", "+34"),
        new("Zaragoza", "ES", "+34"), new("Girona", "ES", "+34"), new("Tarragona", "ES", "+34"),
        new("Lisboa", "PT", "+351"), new("Oporto", "PT", "+351"), new("Faro", "PT", "+351"), new("Funchal", "PT", "+351"),
        new("París", "FR", "+33"), new("Niza", "FR", "+33"), new("Lyon", "FR", "+33"), new("Burdeos", "FR", "+33"),
        new("Roma", "IT", "+39"), new("Florencia", "IT", "+39"), new("Venecia", "IT", "+39"), new("Milán", "IT", "+39"),
        new("Londres", "GB", "+44"), new("Edimburgo", "GB", "+44"),
        new("Berlín", "DE", "+49"), new("Múnich", "DE", "+49"),
        new("Ámsterdam", "NL", "+31"), new("Atenas", "GR", "+30"), new("Marrakech", "MA", "+212"),
        new("Cancún", "MX", "+52"), new("Ciudad de México", "MX", "+52"), new("Punta Cana", "DO", "+1"),
        new("La Habana", "CU", "+53"), new("Buenos Aires", "AR", "+54"), new("Nueva York", "US", "+1"), new("Miami", "US", "+1"),
    ];

    private static readonly string[] NamePrefixes =
        ["Hotel", "Gran Hotel", "Hostal", "Apartahotel", "Hotel Boutique", "Resort", "Posada"];

    private static readonly string[] NameThemes =
    [
        "Mirador", "Las Palmeras", "Del Mar", "Plaza", "Real", "Sol y Luna", "Alameda", "Bahía", "Puerta Antigua", "Los Olivos",
        "La Muralla", "El Faro", "Jardines", "San Miguel", "Santa Cruz", "Atlántico", "Mediterráneo", "Los Arcos", "La Ribera", "Catedral",
    ];

    // Contracted hotels use theme words found nowhere else, so a search finds exactly one of each.
    private static readonly string[] PortfolioThemes =
    [
        "Atalaya", "Alcázar", "Azahar", "Brisamar", "Candelaria", "Cigarral", "Duquesa", "Esmeralda", "Galeón", "Gaviota",
        "Jacaranda", "Lavanda", "Levante", "Magnolia", "Marqués", "Mirabel", "Montecristo", "Nogal", "Oasis", "Orquídea",
        "Perla Azul", "Quinta Rosa", "Rocamar", "Salinas", "Sirena", "Tamarindo", "Torre Blanca", "Tramontana", "Ulises", "Vela Latina",
        "Veranda", "Vistalegre", "Zafiro", "Albatros", "Buganvilla", "Coral", "Estrella Polar", "Granados", "Hesperia", "Ínsula",
    ];

    private static readonly string[] NameSuffixes = ["", "", "", "Centro", "Playa", "Puerto", "Casco Antiguo", "Estación"];

    private static readonly string[] Streets =
    [
        "Calle Mayor", "Avenida de la Constitución", "Paseo Marítimo", "Calle Real", "Plaza de España", "Avenida del Puerto",
        "Calle del Carmen", "Gran Vía", "Rambla Nueva", "Calle San Francisco", "Avenida de Andalucía", "Calle de la Paz",
    ];

    private static readonly int[] StarsDistribution = [1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5];

    private static readonly SampleRoomType[] RoomTypes =
    [
        new("Individual", RoomCategory.Single, 1),
        new("Doble Estándar", RoomCategory.Double, 2),
        new("Doble Superior", RoomCategory.Double, 2),
        new("Twin", RoomCategory.Twin, 2),
        new("Triple", RoomCategory.Triple, 3),
        new("Familiar", RoomCategory.Family, 4),
        new("Junior Suite", RoomCategory.Suite, 3),
        new("Suite", RoomCategory.Suite, 4),
    ];

    public static int PortfolioHotelCount => PortfolioThemes.Length;

    public static List<Hotel> Generate(int count, DateTime nowUtc)
    {
        var random = new Random(RANDOM_SEED);
        var hotels = new List<Hotel>(count);
        for (int index = 0; index < count; index++)
        {
            string theme = index < PortfolioThemes.Length ? PortfolioThemes[index] : Pick(random, NameThemes);
            hotels.Add(GenerateHotel(random, theme, nowUtc));
        }

        return hotels;
    }

    private static Hotel GenerateHotel(Random random, string theme, DateTime nowUtc)
    {
        SampleCity city = Pick(random, Cities);
        string name = $"{Pick(random, NamePrefixes)} {theme} {city.Name} {Pick(random, NameSuffixes)}".TrimEnd();
        string address = $"{Pick(random, Streets)}, {random.Next(1, MAX_STREET_NUMBER + 1)}";
        string? email = random.NextDouble() < CONTACT_EMAIL_RATIO ? $"reservas@{Slugify(theme)}-{Slugify(city.Name)}.example" : null;
        string? phone = random.NextDouble() < CONTACT_PHONE_RATIO ? $"{city.PhonePrefix} {random.Next(100, 1000)} {random.Next(100, 1000)} {random.Next(100, 1000)}" : null;
        DateTime createdAtUtc = nowUtc.AddDays(-random.NextDouble() * MAX_HOTEL_AGE_DAYS);

        Hotel hotel = Hotel.Create(name, Pick(random, StarsDistribution), address, city.Name, city.CountryCode, email, phone, createdAtUtc).Value;

        SampleRoomType[] roomTypes = RoomTypes.ToArray();
        random.Shuffle(roomTypes);
        foreach (SampleRoomType roomType in roomTypes.Take(random.Next(1, MAX_ROOM_TYPES_PER_HOTEL + 1)))
        {
            Result<RoomType> added = hotel.AddRoomType(roomType.Name, roomType.Category, roomType.MaxOccupancy);
            Debug.Assert(added.IsSuccess, $"Sample room type '{roomType.Name}' was rejected by the domain.");
        }

        return hotel;
    }

    private static T Pick<T>(Random random, T[] items) => items[random.Next(items.Length)];

    private static string Slugify(string text)
    {
        var slug = new StringBuilder(text.Length);
        foreach (char character in text.ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                slug.Append(character);
            }
            else if (character == ' ')
            {
                slug.Append('-');
            }
        }

        return slug.ToString();
    }

    private sealed record SampleCity(string Name, string CountryCode, string PhonePrefix);

    private sealed record SampleRoomType(string Name, RoomCategory Category, int MaxOccupancy);
}
