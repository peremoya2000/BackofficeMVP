using System.ComponentModel;
using Altairis.Backoffice.Contracts.Common;

namespace Altairis.Backoffice.Contracts.Hotels;

public sealed class HotelSearchRequest : PagedRequest
{
    [Description("Matches part of the hotel's name or city, ignoring case.")]
    public string? Search { get; init; }

    public string? CountryCode { get; init; }

    public int? Stars { get; init; }
}
