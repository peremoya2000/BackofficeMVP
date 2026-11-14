using Altairis.Backoffice.Contracts.RoomTypes;
using Altairis.Backoffice.Domain.Hotels;

namespace Altairis.Backoffice.Application.RoomTypes;

internal static class RoomTypeMappings
{
    public static RoomTypeResponse ToResponse(this RoomType roomType) => new(
        roomType.Id,
        roomType.HotelId,
        roomType.Name,
        roomType.Category.ToString(),
        roomType.MaxOccupancy);
}