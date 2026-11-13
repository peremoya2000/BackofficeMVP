using System.ComponentModel;

namespace Altairis.Backoffice.Contracts.Inventory;

public sealed record RoomTypeInventoryResponse(
    Guid RoomTypeId,
    string Name,
    [property: Description("Only the nights that have inventory; a missing date has no allotment.")]
    IReadOnlyList<InventoryDayResponse> Days);
