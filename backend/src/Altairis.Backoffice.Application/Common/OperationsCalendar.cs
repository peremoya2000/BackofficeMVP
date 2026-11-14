namespace Altairis.Backoffice.Application.Common;

public static class OperationsCalendar
{
    private static readonly TimeZoneInfo OperationsTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");

    public static DateOnly DateOf(DateTime utcDateTime) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, OperationsTimeZone));

    public static DateOnly Today(TimeProvider timeProvider) => DateOf(timeProvider.GetUtcNow().UtcDateTime);

    public static DateTime StartOfDayUtc(DateOnly date) =>
        TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue), OperationsTimeZone);
}
