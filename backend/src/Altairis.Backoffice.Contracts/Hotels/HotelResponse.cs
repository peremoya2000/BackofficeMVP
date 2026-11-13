namespace Altairis.Backoffice.Contracts.Hotels;

public sealed record HotelResponse(
    Guid Id,
    string Name,
    int Stars,
    string Address,
    string City,
    string CountryCode,
    string Currency,
    string? ContactEmail,
    string? ContactPhone,
    DateTime CreatedAtUtc);
