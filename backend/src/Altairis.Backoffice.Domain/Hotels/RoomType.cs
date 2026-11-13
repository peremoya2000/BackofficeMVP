using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Domain.Hotels;

/// <summary>
/// A kind of room that one hotel sells (e.g. "Doble Superior Vista Mar"), grouped across hotels by <see cref="RoomCategory"/>.
/// How many of it can be sold, and at what price, is set per night in <see cref="Inventory.InventoryDay"/>.
/// </summary>
public sealed class RoomType
{
    public const int NAME_MAX_LENGTH = 100;
    public const int MIN_OCCUPANCY = 1;
    public const int ABSOLUTE_MAX_OCCUPANCY = 10;

    private RoomType(Guid id, Guid hotelId, string name, RoomCategory category, int maxOccupancy)
    {
        Id = id;
        HotelId = hotelId;
        Name = name;
        Category = category;
        MaxOccupancy = maxOccupancy;
    }

    public Guid Id { get; private set; }

    public Guid HotelId { get; private set; }

    public string Name { get; private set; }

    public RoomCategory Category { get; private set; }

    public int MaxOccupancy { get; private set; }

    internal static Result<RoomType> Create(Guid hotelId, string name, RoomCategory category, int maxOccupancy)
    {
        string trimmedName = name.Trim();

        if (trimmedName.Length == 0) return RoomTypeErrors.NameRequired;
        if (trimmedName.Length > NAME_MAX_LENGTH) return RoomTypeErrors.NameTooLong;
        if (!Enum.IsDefined(category)) return RoomTypeErrors.CategoryInvalid;
        if (maxOccupancy < MIN_OCCUPANCY || maxOccupancy > ABSOLUTE_MAX_OCCUPANCY) return RoomTypeErrors.MaxOccupancyOutOfRange;

        return new RoomType(Guid.CreateVersion7(), hotelId, trimmedName, category, maxOccupancy);
    }

    internal bool HasName(string name) => string.Equals(Name, name, StringComparison.OrdinalIgnoreCase);
}
