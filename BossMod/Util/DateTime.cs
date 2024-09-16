namespace BossMod;

public static class DateTimeExtensions
{
    public static DateTime Floor(this DateTime dt, TimeSpan interval)
        => dt.AddTicks(-(dt.Ticks % interval.Ticks));

    public static DateTime Ceiling(this DateTime dt, TimeSpan interval)
    {
        var overflow = dt.Ticks % interval.Ticks;
        return overflow == 0 ? dt : dt.AddTicks(interval.Ticks - overflow);
    }

    public static DateTime Round(this DateTime dt, TimeSpan interval)
    {
        var halfIntervalTicks = (interval.Ticks + 1) >> 1;

        return dt.AddTicks(halfIntervalTicks - (dt.Ticks + halfIntervalTicks) % interval.Ticks);
    }
}
