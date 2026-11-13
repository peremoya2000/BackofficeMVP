using System.ComponentModel;

namespace Altairis.Backoffice.Contracts.Inventory;

[Description("Sets the same values on every night from From to To, both included. A field left empty keeps each night's current value; a night without inventory yet needs TotalRooms and Price.")]
public sealed record UpdateInventoryRangeRequest(
    DateOnly From,
    DateOnly To,
    int? TotalRooms = null,
    decimal? Price = null,
    bool? SalesClosed = null);
