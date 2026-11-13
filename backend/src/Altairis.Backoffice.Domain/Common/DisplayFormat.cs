using System.Globalization;

namespace Altairis.Backoffice.Domain.Common;

public static class DisplayFormat
{
    public static string Date(DateOnly date) => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
}
