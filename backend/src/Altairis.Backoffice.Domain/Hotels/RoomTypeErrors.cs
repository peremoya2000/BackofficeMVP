using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Domain.Hotels;

public static class RoomTypeErrors
{
    public static readonly Error NameRequired = Error.Validation(
        "room_type.name_required",
        "El nombre del tipo de habitación es obligatorio.");

    public static readonly Error NameTooLong = Error.Validation(
        "room_type.name_too_long",
        $"El nombre del tipo de habitación no puede superar {RoomType.NAME_MAX_LENGTH} caracteres.");

    public static readonly Error CategoryInvalid = Error.Validation(
        "room_type.category_invalid",
        "La categoría del tipo de habitación no es válida.");

    public static readonly Error MaxOccupancyOutOfRange = Error.Validation(
        "room_type.max_occupancy_out_of_range",
        $"La ocupación máxima debe estar entre {RoomType.MIN_OCCUPANCY} y {RoomType.ABSOLUTE_MAX_OCCUPANCY} personas.");

    public static Error DuplicateName(string name) => Error.Conflict(
        "room_type.duplicate_name",
        $"El hotel ya tiene un tipo de habitación llamado «{name}».");
}
