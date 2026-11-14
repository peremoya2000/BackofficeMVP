using Altairis.Backoffice.Application.Dashboard;
using Altairis.Backoffice.Contracts.Dashboard;
using Altairis.Backoffice.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Altairis.Backoffice.Api.Controllers;

[Route("api")]
public sealed class DashboardController(IDashboardService dashboardService) : ApiControllerBase
{
    [HttpGet("dashboard/summary")]
    [EndpointDescription("The day's operation: confirmed arrivals, departures and stays in progress, bookings made and cancelled that day, and rooms booked out of the allotment.")]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary(
        [FromQuery, BindRequired] DateOnly date,
        CancellationToken cancellationToken)
    {
        DashboardSummaryResponse summary = await dashboardService.GetSummaryAsync(date, cancellationToken);
        return Ok(summary);
    }

    [HttpGet("dashboard/occupancy")]
    [EndpointDescription("Rooms booked and the total allotment of every night from 'from' to 'to', both included, across all hotels.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<OccupancyDayResponse>>> GetOccupancy(
        [FromQuery, BindRequired] DateOnly from,
        [FromQuery, BindRequired] DateOnly to,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<OccupancyDayResponse>> result = await dashboardService.GetOccupancyAsync(from, to, cancellationToken);
        return OkOrProblem(result);
    }
}
