using Altairis.Backoffice.Application.Inventory;
using Altairis.Backoffice.Contracts.Inventory;
using Altairis.Backoffice.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Altairis.Backoffice.Api.Controllers;

[Route("api")]
public sealed class InventoryController(IInventoryService inventoryService) : ApiControllerBase
{
    [HttpGet("hotels/{hotelId:guid}/inventory")]
    [EndpointDescription("Every room type of the hotel with its nights from 'from' to 'to', both included.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryCalendarResponse>> GetCalendar(
        Guid hotelId,
        [FromQuery, BindRequired] DateOnly from,
        [FromQuery, BindRequired] DateOnly to,
        CancellationToken cancellationToken)
    {
        Result<InventoryCalendarResponse> result = await inventoryService.GetCalendarAsync(hotelId, from, to, cancellationToken);
        return OkOrProblem(result);
    }

    [HttpPut("room-types/{roomTypeId:guid}/inventory")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateRange(
        Guid roomTypeId,
        UpdateInventoryRangeRequest request,
        CancellationToken cancellationToken)
    {
        Result result = await inventoryService.UpdateRangeAsync(roomTypeId, request, cancellationToken);
        return result.IsSuccess ? NoContent() : ProblemFor(result.Error);
    }
}
