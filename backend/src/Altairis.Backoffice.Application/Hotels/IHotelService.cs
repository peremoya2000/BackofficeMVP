using Altairis.Backoffice.Contracts.Common;
using Altairis.Backoffice.Contracts.Hotels;
using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Application.Hotels;

public interface IHotelService
{
    Task<Result<PagedResponse<HotelListItemResponse>>> SearchAsync(HotelSearchRequest request, CancellationToken cancellationToken);

    Task<Result<HotelResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<HotelResponse>> CreateAsync(CreateHotelRequest request, CancellationToken cancellationToken);
}
