namespace Altairis.Backoffice.Contracts.Inventory;

public sealed record InventoryCalendarResponse(
    Guid HotelId,
    string HotelName,
    string Currency,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<RoomTypeInventoryResponse> RoomTypes);
