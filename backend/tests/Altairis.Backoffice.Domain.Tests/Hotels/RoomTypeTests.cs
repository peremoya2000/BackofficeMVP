using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;

namespace Altairis.Backoffice.Domain.Tests.Hotels;

public class RoomTypeTests
{
    private readonly Hotel _hotel = HotelTestData.CreateValid();

    [Fact]
    public void Create_WithValidData_ReturnsTrimmedRoomType()
    {
        Result<RoomType> result = _hotel.AddRoomType("  Suite Junior Vista Mar ", RoomCategory.Suite, 3);

        Assert.True(result.IsSuccess);
        RoomType roomType = result.Value;
        Assert.Equal(7, roomType.Id.Version);
        Assert.Equal("Suite Junior Vista Mar", roomType.Name);
        Assert.Equal(RoomCategory.Suite, roomType.Category);
        Assert.Equal(3, roomType.MaxOccupancy);
    }

    [Fact]
    public void Create_WithBlankName_ReturnsNameRequired()
    {
        Result<RoomType> result = _hotel.AddRoomType(" ", RoomCategory.Double, 2);

        Assert.Equal(RoomTypeErrors.NameRequired, result.Error);
    }

    [Fact]
    public void Create_WithNameOverMaxLength_ReturnsNameTooLong()
    {
        Result<RoomType> result = _hotel.AddRoomType(new string('a', RoomType.NAME_MAX_LENGTH + 1), RoomCategory.Double, 2);

        Assert.Equal(RoomTypeErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Create_WithUndefinedCategory_ReturnsCategoryInvalid()
    {
        Result<RoomType> result = _hotel.AddRoomType("Doble", (RoomCategory)99, 2);

        Assert.Equal(RoomTypeErrors.CategoryInvalid, result.Error);
    }

    [Theory]
    [InlineData(RoomType.MIN_OCCUPANCY - 1)]
    [InlineData(RoomType.ABSOLUTE_MAX_OCCUPANCY + 1)]
    public void Create_WithOccupancyOutOfRange_ReturnsMaxOccupancyOutOfRange(int maxOccupancy)
    {
        Result<RoomType> result = _hotel.AddRoomType("Familiar", RoomCategory.Family, maxOccupancy);

        Assert.Equal(RoomTypeErrors.MaxOccupancyOutOfRange, result.Error);
    }

    [Fact]
    public void Create_WithOccupancyAtAbsoluteMax_Succeeds()
    {
        Result<RoomType> result = _hotel.AddRoomType("Villa", RoomCategory.Family, RoomType.ABSOLUTE_MAX_OCCUPANCY);

        Assert.True(result.IsSuccess);
    }
}
