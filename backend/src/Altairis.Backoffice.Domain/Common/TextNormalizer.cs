namespace Altairis.Backoffice.Domain.Common;

internal static class TextNormalizer
{
    public static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
