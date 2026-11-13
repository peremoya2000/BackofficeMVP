using Altairis.Backoffice.Application.Hotels;
using Altairis.Backoffice.Contracts.Common;
using Altairis.Backoffice.Contracts.Hotels;
using Altairis.Backoffice.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Altairis.Backoffice.Api.Controllers;

[Route("api/hotels")]
public sealed class HotelsController(IHotelService hotelService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<HotelListItemResponse>>> Search(
        [FromQuery] HotelSearchRequest request,
        CancellationToken cancellationToken)
    {
        Result<PagedResponse<HotelListItemResponse>> result = await hotelService.SearchAsync(request, cancellationToken);
        return OkOrProblem(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HotelResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        Result<HotelResponse> result = await hotelService.GetByIdAsync(id, cancellationToken);
        return OkOrProblem(result);
    }

    [HttpPost]
    [ProducesResponseType<HotelResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HotelResponse>> Create(CreateHotelRequest request, CancellationToken cancellationToken)
    {
        Result<HotelResponse> result = await hotelService.CreateAsync(request, cancellationToken);
        if (result.IsFailure) return ProblemFor(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }
}
