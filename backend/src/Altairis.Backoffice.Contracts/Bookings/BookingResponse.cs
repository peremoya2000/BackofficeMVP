namespace Altairis.Backoffice.Contracts.Bookings;

public sealed record BookingResponse(
    Guid Id,
    string Reference,
    string Status,
    Guid HotelId,
    string HotelName,
    Guid RoomTypeId,
    string RoomTypeName,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Rooms,
    int Adults,
    int Children,
    string LeadGuestName,
    string Channel,
    string ClientName,
    string? ClientReference,
    string? Notes,
    decimal TotalPrice,
    string Currency,
    DateTime CreatedAtUtc,
    DateTime? CancelledAtUtc);
