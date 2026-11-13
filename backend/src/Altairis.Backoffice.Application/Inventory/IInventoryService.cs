using Altairis.Backoffice.Contracts.Inventory;
using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Application.Inventory;

public interface IInventoryService
{
    Task<Result<InventoryCalendarResponse>> GetCalendarAsync(
        Guid hotelId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken);

    Task<Result> UpdateRangeAsync(Guid roomTypeId, UpdateInventoryRangeRequest request, CancellationToken cancellationToken);
}
