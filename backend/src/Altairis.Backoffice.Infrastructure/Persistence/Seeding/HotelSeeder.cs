using System.Diagnostics;
using Altairis.Backoffice.Domain.Hotels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Altairis.Backoffice.Infrastructure.Persistence.Seeding;

internal sealed class HotelSeeder(BackofficeDbContext dbContext, TimeProvider timeProvider, ILogger<HotelSeeder> logger)
{
    private const int HOTEL_COUNT = 50_000;

    private const string COPY_HOTELS_SQL =
        "COPY hotels (id, name, stars, address, city, country_code, currency, contact_email, contact_phone, created_at_utc) FROM STDIN (FORMAT BINARY)";

    private const string COPY_ROOM_TYPES_SQL =
        "COPY room_types (id, hotel_id, name, category, max_occupancy) FROM STDIN (FORMAT BINARY)";

    public async Task<List<Hotel>> SeedAsync(CancellationToken cancellationToken)
    {
        long startedAt = Stopwatch.GetTimestamp();
        List<Hotel> hotels = SampleHotelGenerator.Generate(HOTEL_COUNT, timeProvider.GetUtcNow().UtcDateTime);

        NpgsqlConnection connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await CopyHotelsAsync(connection, hotels, cancellationToken);
        int roomTypeCount = await CopyRoomTypesAsync(connection, hotels, cancellationToken);

        logger.LogInformation(
            "Seeded {HotelCount} hotels and {RoomTypeCount} room types in {ElapsedMs:N0} ms.",
            hotels.Count,
            roomTypeCount,
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);

        return hotels;
    }

    private static async Task CopyHotelsAsync(NpgsqlConnection connection, List<Hotel> hotels, CancellationToken cancellationToken)
    {
        await using NpgsqlBinaryImporter importer = await connection.BeginBinaryImportAsync(COPY_HOTELS_SQL, cancellationToken);
        foreach (Hotel hotel in hotels)
        {
            await importer.StartRowAsync(cancellationToken);
            await importer.WriteAsync(hotel.Id, cancellationToken);
            await importer.WriteAsync(hotel.Name, cancellationToken);
            await importer.WriteAsync(hotel.Stars, cancellationToken);
            await importer.WriteAsync(hotel.Address, cancellationToken);
            await importer.WriteAsync(hotel.City, cancellationToken);
            await importer.WriteAsync(hotel.CountryCode, cancellationToken);
            await importer.WriteAsync(hotel.Currency, cancellationToken);
            await importer.WriteAsync(hotel.ContactEmail, cancellationToken);
            await importer.WriteAsync(hotel.ContactPhone, cancellationToken);
            await importer.WriteAsync(hotel.CreatedAtUtc, cancellationToken);
        }

        await importer.CompleteAsync(cancellationToken);
    }

    private static async Task<int> CopyRoomTypesAsync(NpgsqlConnection connection, List<Hotel> hotels, CancellationToken cancellationToken)
    {
        await using NpgsqlBinaryImporter importer = await connection.BeginBinaryImportAsync(COPY_ROOM_TYPES_SQL, cancellationToken);
        int count = 0;
        foreach (RoomType roomType in hotels.SelectMany(hotel => hotel.RoomTypes))
        {
            await importer.StartRowAsync(cancellationToken);
            await importer.WriteAsync(roomType.Id, cancellationToken);
            await importer.WriteAsync(roomType.HotelId, cancellationToken);
            await importer.WriteAsync(roomType.Name, cancellationToken);
            await importer.WriteAsync(roomType.Category.ToString(), cancellationToken);
            await importer.WriteAsync(roomType.MaxOccupancy, cancellationToken);
            count++;
        }

        await importer.CompleteAsync(cancellationToken);
        return count;
    }
}
