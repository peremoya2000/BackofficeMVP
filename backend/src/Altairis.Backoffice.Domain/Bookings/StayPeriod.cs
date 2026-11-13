using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Domain.Bookings;

public readonly record struct StayPeriod
{
    private StayPeriod(DateOnly checkIn, DateOnly checkOut)
    {
        CheckIn = checkIn;
        CheckOut = checkOut;
    }

    public DateOnly CheckIn { get; }

    public DateOnly CheckOut { get; }

    public int Nights => CheckOut.DayNumber - CheckIn.DayNumber;

    public static Result<StayPeriod> Create(DateOnly checkIn, DateOnly checkOut)
    {
        if (checkOut <= checkIn) return StayPeriodErrors.CheckOutNotAfterCheckIn;

        return new StayPeriod(checkIn, checkOut);
    }

    public IEnumerable<DateOnly> EachNight()
    {
        for (DateOnly night = CheckIn; night < CheckOut; night = night.AddDays(1))
        {
            yield return night;
        }
    }
}
