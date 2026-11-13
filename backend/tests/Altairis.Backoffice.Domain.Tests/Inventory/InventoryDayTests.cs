using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Inventory;

namespace Altairis.Backoffice.Domain.Tests.Inventory;

public class InventoryDayTests
{
    [Fact]
    public void Create_WithValidData_StartsWithNothingBooked()
    {
        Result<InventoryDay> result = InventoryTestData.Create(totalRooms: 5, price: 100m);

        Assert.True(result.IsSuccess);
        InventoryDay day = result.Value;
        Assert.Equal(5, day.TotalRooms);
        Assert.Equal(0, day.BookedRooms);
        Assert.Equal(5, day.Available);
        Assert.Equal(100m, day.Price);
    }

    [Fact]
    public void Create_WithNegativeTotalRooms_ReturnsTotalRoomsNegative()
    {
        Result<InventoryDay> result = InventoryTestData.Create(totalRooms: -1);

        Assert.Equal(InventoryDayErrors.TotalRoomsNegative, result.Error);
    }

    [Fact]
    public void Create_WithZeroTotalRooms_Succeeds()
    {
        Result<InventoryDay> result = InventoryTestData.Create(totalRooms: 0);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositivePrice_ReturnsPriceNotPositive(int price)
    {
        Result<InventoryDay> result = InventoryTestData.Create(price: price);

        Assert.Equal(InventoryDayErrors.PriceNotPositive, result.Error);
    }

    [Fact]
    public void Reserve_ThenRelease_UpdatesAvailable()
    {
        InventoryDay day = InventoryTestData.CreateValid(totalRooms: 5);

        Result reserved = day.Reserve(3);
        int availableAfterReserve = day.Available;
        day.Release(2);

        Assert.True(reserved.IsSuccess);
        Assert.Equal(2, availableAfterReserve);
        Assert.Equal(1, day.BookedRooms);
        Assert.Equal(4, day.Available);
    }

    [Fact]
    public void Reserve_OnSalesClosedNight_ReturnsSalesClosed()
    {
        InventoryDay day = InventoryTestData.Create(salesClosed: true).Value;

        Result result = day.Reserve(1);

        Assert.Equal(InventoryDayErrors.SalesClosed(day.Date), result.Error);
        Assert.Equal(0, day.BookedRooms);
    }

    [Fact]
    public void Reserve_MoreThanAvailable_ReturnsNotEnoughRooms()
    {
        InventoryDay day = InventoryTestData.CreateValid(totalRooms: 5);
        day.Reserve(3);

        Result result = day.Reserve(3);

        Assert.Equal(InventoryDayErrors.NotEnoughRooms(day.Date, 2), result.Error);
        Assert.Equal(3, day.BookedRooms);
    }

    [Fact]
    public void Reserve_ExactlyAvailable_Succeeds()
    {
        InventoryDay day = InventoryTestData.CreateValid(totalRooms: 5);

        Result result = day.Reserve(5);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, day.Available);
    }

    [Fact]
    public void Update_WithValidData_ChangesValues()
    {
        InventoryDay day = InventoryTestData.CreateValid();

        Result result = day.Update(8, 150m, salesClosed: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(8, day.TotalRooms);
        Assert.Equal(150m, day.Price);
        Assert.True(day.SalesClosed);
    }

    [Fact]
    public void Update_WithNonPositivePrice_ReturnsPriceNotPositive()
    {
        InventoryDay day = InventoryTestData.CreateValid();

        Result result = day.Update(5, 0m, salesClosed: false);

        Assert.Equal(InventoryDayErrors.PriceNotPositive, result.Error);
        Assert.Equal(100m, day.Price);
    }

    [Fact]
    public void Update_BelowBookedRooms_ReturnsTotalRoomsBelowBooked()
    {
        InventoryDay day = InventoryTestData.CreateValid(totalRooms: 5);
        day.Reserve(3);

        Result result = day.Update(2, 100m, salesClosed: false);

        Assert.Equal(InventoryDayErrors.TotalRoomsBelowBooked(day.Date, 3), result.Error);
        Assert.Equal(5, day.TotalRooms);
    }

    [Fact]
    public void Update_ToExactlyBookedRooms_Succeeds()
    {
        InventoryDay day = InventoryTestData.CreateValid(totalRooms: 5);
        day.Reserve(3);

        Result result = day.Update(3, 100m, salesClosed: false);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, day.Available);
    }

    [Fact]
    public void Update_ToSalesClosedWithRoomsBooked_KeepsExistingBookings()
    {
        InventoryDay day = InventoryTestData.CreateValid(totalRooms: 5);
        day.Reserve(2);

        Result result = day.Update(5, 100m, salesClosed: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, day.BookedRooms);
    }
}
