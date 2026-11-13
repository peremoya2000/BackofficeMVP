using Altairis.Backoffice.Domain.Common;

namespace Altairis.Backoffice.Domain.Bookings;

public static class StayPeriodErrors
{
    public static readonly Error CheckOutNotAfterCheckIn = Error.Validation(
        "stay_period.check_out_not_after_check_in",
        "La fecha de salida debe ser posterior a la de entrada.");
}
