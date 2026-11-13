namespace Altairis.Backoffice.Application.Common;

public static class OperationsCalendar
{
    private static readonly TimeZoneInfo OperationsTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");

    public static DateOnly DateOf(DateTime utcDateTime) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, OperationsTimeZone));
}
