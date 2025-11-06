namespace MySolarCells.Helpers;

public static class DateHelper
{
    public static RelatedDateResult GetRelatedDates(DateTime input)
    {
        var result = new RelatedDateResult
        {
            BaseDate = input
        };
        result.Yesterday = result.BaseDate.AddDays(-1);
        result.ThisWeekStart = result.BaseDate.AddDays(-(int)result.BaseDate.DayOfWeek);
        //fix for sunday
        if (result.ThisWeekStart.DayOfWeek != DayOfWeek.Monday)
            result.ThisWeekStart = result.ThisWeekStart.AddDays(1);

        result.ThisWeekEnd = result.ThisWeekStart.AddDays(7).AddSeconds(-1);
        result.LastWeekStart = result.ThisWeekStart.AddDays(-7);
        result.LastWeekEnd = result.ThisWeekStart.AddSeconds(-1);
        result.ThisMonthStart = result.BaseDate.AddDays(1 - result.BaseDate.Day);
        result.ThisMonthEnd = result.ThisMonthStart.AddMonths(1).AddSeconds(-1);
        result.LastMonthStart = result.ThisMonthStart.AddMonths(-1);
        result.LastMonthEnd = result.ThisMonthStart.AddSeconds(-1);
        result.ThisYearStart = new DateTime(result.ThisMonthStart.Year, 1, 1);
        result.ThisYearEnd = new DateTime(result.ThisMonthStart.Year, 12, 31).AddDays(1);
        return result;
    }
    private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    /// <summary>Get extra long current timestamp</summary>
    public static long DateTimeToMillis(DateTime date)
    {
        return (long)((date.ToUniversalTime() - Jan1St1970).TotalMilliseconds);
    }
    public static DateTime MillisToDateTime(long milliseconds)
    {

        TimeSpan time = TimeSpan.FromMilliseconds(milliseconds);
        DateTime startDate = Jan1St1970 + time;
        return startDate.ToLocalTime();
    }

}
public class RelatedDateResult
{
    public DateTime BaseDate { get; set; }
    public DateTime Yesterday { get; set; }
    public DateTime ThisWeekStart { get; set; }
    public DateTime ThisWeekEnd { get; set; }
    public DateTime LastWeekStart { get; set; }
    public DateTime LastWeekEnd { get; set; }
    public DateTime ThisMonthStart { get; set; }
    public DateTime ThisMonthEnd { get; set; }
    public DateTime LastMonthEnd { get; set; }
    public DateTime LastMonthStart { get; set; }
    public DateTime ThisYearStart { get; set; }
    public DateTime ThisYearEnd { get; set; }


}

