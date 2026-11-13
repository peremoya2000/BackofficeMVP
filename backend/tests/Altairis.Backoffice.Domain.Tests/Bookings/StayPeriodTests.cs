using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Domain.Tests.Bookings;

public class StayPeriodTests
{
    private static readonly DateOnly CheckInDate = new(2026, 11, 20);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithCheckOutNotAfterCheckIn_ReturnsCheckOutNotAfterCheckIn(int daysAfterCheckIn)
    {
        Result<StayPeriod> result = StayPeriod.Create(CheckInDate, CheckInDate.AddDays(daysAfterCheckIn));

        Assert.Equal(StayPeriodErrors.CheckOutNotAfterCheckIn, result.Error);
    }

    [Fact]
    public void Nights_ReturnsNumberOfNights()
    {
        StayPeriod stayPeriod = StayPeriod.Create(CheckInDate, CheckInDate.AddDays(3)).Value;

        Assert.Equal(3, stayPeriod.Nights);
    }

    [Fact]
    public void EachNight_ExcludesCheckOutDate()
    {
        StayPeriod stayPeriod = StayPeriod.Create(CheckInDate, CheckInDate.AddDays(2)).Value;
        DateOnly[] expected = [new(2026, 11, 20), new(2026, 11, 21)];

        Assert.Equal(expected, stayPeriod.EachNight());
    }
}
