using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Domain.Inventory;

public static class InventoryDayErrors
{
    public static readonly Error TotalRoomsNegative = Error.Validation(
        "inventory_day.total_rooms_negative",
        "El cupo no puede ser negativo.");

    public static readonly Error PriceNotPositive = Error.Validation(
        "inventory_day.price_not_positive",
        "El precio debe ser mayor que cero.");

    public static Error TotalRoomsBelowBooked(DateOnly date, int bookedRooms) => Error.Conflict(
        "inventory_day.total_rooms_below_booked",
        $"El {DisplayFormat.Date(date)} ya hay {bookedRooms} habitaciones reservadas; el cupo no puede ser menor.");

    public static Error SalesClosed(DateOnly date) => Error.Conflict(
        "inventory_day.sales_closed",
        $"La venta está cerrada el {DisplayFormat.Date(date)}.");

    public static Error NotEnoughRooms(DateOnly date, int available) => Error.Conflict(
        "inventory_day.not_enough_rooms",
        $"El {DisplayFormat.Date(date)} no hay suficientes habitaciones disponibles (quedan {available}).");
}
