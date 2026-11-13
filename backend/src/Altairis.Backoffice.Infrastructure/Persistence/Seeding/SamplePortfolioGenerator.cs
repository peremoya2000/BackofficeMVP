using System.Diagnostics;
using Altairis.Backoffice.Application.Common;
using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;
using Altairis.Backoffice.Domain.Inventory;

namespace Altairis.Backoffice.Infrastructure.Persistence.Seeding;

internal static class SamplePortfolioGenerator
{
    private const int RANDOM_SEED = 20261114;
    private const int PAST_DAYS = 60;
    private const int FUTURE_DAYS = 120;
    private const int MAX_LEAD_DAYS = 120;
    private const int BOOKING_ATTEMPTS_PER_ROOM = 70;
    private const int MAX_SALES_CLOSED_START_DAYS = 60;
    private const int MIN_SALES_CLOSED_NIGHTS = 2;
    private const int MAX_SALES_CLOSED_NIGHTS = 5;
    private const double SALES_CLOSED_RATIO = 0.15;
    private const double CANCELLATION_RATIO = 0.1;
    private const double NOTES_RATIO = 0.1;
    private const double TWO_ADULTS_RATIO = 0.75;
    private const double CHILDREN_RATIO = 0.3;
    private const double HOTEL_PRICE_VARIATION = 0.2;
    private const decimal WEEKEND_PRICE_FACTOR = 1.15m;

    private static readonly decimal[] BasePriceByStars = [45m, 60m, 85m, 120m, 220m];

    private static readonly decimal[] SeasonFactorByMonth = [0.85m, 0.85m, 0.95m, 1m, 1.05m, 1.2m, 1.35m, 1.4m, 1.1m, 1m, 0.85m, 1.05m];

    private static readonly int[] NightsDistribution = [1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 5, 7];

    private static readonly int[] RoomsDistribution = [1, 1, 1, 1, 1, 1, 1, 1, 2, 3];

    private static readonly SampleGuestNames SpanishGuestNames = new(
        ["Lucía", "Javier", "María", "Carlos", "Ana", "Pablo", "Sofía", "Miguel", "Laura", "David", "Carmen", "Jorge"],
        ["García", "Martínez", "López", "Sánchez", "Fernández", "Romero", "Navarro", "Torres", "Ruiz", "Ortega"]);

    // Names are paired by origin; Spanish appears twice because it is the main market.
    private static readonly SampleGuestNames[] GuestNames =
    [
        SpanishGuestNames,
        SpanishGuestNames,
        new(["Emma", "James", "Olivia", "Thomas", "Sophie", "Jack"], ["Smith", "Johnson", "Brown", "Taylor", "Wilson"]),
        new(["Lukas", "Anna", "Felix", "Lena"], ["Müller", "Schmidt", "Weber", "Fischer"]),
        new(["Giulia", "Marco", "Chiara", "Luca"], ["Rossi", "Bianchi", "Esposito", "Romano"]),
        new(["Pierre", "Camille", "Louis", "Chloé"], ["Dubois", "Moreau", "Laurent", "Lefebvre"]),
    ];

    // Each entry is equally likely, so repeating a channel sets the mix; a null name means the guest booked directly.
    private static readonly SampleClient[] Clients =
    [
        new("Viajes Horizonte", BookingChannel.Agency, "VH"),
        new("Rutas del Sur Viajes", BookingChannel.Agency, "RS"),
        new("Agencia Brújula", BookingChannel.Agency, "BRJ"),
        new("Viajes Atlántida", BookingChannel.Agency, "ATL"),
        new("Mundo Azul Tours", BookingChannel.TourOperator, "MAT"),
        new("Meridiano Tour Operador", BookingChannel.TourOperator, "MER"),
        new("Lumen Holidays", BookingChannel.TourOperator, "LUM"),
        new("Consultora Atlas", BookingChannel.Corporate, null),
        new("Ingeniería Cierzo", BookingChannel.Corporate, null),
        new(null, BookingChannel.Direct, null),
    ];

    private static readonly string[] Notes =
    [
        "Llegada tardía, después de las 22:00", "Cuna en la habitación", "Cama supletoria para un niño", "Celebran su aniversario",
        "Cliente habitual", "Planta alta si es posible", "Alergia al gluten", "Traslado desde el aeropuerto contratado aparte",
    ];

    public static SamplePortfolio Generate(IEnumerable<Hotel> hotels, DateTime nowUtc)
    {
        var random = new Random(RANDOM_SEED);
        DateOnly today = OperationsCalendar.DateOf(nowUtc);
        var portfolio = new SamplePortfolio([], []);
        var usedReferences = new HashSet<string>();
        foreach (Hotel hotel in hotels)
        {
            foreach (RoomType roomType in hotel.RoomTypes)
            {
                List<InventoryDay> nights = GenerateInventory(random, hotel, roomType, today);
                portfolio.Bookings.AddRange(GenerateBookings(random, hotel, roomType, nights, nowUtc, usedReferences));
                portfolio.Inventory.AddRange(nights);
            }
        }

        return portfolio;
    }

    private static List<InventoryDay> GenerateInventory(Random random, Hotel hotel, RoomType roomType, DateOnly today)
    {
        int totalRooms = AllotmentFor(random, roomType.Category);
        decimal hotelPriceFactor = (decimal)(1 + (random.NextDouble() * 2 - 1) * HOTEL_PRICE_VARIATION);
        decimal basePrice = BasePriceByStars[hotel.Stars - 1] * PriceFactorFor(roomType.Category) * hotelPriceFactor;
        int salesClosedFrom = PAST_DAYS + random.Next(MAX_SALES_CLOSED_START_DAYS);
        int salesClosedNights = random.NextDouble() < SALES_CLOSED_RATIO ? random.Next(MIN_SALES_CLOSED_NIGHTS, MAX_SALES_CLOSED_NIGHTS + 1) : 0;

        DateOnly firstNight = today.AddDays(-PAST_DAYS);
        var nights = new List<InventoryDay>(PAST_DAYS + FUTURE_DAYS);
        for (int offset = 0; offset < PAST_DAYS + FUTURE_DAYS; offset++)
        {
            DateOnly night = firstNight.AddDays(offset);
            decimal price = Math.Round(basePrice * NightPriceFactor(night));
            bool salesClosed = offset >= salesClosedFrom && offset < salesClosedFrom + salesClosedNights;
            nights.Add(InventoryDay.Create(roomType.Id, night, totalRooms, price, salesClosed).Value);
        }

        return nights;
    }

    private static List<Booking> GenerateBookings(
        Random random,
        Hotel hotel,
        RoomType roomType,
        List<InventoryDay> nights,
        DateTime nowUtc,
        HashSet<string> usedReferences)
    {
        var bookings = new List<Booking>();
        int attempts = nights[0].TotalRooms * BOOKING_ATTEMPTS_PER_ROOM;
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            DateTime createdAtUtc = nowUtc.AddDays(-random.NextDouble() * (PAST_DAYS + MAX_LEAD_DAYS));
            if (createdAtUtc < hotel.CreatedAtUtc) continue;

            DateOnly createdOn = OperationsCalendar.DateOf(createdAtUtc);
            // Squaring skews lead times short, so most bookings arrive in the weeks before check-in.
            DateOnly checkIn = createdOn.AddDays((int)(MAX_LEAD_DAYS * Math.Pow(random.NextDouble(), 2)));
            int firstNightIndex = checkIn.DayNumber - nights[0].Date.DayNumber;
            int nightCount = Pick(random, NightsDistribution);
            if (firstNightIndex < 0 || firstNightIndex + nightCount > nights.Count) continue;

            StayPeriod stay = StayPeriod.Create(checkIn, checkIn.AddDays(nightCount)).Value;
            List<InventoryDay> stayNights = nights.GetRange(firstNightIndex, nightCount);
            int rooms = Pick(random, RoomsDistribution);
            Result<Money> quote = Booking.Quote(roomType.Id, stay, rooms, stayNights, hotel.Currency, createdOn);
            if (quote.IsFailure) continue;

            int adults = roomType.MaxOccupancy >= 2 && random.NextDouble() < TWO_ADULTS_RATIO ? rooms * 2 : rooms;
            int freeBeds = rooms * roomType.MaxOccupancy - adults;
            int children = freeBeds > 0 && random.NextDouble() < CHILDREN_RATIO ? random.Next(1, freeBeds + 1) : 0;
            SampleGuestNames names = Pick(random, GuestNames);
            string leadGuestName = $"{Pick(random, names.FirstNames)} {Pick(random, names.LastNames)}";
            string? notes = random.NextDouble() < NOTES_RATIO ? Pick(random, Notes) : null;
            SampleClient client = Pick(random, Clients);
            string? clientReference = client.ReferencePrefix is null ? null : $"{client.ReferencePrefix}-{random.Next(100_000, 1_000_000)}";

            Booking booking = Booking.Create(
                roomType,
                stay,
                rooms,
                adults,
                children,
                leadGuestName,
                notes,
                client.Channel,
                client.Name ?? leadGuestName,
                clientReference,
                stayNights,
                hotel.Currency,
                quote.Value,
                createdOn,
                createdAtUtc).Value;

            // References are random, so the rare duplicate is cancelled to give its rooms back and is never saved.
            if (!usedReferences.Add(booking.Reference))
            {
                Result released = booking.Cancel(stayNights, createdOn, createdAtUtc);
                Debug.Assert(released.IsSuccess, "Cancelling a just-created sample booking cannot fail.");
                continue;
            }

            DateTime checkInUtc = stay.CheckIn.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            TimeSpan cancellationWindow = (checkInUtc < nowUtc ? checkInUtc : nowUtc) - createdAtUtc;
            if (cancellationWindow > TimeSpan.Zero && random.NextDouble() < CANCELLATION_RATIO)
            {
                DateTime cancelledAtUtc = createdAtUtc + cancellationWindow * random.NextDouble();
                Result cancelled = booking.Cancel(stayNights, OperationsCalendar.DateOf(cancelledAtUtc), cancelledAtUtc);
                Debug.Assert(cancelled.IsSuccess, "Sample cancellations happen before check-in, so the domain accepts them.");
            }

            bookings.Add(booking);
        }

        return bookings;
    }

    private static int AllotmentFor(Random random, RoomCategory category) => category switch
    {
        RoomCategory.Double or RoomCategory.Twin => random.Next(4, 9),
        RoomCategory.Suite => random.Next(1, 3),
        _ => random.Next(2, 5),
    };

    private static decimal PriceFactorFor(RoomCategory category) => category switch
    {
        RoomCategory.Single => 0.8m,
        RoomCategory.Triple => 1.3m,
        RoomCategory.Family => 1.5m,
        RoomCategory.Suite => 2.2m,
        _ => 1m,
    };

    private static decimal NightPriceFactor(DateOnly night)
    {
        decimal season = SeasonFactorByMonth[night.Month - 1];
        bool weekendNight = night.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday;

        return weekendNight ? season * WEEKEND_PRICE_FACTOR : season;
    }

    private static T Pick<T>(Random random, T[] items) => items[random.Next(items.Length)];

    private sealed record SampleClient(string? Name, BookingChannel Channel, string? ReferencePrefix);

    private sealed record SampleGuestNames(string[] FirstNames, string[] LastNames);
}
