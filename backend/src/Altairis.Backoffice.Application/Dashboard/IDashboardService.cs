using Altairis.Backoffice.Contracts.Dashboard;
using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(DateOnly date, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<OccupancyDayResponse>>> GetOccupancyAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);
}
