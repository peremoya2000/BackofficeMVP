using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Inventory;

namespace Altairis.Backoffice.Domain.Tests.Inventory;

internal static class InventoryTestData
{
    public static readonly Guid RoomTypeId = Guid.CreateVersion7();
    public static readonly DateOnly Date = new(2026, 11, 20);

    public static Result<InventoryDay> Create(int totalRooms = 5, decimal price = 100m, bool salesClosed = false)
    {
        return InventoryDay.Create(RoomTypeId, Date, totalRooms, price, salesClosed);
    }

    public static InventoryDay CreateValid(int totalRooms = 5) => Create(totalRooms: totalRooms).Value;

    public static List<InventoryDay> ForStay(Guid roomTypeId, StayPeriod stay, int totalRooms = 5, decimal price = 100m)
    {
        return stay.EachNight()
            .Select(night => InventoryDay.Create(roomTypeId, night, totalRooms, price, salesClosed: false).Value)
            .ToList();
    }
}
