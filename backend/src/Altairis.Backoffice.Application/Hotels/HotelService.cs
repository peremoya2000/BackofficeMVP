using Altairis.Backoffice.Application.Common;
using Altairis.Backoffice.Contracts.Common;
using Altairis.Backoffice.Contracts.Hotels;
using Altairis.Backoffice.Domain.Common;
using Altairis.Backoffice.Domain.Hotels;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Backoffice.Application.Hotels;

internal sealed class HotelService(IBackofficeDbContext dbContext, TimeProvider timeProvider) : IHotelService
{
    public async Task<Result<PagedResponse<HotelListItemResponse>>> SearchAsync(
        HotelSearchRequest request,
        CancellationToken cancellationToken)
    {
        IQueryable<Hotel> hotels = dbContext.Hotels.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string pattern = LikePattern.Containing(request.Search.Trim());
            hotels = hotels.Where(hotel =>
                EF.Functions.ILike(SearchText.Unaccent(hotel.Name), SearchText.Unaccent(pattern), LikePattern.ESCAPE_CHARACTER)
                || EF.Functions.ILike(SearchText.Unaccent(hotel.City), SearchText.Unaccent(pattern), LikePattern.ESCAPE_CHARACTER));
        }

        if (!string.IsNullOrWhiteSpace(request.CountryCode))
        {
            string countryCode = request.CountryCode.Trim().ToUpperInvariant();
            hotels = hotels.Where(hotel => hotel.CountryCode == countryCode);
        }

        if (request.Stars is int stars) hotels = hotels.Where(hotel => hotel.Stars == stars);

        IOrderedQueryable<Hotel>? sorted = request.Sort switch
        {
            null or "name" => hotels.OrderBy(hotel => hotel.Name),
            "-name" => hotels.OrderByDescending(hotel => hotel.Name),
            "stars" => hotels.OrderBy(hotel => hotel.Stars),
            "-stars" => hotels.OrderByDescending(hotel => hotel.Stars),
            "city" => hotels.OrderBy(hotel => hotel.City),
            "-city" => hotels.OrderByDescending(hotel => hotel.City),
            "countryCode" => hotels.OrderBy(hotel => hotel.CountryCode),
            "-countryCode" => hotels.OrderByDescending(hotel => hotel.CountryCode),
            _ => null,
        };
        if (sorted is null) return RequestErrors.SortInvalid(request.Sort);

        return await sorted
            .ThenBy(hotel => hotel.Id)
            .Select(hotel => new HotelListItemResponse(hotel.Id, hotel.Name, hotel.Stars, hotel.City, hotel.CountryCode))
            .ToPagedResponseAsync(request, cancellationToken);
    }

    public async Task<Result<HotelResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Hotel? hotel = await dbContext.Hotels.FindAsync([id], cancellationToken);

        return hotel is null ? HotelErrors.NotFound(id) : hotel.ToResponse();
    }

    public async Task<Result<HotelResponse>> CreateAsync(CreateHotelRequest request, CancellationToken cancellationToken)
    {
        Result<Hotel> created = Hotel.Create(
            request.Name,
            request.Stars,
            request.Address,
            request.City,
            request.CountryCode,
            request.ContactEmail,
            request.ContactPhone,
            timeProvider.GetUtcNow().UtcDateTime);
        if (created.IsFailure) return created.Error;

        dbContext.Hotels.Add(created.Value);
        await dbContext.SaveChangesAsync(cancellationToken);

        return created.Value.ToResponse();
    }
}
