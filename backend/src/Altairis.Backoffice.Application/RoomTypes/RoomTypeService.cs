using Altairis.Backoffice.Application.Common;
using Altairis.Backoffice.Contracts.RoomTypes;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;
using Microsoft.EntityFrameworkCore;


namespace Altairis.Backoffice.Application.RoomTypes;

internal sealed class RoomTypeService(IBackofficeDbContext dbContext) : IRoomTypeService
{
    public async Task<Result<IReadOnlyList<RoomTypeResponse>>> ListAsync(Guid hotelId, CancellationToken cancellationToken)
    {
        bool hotelExists = await dbContext.Hotels.AnyAsync(hotel => hotel.Id == hotelId, cancellationToken);
        if (!hotelExists) return HotelErrors.NotFound(hotelId);

        List<RoomTypeResponse> roomTypes = await dbContext.RoomTypes.AsNoTracking()
            .Where(rt => rt.HotelId == hotelId)
            .OrderBy(rt => rt.Name)
            .Select(rt => new RoomTypeResponse(rt.Id, rt.HotelId, rt.Name, rt.Category.ToString(), rt.MaxOccupancy))
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
        if (!Enum.TryParse<RoomCategory>(request.Category, true, out var category)) return RoomTypeErrors.CategoryInvalid;

        //The hotel checks for duplicate names against the room types it has loaded, so load them all.
        Hotel? hotel = await dbContext.Hotels
            .Include(h => h.RoomTypes)
            .SingleOrDefaultAsync(h => h.Id == hotelId, cancellationToken);
        if (hotel is null) return HotelErrors.NotFound(hotelId);

        Result<RoomType> added = hotel.AddRoomType(request.Name, category, request.MaxOccupancy);
        if (added.IsFailure) return added.Error;

        await dbContext.SaveChangesAsync(cancellationToken);

        return added.Value.ToResponse();
    }
}
