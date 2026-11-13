using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Application.Inventory;

public static class InventoryErrors
{
    public static readonly Error RangeInverted = Error.Validation(
        "inventory.range_inverted",
        "La fecha final no puede ser anterior a la inicial.");

    public static Error RangeTooLong(int maxDays) => Error.Validation(
        "inventory.range_too_long",
        $"El rango no puede superar {maxDays} días.");

    public static readonly Error PastNights = Error.Validation(
        "inventory.past_nights",
        "No se puede modificar el inventario de noches pasadas.");

    public static Error NewNightIncomplete(DateOnly date) => Error.Validation(
        "inventory.new_night_incomplete",
        $"El {DisplayFormat.Date(date)} aún no tiene inventario: indica el cupo y el precio para crearlo.");

    public static readonly Error ConcurrentUpdate = Error.Conflict(
        "inventory.concurrent_update",
        "El inventario ha cambiado mientras se guardaba (por ejemplo, por una reserva). Vuelve a intentarlo.");
}
