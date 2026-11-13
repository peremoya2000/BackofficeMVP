using Altairis.Backoffice.Contracts.Hotels;
using Altairis.Backoffice.Domain.Hotels;

namespace Altairis.Backoffice.Application.Hotels;

internal static class HotelMappings
{
    public static HotelResponse ToResponse(this Hotel hotel) => new(
        hotel.Id,
        hotel.Name,
        hotel.Stars,
        hotel.Address,
        hotel.City,
        hotel.CountryCode,
        hotel.Currency,
        hotel.ContactEmail,
        hotel.ContactPhone,
        hotel.CreatedAtUtc);
}
