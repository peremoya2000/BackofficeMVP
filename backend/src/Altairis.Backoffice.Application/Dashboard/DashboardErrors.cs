using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Application.Dashboard;

public static class DashboardErrors
{
    public static readonly Error RangeInverted = Error.Validation(
        "dashboard.range_inverted",
        "La fecha final no puede ser anterior a la inicial.");

    public static Error RangeTooLong(int maxDays) => Error.Validation(
        "dashboard.range_too_long",
        $"El rango no puede superar {maxDays} días.");
}
