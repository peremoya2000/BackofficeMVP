using System.ComponentModel;

namespace Altairis.Backoffice.Contracts.RoomTypes;

public sealed record CreateRoomTypeRequest(
    string Name,
    [property: Description("Single, Double, Twin, Triple, Family or Suite.")] string Category,
    int MaxOccupancy);
