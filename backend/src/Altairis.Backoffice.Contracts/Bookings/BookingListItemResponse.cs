namespace Altairis.Backoffice.Contracts.Bookings;

public sealed record BookingListItemResponse(
    Guid Id,
    string Reference,
    string Status,
    Guid HotelId,
    string HotelName,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string LeadGuestName,
    string ClientName,
    DateTime CreatedAtUtc);