using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Domain.Bookings;

public static class BookingErrors
{
    public static Error NotFound(Guid bookingId) => Error.NotFound(
        "booking.not_found",
        $"No existe ninguna reserva con el id {bookingId}.");

    public static readonly Error RoomsInvalid = Error.Validation(
        "booking.rooms_invalid",
        "La reserva debe incluir al menos una habitación.");

    public static readonly Error TooManyNights = Error.Validation(
        "booking.too_many_nights",
        $"Una reserva no puede superar {Booking.MAX_NIGHTS} noches.");

    public static readonly Error CheckInInPast = Error.Validation(
        "booking.check_in_in_past",
        "La fecha de entrada no puede ser anterior a hoy.");

    public static readonly Error LeadGuestNameRequired = Error.Validation(
        "booking.lead_guest_name_required",
        "El nombre del huésped principal es obligatorio.");

    public static readonly Error LeadGuestNameTooLong = Error.Validation(
        "booking.lead_guest_name_too_long",
        $"El nombre del huésped principal no puede superar {Booking.LEAD_GUEST_NAME_MAX_LENGTH} caracteres.");

    public static readonly Error NotesTooLong = Error.Validation(
        "booking.notes_too_long",
        $"Las observaciones no pueden superar {Booking.NOTES_MAX_LENGTH} caracteres.");

    public static readonly Error ChannelInvalid = Error.Validation(
        "booking.channel_invalid",
        "El canal de venta no es válido.");

    public static readonly Error ClientNameRequired = Error.Validation(
        "booking.client_name_required",
        "El nombre del cliente es obligatorio.");

    public static readonly Error ClientNameTooLong = Error.Validation(
        "booking.client_name_too_long",
        $"El nombre del cliente no puede superar {Booking.CLIENT_NAME_MAX_LENGTH} caracteres.");

    public static readonly Error ClientReferenceTooLong = Error.Validation(
        "booking.client_reference_too_long",
        $"La referencia del cliente no puede superar {Booking.CLIENT_REFERENCE_MAX_LENGTH} caracteres.");

    public static readonly Error ChildrenNegative = Error.Validation(
        "booking.children_negative",
        "El número de niños no puede ser negativo.");

    public static readonly Error NotEnoughAdults = Error.Validation(
        "booking.not_enough_adults",
        "Cada habitación necesita al menos un adulto.");

    public static readonly Error PriceChanged = Error.Conflict(
        "booking.price_changed",
        "El precio ha cambiado desde la cotización. Vuelve a cotizar antes de confirmar.");

    public static readonly Error AlreadyCancelled = Error.Conflict(
        "booking.already_cancelled",
        "La reserva ya está cancelada.");

    public static readonly Error CheckInPassed = Error.Conflict(
        "booking.check_in_passed",
        "No se puede cancelar una reserva cuya fecha de entrada ya ha pasado.");

    public static Error NoInventory(DateOnly date) => Error.Conflict(
        "booking.no_inventory",
        $"No hay cupo cargado para el {DisplayFormat.Date(date)}.");

    public static Error TooManyGuests(int rooms, int maxGuests) => Error.Validation(
        "booking.too_many_guests",
        $"En {rooms} habitaciones caben como máximo {maxGuests} personas.");
}
