using System.Globalization;

namespace Altairis.Backoffice.Domain.Common;

internal static class DisplayFormat
{
    public static string Date(DateOnly date) => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
}
