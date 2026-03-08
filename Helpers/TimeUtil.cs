public static class TimeUtil
{
    public static DateTime ToIST(DateTime utc)
    {
        var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(utc, ist);
    }
}
