namespace AniListDiscordBot.Services;

public class TimeZoneUtils
{
    private static readonly TimeZoneInfo PolandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

    public static DateTime ToPolandTime(DateTime dateTime)
    {
        var utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, PolandTimeZone);
    }

    public static DateTime ToUtc(DateTime dateTime)
    {
        var polandDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(polandDateTime, PolandTimeZone);
    }

    public static string GetDateTimeInfo(DateTime dateTime)
    {
        return $"DateTime: {dateTime}, Kind: {dateTime.Kind}, Ticks: {dateTime.Ticks}";
    }
}