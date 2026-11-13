namespace Altairis.Backoffice.Contracts.Hotels;

public sealed record CreateHotelRequest(
    string Name,
    int Stars,
    string Address,
    string City,
    string CountryCode,
    string? ContactEmail = null,
    string? ContactPhone = null);
