using Altairis.Backoffice.Contracts.RoomTypes;
using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Application.RoomTypes;

public interface IRoomTypeService
{
    Task<Result<IReadOnlyList<RoomTypeResponse>>> ListAsync(Guid hotelId, CancellationToken cancellationToken);

    Task<Result<RoomTypeResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<RoomTypeResponse>> CreateAsync(Guid hotelId, CreateRoomTypeRequest request, CancellationToken cancellationToken);
}
