using Altairis.Backoffice.Application.Hotels;
using Microsoft.Extensions.DependencyInjection;

namespace Altairis.Backoffice.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IHotelService, HotelService>();

        return services;
    }
}
