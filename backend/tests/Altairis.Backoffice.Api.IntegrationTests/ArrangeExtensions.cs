using Altairis.Backoffice.Application.Common;
using Altairis.Backoffice.Domain.Hotels;
using Microsoft.Extensions.DependencyInjection;

namespace Altairis.Backoffice.Api.IntegrationTests;

internal static class ArrangeExtensions
{
    // There's no room types API yet, so the hotel is built through the domain and saved with the DbContext.
    public static async Task<(Guid HotelId, Guid RoomTypeId)> CreateHotelWithRoomTypeAsync(this BackofficeApiFactory factory)
    {
        Hotel hotel = Hotel.Create("Hotel de prueba", 3, "Calle de prueba 1", "Palma", "ES", null, null, DateTime.UtcNow).Value;
        RoomType roomType = hotel.AddRoomType("Doble estándar", RoomCategory.Double, 2).Value;

        using IServiceScope scope = factory.Services.CreateScope();
        IBackofficeDbContext dbContext = scope.ServiceProvider.GetRequiredService<IBackofficeDbContext>();
        dbContext.Hotels.Add(hotel);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (hotel.Id, roomType.Id);
    }
}
