using Altairis.Backoffice.Contracts.Common;
using Microsoft.EntityFrameworkCore;

namespace Altairis.Backoffice.Application.Common;

public static class PagingExtensions
{
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        PagedRequest request,
        CancellationToken cancellationToken)
    {
        int totalCount = await query.CountAsync(cancellationToken);
        List<T> items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<T>(items, request.Page, request.PageSize, totalCount);
    }
}
