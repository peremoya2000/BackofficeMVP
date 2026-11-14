using Altairis.Backoffice.Application.Bookings;
using Altairis.Backoffice.Contracts.Bookings;
using Altairis.Backoffice.Contracts.Common;
using Altairis.Backoffice.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Altairis.Backoffice.Api.Controllers;

[Route("api")]
public sealed class BookingsController(IBookingService bookingService) : ApiControllerBase
{
    [HttpGet("bookings")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<BookingListItemResponse>>> Search(
        [FromQuery] BookingSearchRequest request,
        CancellationToken cancellationToken)
    {
        Result<PagedResponse<BookingListItemResponse>> result = await bookingService.SearchAsync(request, cancellationToken);
        return OkOrProblem(result);
    }

    [HttpGet("room-types/{roomTypeId:guid}/quote")]
    [EndpointDescription("The total price of the stay, or why it can't be sold (400 or 409). Saves nothing.")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteResponse>> Quote(
        Guid roomTypeId,
        [FromQuery, BindRequired] DateOnly checkIn,
        [FromQuery, BindRequired] DateOnly checkOut,
        [FromQuery, BindRequired] int rooms,
        CancellationToken cancellationToken)
    {
        Result<QuoteResponse> result = await bookingService.QuoteAsync(roomTypeId, checkIn, checkOut, rooms, cancellationToken);
        return OkOrProblem(result);
    }

    [HttpPost("bookings")]
    [EndpointDescription("Checks the stay again, reserves its nights and saves the booking, all in one transaction.")]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        Result<BookingResponse> result = await bookingService.CreateAsync(request, cancellationToken);
        if (result.IsFailure) return ProblemFor(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("bookings/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        Result<BookingResponse> result = await bookingService.GetByIdAsync(id, cancellationToken);
        return OkOrProblem(result);
    }

    [HttpPost("bookings/{id:guid}/cancel")]
    [EndpointDescription("Cancels the booking and frees its rooms on every night. Only possible before check-in.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        Result result = await bookingService.CancelAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : ProblemFor(result.Error);
    }
}
