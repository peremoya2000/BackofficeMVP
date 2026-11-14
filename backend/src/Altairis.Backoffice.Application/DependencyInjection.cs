using Altairis.Backoffice.Application.Bookings;
using Altairis.Backoffice.Application.Dashboard;
using Altairis.Backoffice.Application.Hotels;
using Altairis.Backoffice.Application.Inventory;
using Altairis.Backoffice.Application.RoomTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Altairis.Backoffice.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IHotelService, HotelService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IRoomTypeService, RoomTypeService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
