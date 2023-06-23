namespace Shared.Tools;

public static class DateTimeTools
{
    public static DateTime? UnixTimeStampToUtcDateTime(long? unixTimeStamp)
    {
        if (!unixTimeStamp.HasValue) return null;

        DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        return dateTime
            .AddSeconds(unixTimeStamp.Value);
    }

    public static DateTime? UnixTimeStampToLocalDateTime(long? unixTimeStamp) =>
        UnixTimeStampToUtcDateTime(unixTimeStamp)?
            .ToLocalTime();
}