using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;

namespace Altairis.Backoffice.Domain.Tests.Hotels;

public class HotelTests
{
    [Fact]
    public void Create_WithValidData_ReturnsNormalizedHotel()
    {
        Result<Hotel> result = HotelTestData.Create(
            name: "  Hotel Mar Azul  ",
            countryCode: " es ",
            contactEmail: " reservas@marazul.es ",
            contactPhone: "   ");

        Assert.True(result.IsSuccess);
        Hotel hotel = result.Value;
        Assert.Equal(7, hotel.Id.Version);
        Assert.Equal("Hotel Mar Azul", hotel.Name);
        Assert.Equal("ES", hotel.CountryCode);
        Assert.Equal("reservas@marazul.es", hotel.ContactEmail);
        Assert.Null(hotel.ContactPhone);
        Assert.Equal(Hotel.MVP_CURRENCY, hotel.Currency);
        Assert.Equal(HotelTestData.CreatedAtUtc, hotel.CreatedAtUtc);
        Assert.Empty(hotel.RoomTypes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_ReturnsNameRequired(string name)
    {
        Result<Hotel> result = HotelTestData.Create(name: name);

        Assert.Equal(HotelErrors.NameRequired, result.Error);
    }

    [Fact]
    public void Create_WithNameAtMaxLength_Succeeds()
    {
        Result<Hotel> result = HotelTestData.Create(name: new string('a', Hotel.NAME_MAX_LENGTH));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithNameOverMaxLength_ReturnsNameTooLong()
    {
        Result<Hotel> result = HotelTestData.Create(name: new string('a', Hotel.NAME_MAX_LENGTH + 1));

        Assert.Equal(HotelErrors.NameTooLong, result.Error);
    }

    [Theory]
    [InlineData(Hotel.MIN_STARS - 1)]
    [InlineData(Hotel.MAX_STARS + 1)]
    public void Create_WithStarsOutOfRange_ReturnsStarsOutOfRange(int stars)
    {
        Result<Hotel> result = HotelTestData.Create(stars: stars);

        Assert.Equal(HotelErrors.StarsOutOfRange, result.Error);
    }

    [Fact]
    public void Create_WithBlankAddress_ReturnsAddressRequired()
    {
        Result<Hotel> result = HotelTestData.Create(address: " ");

        Assert.Equal(HotelErrors.AddressRequired, result.Error);
    }

    [Fact]
    public void Create_WithBlankCity_ReturnsCityRequired()
    {
        Result<Hotel> result = HotelTestData.Create(city: " ");

        Assert.Equal(HotelErrors.CityRequired, result.Error);
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("ESP")]
    [InlineData("E")]
    [InlineData("")]
    public void Create_WithUnassignedCountryCode_ReturnsCountryCodeInvalid(string countryCode)
    {
        Result<Hotel> result = HotelTestData.Create(countryCode: countryCode);

        Assert.Equal(HotelErrors.CountryCodeInvalid, result.Error);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("Ana <ana@marazul.es>")]
    public void Create_WithInvalidEmail_ReturnsContactEmailInvalid(string contactEmail)
    {
        Result<Hotel> result = HotelTestData.Create(contactEmail: contactEmail);

        Assert.Equal(HotelErrors.ContactEmailInvalid, result.Error);
    }

    [Theory]
    [InlineData("971 12 AB 56")]
    [InlineData("12345")]
    public void Create_WithInvalidPhone_ReturnsContactPhoneInvalid(string contactPhone)
    {
        Result<Hotel> result = HotelTestData.Create(contactPhone: contactPhone);

        Assert.Equal(HotelErrors.ContactPhoneInvalid, result.Error);
    }

    [Fact]
    public void Create_WithFormattedPhone_Succeeds()
    {
        Result<Hotel> result = HotelTestData.Create(contactPhone: "+34 (971) 123-456");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AddRoomType_WithNewName_AddsIt()
    {
        Hotel hotel = HotelTestData.CreateValid();

        Result<RoomType> result = hotel.AddRoomType("Doble Superior", RoomCategory.Double, 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(hotel.Id, result.Value.HotelId);
        Assert.Same(result.Value, Assert.Single(hotel.RoomTypes));
    }

    [Theory]
    [InlineData("Doble Superior")]
    [InlineData("  doble SUPERIOR ")]
    public void AddRoomType_WithExistingName_ReturnsDuplicateNameConflict(string duplicateName)
    {
        Hotel hotel = HotelTestData.CreateValid();
        hotel.AddRoomType("Doble Superior", RoomCategory.Double, 2);

        Result<RoomType> result = hotel.AddRoomType(duplicateName, RoomCategory.Twin, 2);

        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("room_type.duplicate_name", result.Error.Code);
        Assert.Single(hotel.RoomTypes);
    }
}
