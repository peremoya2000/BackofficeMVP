using Altairis.Backoffice.Application.Common;
using Altairis.Backoffice.Contracts.Inventory;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;
using Altairis.Backoffice.Domain.Inventory;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Backoffice.Application.Inventory;

internal sealed class InventoryService(IBackofficeDbContext dbContext, TimeProvider timeProvider) : IInventoryService
{
    public const int MAX_CALENDAR_DAYS = 62;
    public const int MAX_UPDATE_NIGHTS = 366;

    public async Task<Result<InventoryCalendarResponse>> GetCalendarAsync(
        Guid hotelId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        Result range = ValidateRange(from, to, MAX_CALENDAR_DAYS);
        if (range.IsFailure) return range.Error;

        InventoryCalendarResponse? calendar = await dbContext.Hotels
            .AsNoTracking()
            .Where(hotel => hotel.Id == hotelId)
            .Select(hotel => new InventoryCalendarResponse(
                hotel.Id,
                hotel.Name,
                hotel.Currency,
                from,
                to,
                hotel.RoomTypes
                    .OrderBy(roomType => roomType.Name)
                    .Select(roomType => new RoomTypeInventoryResponse(
                        roomType.Id,
                        roomType.Name,
                        dbContext.InventoryDays
                            .Where(day => day.RoomTypeId == roomType.Id && day.Date >= from && day.Date <= to)
                            .OrderBy(day => day.Date)
                            .Select(day => new InventoryDayResponse(day.Date, day.TotalRooms, day.BookedRooms, day.Price, day.SalesClosed))
                            .ToList()))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return calendar is null ? HotelErrors.NotFound(hotelId) : calendar;
    }

    public async Task<Result> UpdateRangeAsync(
        Guid roomTypeId,
        UpdateInventoryRangeRequest request,
        CancellationToken cancellationToken)
    {
        Result range = ValidateRange(request.From, request.To, MAX_UPDATE_NIGHTS);
        if (range.IsFailure) return range;
        if (request.From < OperationsCalendar.Today(timeProvider)) return InventoryErrors.PastNights;

        bool roomTypeExists = await dbContext.RoomTypes.AnyAsync(roomType => roomType.Id == roomTypeId, cancellationToken);
        if (!roomTypeExists) return RoomTypeErrors.NotFound(roomTypeId);

        Dictionary<DateOnly, InventoryDay> existingNights = await dbContext.InventoryDays
            .Where(day => day.RoomTypeId == roomTypeId && day.Date >= request.From && day.Date <= request.To)
            .ToDictionaryAsync(day => day.Date, cancellationToken);

        for (DateOnly date = request.From; date <= request.To; date = date.AddDays(1))
        {
            Result applied = existingNights.TryGetValue(date, out InventoryDay? night)
                ? night.Update(
                    request.TotalRooms ?? night.TotalRooms,
                    request.Price ?? night.Price,
                    request.SalesClosed ?? night.SalesClosed)
                : AddNight(roomTypeId, date, request);
            if (applied.IsFailure) return applied;
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return InventoryErrors.ConcurrentUpdate;
        }

        return Result.Success();
    }

    private static Result ValidateRange(DateOnly from, DateOnly to, int maxDays)
    {
        if (to < from) return InventoryErrors.RangeInverted;

        int days = to.DayNumber - from.DayNumber + 1;
        if (days > maxDays) return InventoryErrors.RangeTooLong(maxDays);

        return Result.Success();
    }

    private Result AddNight(Guid roomTypeId, DateOnly date, UpdateInventoryRangeRequest request)
    {
        if (request.TotalRooms is not int totalRooms || request.Price is not decimal price)
        {
            return InventoryErrors.NewNightIncomplete(date);
        }

        Result<InventoryDay> created = InventoryDay.Create(roomTypeId, date, totalRooms, price, request.SalesClosed ?? false);
        if (created.IsFailure) return created;

        dbContext.InventoryDays.Add(created.Value);
        return Result.Success();
    }
}
