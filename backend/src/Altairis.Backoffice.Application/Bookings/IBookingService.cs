using Altairis.Backoffice.Contracts.Bookings;
using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Application.Bookings;

public interface IBookingService
{
    Task<Result<QuoteResponse>> QuoteAsync(
        Guid roomTypeId,
        DateOnly checkIn,
        DateOnly checkOut,
        int rooms,
        CancellationToken cancellationToken);

    Task<Result<BookingResponse>> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken);

    Task<Result<BookingResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken);
}
