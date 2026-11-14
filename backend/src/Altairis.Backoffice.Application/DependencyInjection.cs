using Altairis.Backoffice.Application.Bookings;
using Altairis.Backoffice.Application.Hotels;
using Altairis.Backoffice.Application.Inventory;
using Microsoft.Extensions.DependencyInjection;
using Altairis.Backoffice.Application.RoomTypes;

namespace Altairis.Backoffice.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IRoomTypeService, RoomTypeService>();

        return services;
    }
}
