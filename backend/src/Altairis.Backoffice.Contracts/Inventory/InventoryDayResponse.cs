namespace Altairis.Backoffice.Contracts.Inventory;

public sealed record InventoryDayResponse(DateOnly Date, int TotalRooms, int BookedRooms, decimal Price, bool SalesClosed);
