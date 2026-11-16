using Altairis.Backoffice.Application.Common;
using Altairis.Backoffice.Contracts.RoomTypes;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Altairis.Backoffice.Application.RoomTypes;

internal sealed class RoomTypeService(IBackofficeDbContext dbContext) : IRoomTypeService
{
    private const string NAME_INDEX_NAME = "ix_room_types_hotel_id_lower_name";

    public async Task<Result<IReadOnlyList<RoomTypeResponse>>> ListAsync(Guid hotelId, CancellationToken cancellationToken)
    {
        bool hotelExists = await dbContext.Hotels.AnyAsync(hotel => hotel.Id == hotelId, cancellationToken);
        if (!hotelExists) return HotelErrors.NotFound(hotelId);

        List<RoomTypeResponse> roomTypes = await dbContext.RoomTypes.AsNoTracking()
            .Where(roomType => roomType.HotelId == hotelId)
            .OrderBy(roomType => roomType.Name)
            .Select(roomType => new RoomTypeResponse(
                roomType.Id,
                roomType.HotelId,
                roomType.Name,
                roomType.Category.ToString(),
                roomType.MaxOccupancy))
            .ToListAsync(cancellationToken);

        return roomTypes;
    }

    public async Task<Result<RoomTypeResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        RoomType? roomType = await dbContext.RoomTypes.FindAsync([id], cancellationToken);
        if (roomType is null) return RoomTypeErrors.NotFound(id);

        return roomType.ToResponse();
    }

    public async Task<Result<RoomTypeResponse>> CreateAsync(Guid hotelId, CreateRoomTypeRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(request.Category, true, out RoomCategory category)) return RoomTypeErrors.CategoryInvalid;

        // The hotel checks for duplicate names against the room types it has loaded, so load them all.
        Hotel? hotel = await dbContext.Hotels
            .Include(hotel => hotel.RoomTypes)
            .SingleOrDefaultAsync(hotel => hotel.Id == hotelId, cancellationToken);
        if (hotel is null) return HotelErrors.NotFound(hotelId);

        Result<RoomType> added = hotel.AddRoomType(request.Name, category, request.MaxOccupancy);
        if (added.IsFailure) return added.Error;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        // A concurrent request can add the same name after this hotel's room types were loaded.
        catch (DbUpdateException exception) when (IsDuplicateName(exception))
        {
            return RoomTypeErrors.DuplicateName(added.Value.Name);
        }

        return added.Value.ToResponse();
    }

    private static bool IsDuplicateName(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: NAME_INDEX_NAME };
}
