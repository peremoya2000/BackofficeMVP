using System.Diagnostics;
using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Domain.Inventory;

public sealed class InventoryDay
{
    private InventoryDay(Guid roomTypeId, DateOnly date, int totalRooms, int bookedRooms, decimal price, bool salesClosed)
    {
        RoomTypeId = roomTypeId;
        Date = date;
        TotalRooms = totalRooms;
        BookedRooms = bookedRooms;
        Price = price;
        SalesClosed = salesClosed;
    }

    public Guid RoomTypeId { get; private set; }

    public DateOnly Date { get; private set; }

    public int TotalRooms { get; private set; }

    public int BookedRooms { get; private set; }

    public decimal Price { get; private set; }

    public bool SalesClosed { get; private set; }

    public int Available => TotalRooms - BookedRooms;

    public static Result<InventoryDay> Create(Guid roomTypeId, DateOnly date, int totalRooms, decimal price, bool salesClosed)
    {
        Result validation = ValidateValues(totalRooms, price);
        if (validation.IsFailure) return validation.Error;

        return new InventoryDay(roomTypeId, date, totalRooms, 0, price, salesClosed);
    }

    public Result Update(int totalRooms, decimal price, bool salesClosed)
    {
        Result validation = ValidateValues(totalRooms, price);
        if (validation.IsFailure) return validation;
        if (totalRooms < BookedRooms) return InventoryDayErrors.TotalRoomsBelowBooked(Date, BookedRooms);

        TotalRooms = totalRooms;
        Price = price;
        SalesClosed = salesClosed;
        return Result.Success();
    }

    public Result CanReserve(int rooms)
    {
        Debug.Assert(rooms > 0, "Reserving inventory needs a positive number of rooms.");

        if (SalesClosed) return InventoryDayErrors.SalesClosed(Date);
        if (rooms > Available) return InventoryDayErrors.NotEnoughRooms(Date, Available);

        return Result.Success();
    }

    public Result Reserve(int rooms)
    {
        Result check = CanReserve(rooms);
        if (check.IsFailure) return check;

        BookedRooms += rooms;
        return Result.Success();
    }

    public void Release(int rooms)
    {
        Debug.Assert(rooms > 0 && rooms <= BookedRooms, $"Cannot release {rooms} rooms on {Date} when {BookedRooms} are booked.");

        BookedRooms -= rooms;
    }

    private static Result ValidateValues(int totalRooms, decimal price)
    {
        if (totalRooms < 0) return InventoryDayErrors.TotalRoomsNegative;
        if (price <= 0) return InventoryDayErrors.PriceNotPositive;

        return Result.Success();
    }
}
