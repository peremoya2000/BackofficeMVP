namespace Altairis.Backoffice.Contracts.Hotels;

public sealed record HotelListItemResponse(Guid Id, string Name, int Stars, string City, string CountryCode);
