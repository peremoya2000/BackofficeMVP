using Altairis.Backoffice.Application.Common;
using Altairis.Backoffice.Contracts.Dashboard;
using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Backoffice.Application.Dashboard;

internal sealed class DashboardService(IBackofficeDbContext dbContext) : IDashboardService
{
    public const int MAX_OCCUPANCY_DAYS = 92;

    public async Task<DashboardSummaryResponse> GetSummaryAsync(DateOnly date, CancellationToken cancellationToken)
    {
        IQueryable<Booking> bookings = dbContext.Bookings.AsNoTracking();
        IQueryable<Booking> confirmed = bookings.Where(booking => booking.Status == BookingStatus.Confirmed);
        DateTime dayStartUtc = OperationsCalendar.StartOfDayUtc(date);
        DateTime nextDayStartUtc = OperationsCalendar.StartOfDayUtc(date.AddDays(1));

        int arrivals = await confirmed.CountAsync(booking => booking.Stay.CheckIn == date, cancellationToken);
        int departures = await confirmed.CountAsync(booking => booking.Stay.CheckOut == date, cancellationToken);
        int staysInProgress = await confirmed.CountAsync(
            booking => booking.Stay.CheckIn <= date && booking.Stay.CheckOut > date,
            cancellationToken);
        int newBookings = await bookings.CountAsync(
            booking => booking.CreatedAtUtc >= dayStartUtc && booking.CreatedAtUtc < nextDayStartUtc,
            cancellationToken);
        int cancellations = await bookings.CountAsync(
            booking => booking.CancelledAtUtc >= dayStartUtc && booking.CancelledAtUtc < nextDayStartUtc,
            cancellationToken);
        List<OccupancyDayResponse> occupancy = await LoadOccupancyAsync(date, date, cancellationToken);

        return new DashboardSummaryResponse(
            date,
            arrivals,
            departures,
            staysInProgress,
            newBookings,
            cancellations,
            occupancy[0].BookedRooms,
            occupancy[0].TotalRooms);
    }

    public async Task<Result<IReadOnlyList<OccupancyDayResponse>>> GetOccupancyAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        if (to < from) return DashboardErrors.RangeInverted;
        if (to.DayNumber - from.DayNumber + 1 > MAX_OCCUPANCY_DAYS) return DashboardErrors.RangeTooLong(MAX_OCCUPANCY_DAYS);

        return await LoadOccupancyAsync(from, to, cancellationToken);
    }

    private async Task<List<OccupancyDayResponse>> LoadOccupancyAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        Dictionary<DateOnly, OccupancyDayResponse> nightsWithInventory = await dbContext.InventoryDays
            .Where(day => day.Date >= from && day.Date <= to)
            .GroupBy(day => day.Date)
            .Select(nights => new OccupancyDayResponse(
                nights.Key,
                nights.Sum(night => night.BookedRooms),
                nights.Sum(night => night.TotalRooms)))
            .ToDictionaryAsync(night => night.Date, cancellationToken);

        List<OccupancyDayResponse> occupancy = [];
        for (DateOnly date = from; date <= to; date = date.AddDays(1))
        {
            occupancy.Add(nightsWithInventory.GetValueOrDefault(date) ?? new OccupancyDayResponse(date, 0, 0));
        }

        return occupancy;
    }
}
