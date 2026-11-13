using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Altairis.Backoffice.Contracts.Common;

public abstract class PagedRequest
{
    public const int DEFAULT_PAGE_SIZE = 20;
    public const int MAX_PAGE_SIZE = 100;
    public const int MAX_PAGE = int.MaxValue / MAX_PAGE_SIZE;

    [Range(1, MAX_PAGE)]
    public int Page { get; init; } = 1;

    [Range(1, MAX_PAGE_SIZE)]
    public int PageSize { get; init; } = DEFAULT_PAGE_SIZE;

    [Description("A sortable field name; prefix it with '-' for descending order, e.g. '-stars'.")]
    public string? Sort { get; init; }
}
