using System.ComponentModel;

namespace Altairis.Backoffice.Contracts.Bookings;

public sealed record CreateBookingRequest(
    Guid RoomTypeId,
    DateOnly CheckIn,
    [property: Description("The check-out date: its night is not booked.")] DateOnly CheckOut,
    int Rooms,
    int Adults,
    int Children,
    string LeadGuestName,
    [property: Description("Agency, TourOperator, Corporate or Direct.")] string Channel,
    string ClientName,
    [property: Description("The total of the quote the client accepted. If the price has changed since, the booking is refused with 409.")] decimal QuotedTotal,
    string? ClientReference = null,
    string? Notes = null);
