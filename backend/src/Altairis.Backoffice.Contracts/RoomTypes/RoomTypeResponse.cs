namespace Altairis.Backoffice.Contracts.RoomTypes;

public sealed record RoomTypeResponse(Guid Id, Guid HotelId, string Name, string Category, int MaxOccupancy);