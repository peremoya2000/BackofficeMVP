namespace Altairis.Backoffice.Application.Common;

public static class SearchText
{
    // Mapped to the immutable_unaccent SQL function; EF translates it, so it never runs in .NET.
    public static string Unaccent(string text) =>
        throw new InvalidOperationException($"{nameof(SearchText)}.{nameof(Unaccent)} can only be used inside EF Core queries.");
}
