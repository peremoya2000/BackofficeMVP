using System.ComponentModel;
using Altairis.Backoffice.Contracts.Common;

namespace Altairis.Backoffice.Contracts.Bookings;

public sealed class BookingSearchRequest : PagedRequest
{
    [Description("Matches part of the reference, the lead guest's name or the client's name, ignoring case.")]
    public string? Search { get; init; }

    [Description("Confirmed or Cancelled.")]
    public string? Status { get; init; }

    [Description("Only bookings that arrive on this date.")]
    public DateOnly? CheckIn { get; init; }

    [Description("Only bookings that leave on this date.")]
    public DateOnly? CheckOut { get; init; }
}