using Altairis.Backoffice.Application.Common;
using Altairis.Backoffice.Application.Inventory;
using Altairis.Backoffice.Contracts.Bookings;
using Altairis.Backoffice.Contracts.Common;
using Altairis.Backoffice.Domain.Bookings;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;
using Altairis.Backoffice.Domain.Inventory;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Altairis.Backoffice.Application.Bookings;

internal sealed class BookingService(IBackofficeDbContext dbContext, TimeProvider timeProvider) : IBookingService
{
    private const int MAX_SAVE_ATTEMPTS = 3;
    private const string REFERENCE_INDEX_NAME = "ix_bookings_reference";

    public async Task<Result<PagedResponse<BookingListItemResponse>>> SearchAsync(BookingSearchRequest request, CancellationToken cancellationToken)
    {
        IQueryable<Booking> bookings = dbContext.Bookings.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string pattern = LikePattern.Containing(request.Search.Trim());
            bookings = bookings.Where(booking =>
                EF.Functions.ILike(booking.Reference, pattern, LikePattern.ESCAPE_CHARACTER)
                || EF.Functions.ILike(SearchText.Unaccent(booking.LeadGuestName), SearchText.Unaccent(pattern), LikePattern.ESCAPE_CHARACTER)
                || EF.Functions.ILike(SearchText.Unaccent(booking.ClientName), SearchText.Unaccent(pattern), LikePattern.ESCAPE_CHARACTER));
        }

        if (request.Status is not null)
        {
            if (!Enum.TryParse(request.Status, out BookingStatus status) || !Enum.IsDefined(status)) return BookingErrors.StatusInvalid;
            bookings = bookings.Where(booking => booking.Status == status);
        }

        if (request.CheckIn is DateOnly checkIn) bookings = bookings.Where(booking => booking.Stay.CheckIn == checkIn);
        if (request.CheckOut is DateOnly checkOut) bookings = bookings.Where(booking => booking.Stay.CheckOut == checkOut);

        IOrderedQueryable<Booking>? ordered = request.Sort switch
        {
            null or "-createdAtUtc" => bookings.OrderByDescending(booking => booking.CreatedAtUtc),
            "createdAtUtc" => bookings.OrderBy(booking => booking.CreatedAtUtc),
            "reference" => bookings.OrderBy(booking => booking.Reference),
            "-reference" => bookings.OrderByDescending(booking => booking.Reference),
            "checkIn" => bookings.OrderBy(booking => booking.Stay.CheckIn),
            "-checkIn" => bookings.OrderByDescending(booking => booking.Stay.CheckIn),
            "checkOut" => bookings.OrderBy(booking => booking.Stay.CheckOut),
            "-checkOut" => bookings.OrderByDescending(booking => booking.Stay.CheckOut),
            _ => null,
        };
        if (ordered is null) return RequestErrors.SortInvalid(request.Sort);

        return await (
            from booking in ordered.ThenBy(booking => booking.Id)
            join hotel in dbContext.Hotels on booking.HotelId equals hotel.Id
            select new BookingListItemResponse(
                booking.Id,
                booking.Reference,
                booking.Status.ToString(),
                hotel.Id,
                hotel.Name,
                booking.Stay.CheckIn,
                booking.Stay.CheckOut,
                booking.LeadGuestName,
                booking.ClientName,
                booking.CreatedAtUtc))
            .ToPagedResponseAsync(request, cancellationToken);
    }

    public async Task<Result<QuoteResponse>> QuoteAsync(
        Guid roomTypeId,
        DateOnly checkIn,
        DateOnly checkOut,
        int rooms,
        CancellationToken cancellationToken)
    {
        Result<StayInventory> stay = await LoadStayInventoryAsync(roomTypeId, checkIn, checkOut, cancellationToken);
        if (stay.IsFailure) return stay.Error;

        Result<Money> total = Booking.Quote(
            roomTypeId,
            stay.Value.Stay,
            rooms,
            stay.Value.Nights,
            stay.Value.Currency,
            OperationsCalendar.Today(timeProvider));
        if (total.IsFailure) return total.Error;

        return new QuoteResponse(total.Value.Amount, total.Value.Currency);
    }

    public async Task<Result<BookingResponse>> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(request.Channel, out BookingChannel channel)) return BookingErrors.ChannelInvalid;

        Result<StayInventory> stay = await LoadStayInventoryAsync(request.RoomTypeId, request.CheckIn, request.CheckOut, cancellationToken);
        if (stay.IsFailure) return stay.Error;

        DateTime nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        Result<Booking> created = Booking.Create(
            stay.Value.RoomType,
            stay.Value.Stay,
            request.Rooms,
            request.Adults,
            request.Children,
            request.LeadGuestName,
            request.Notes,
            channel,
            request.ClientName,
            request.ClientReference,
            stay.Value.Nights,
            stay.Value.Currency,
            new Money(request.QuotedTotal, stay.Value.Currency),
            OperationsCalendar.DateOf(nowUtc),
            nowUtc);
        if (created.IsFailure) return created.Error;

        dbContext.Bookings.Add(created.Value);
        Result saved = await SaveNewBookingAsync(created.Value, cancellationToken);
        if (saved.IsFailure) return saved.Error;

        return await GetByIdAsync(created.Value.Id, cancellationToken);
    }

    public async Task<Result<BookingResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        BookingResponse? response = await (
            from booking in dbContext.Bookings.AsNoTracking()
            join hotel in dbContext.Hotels on booking.HotelId equals hotel.Id
            join roomType in dbContext.RoomTypes on booking.RoomTypeId equals roomType.Id
            where booking.Id == id
            select new BookingResponse(
                booking.Id,
                booking.Reference,
                booking.Status.ToString(),
                hotel.Id,
                hotel.Name,
                roomType.Id,
                roomType.Name,
                booking.Stay.CheckIn,
                booking.Stay.CheckOut,
                booking.Rooms,
                booking.Adults,
                booking.Children,
                booking.LeadGuestName,
                booking.Channel.ToString(),
                booking.ClientName,
                booking.ClientReference,
                booking.Notes,
                booking.TotalPrice.Amount,
                booking.TotalPrice.Currency,
                booking.CreatedAtUtc,
                booking.CancelledAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        return response is null ? BookingErrors.NotFound(id) : response;
    }

    public async Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        Booking? booking = await dbContext.Bookings.FindAsync([id], cancellationToken);
        if (booking is null) return BookingErrors.NotFound(id);

        List<InventoryDay> nights = await LoadNightsAsync(booking.RoomTypeId, booking.Stay, cancellationToken);
        DateTime nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        Result cancelled = booking.Cancel(nights, OperationsCalendar.DateOf(nowUtc), nowUtc);
        if (cancelled.IsFailure) return cancelled;

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

    private async Task<Result<StayInventory>> LoadStayInventoryAsync(
        Guid roomTypeId,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken)
    {
        Result<StayPeriod> stay = StayPeriod.Create(checkIn, checkOut);
        if (stay.IsFailure) return stay.Error;

        RoomType? roomType = await dbContext.RoomTypes.FindAsync([roomTypeId], cancellationToken);
        if (roomType is null) return RoomTypeErrors.NotFound(roomTypeId);

        string currency = await dbContext.Hotels
            .Where(hotel => hotel.Id == roomType.HotelId)
            .Select(hotel => hotel.Currency)
            .SingleAsync(cancellationToken);
        List<InventoryDay> nights = await LoadNightsAsync(roomTypeId, stay.Value, cancellationToken);

        return new StayInventory(roomType, stay.Value, currency, nights);
    }

    private Task<List<InventoryDay>> LoadNightsAsync(Guid roomTypeId, StayPeriod stay, CancellationToken cancellationToken) =>
        dbContext.InventoryDays
            .Where(day => day.RoomTypeId == roomTypeId && day.Date >= stay.CheckIn && day.Date < stay.CheckOut)
            .ToListAsync(cancellationToken);

    private async Task<Result> SaveNewBookingAsync(Booking booking, CancellationToken cancellationToken)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                return InventoryErrors.ConcurrentUpdate;
            }
            // References are random, so one can clash with an existing booking's: draw another and save again.
            catch (DbUpdateException exception) when (attempt < MAX_SAVE_ATTEMPTS && IsDuplicateReference(exception))
            {
                booking.RenewReference();
            }
        }
    }

    private static bool IsDuplicateReference(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: REFERENCE_INDEX_NAME };

    private sealed record StayInventory(RoomType RoomType, StayPeriod Stay, string Currency, List<InventoryDay> Nights);
}
