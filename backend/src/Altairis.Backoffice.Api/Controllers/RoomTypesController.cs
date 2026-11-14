using Altairis.Backoffice.Application.RoomTypes;
using Altairis.Backoffice.Contracts.RoomTypes;
using Altairis.Backoffice.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Altairis.Backoffice.Api.Controllers;

[Route("api")]
public sealed class RoomTypesController(IRoomTypeService roomTypeService) : ApiControllerBase
{
    [HttpGet("hotels/{hotelId:guid}/room-types")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RoomTypeResponse>>> List(Guid hotelId, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<RoomTypeResponse>> result = await roomTypeService.ListAsync(hotelId, cancellationToken);
        return OkOrProblem(result);
    }

    [HttpGet("room-types/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomTypeResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        Result<RoomTypeResponse> result = await roomTypeService.GetByIdAsync(id, cancellationToken);
        return OkOrProblem(result);
    }

    [HttpPost("hotels/{hotelId:guid}/room-types")]
    [ProducesResponseType<RoomTypeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomTypeResponse>> Create(
        Guid hotelId,
        CreateRoomTypeRequest request,
        CancellationToken cancellationToken)
    {
        Result<RoomTypeResponse> result = await roomTypeService.CreateAsync(hotelId, request, cancellationToken);
        if (result.IsFailure) return ProblemFor(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }
}