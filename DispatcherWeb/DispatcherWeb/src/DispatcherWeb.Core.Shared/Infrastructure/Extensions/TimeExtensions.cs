using System;

namespace DispatcherWeb
{
    public static class TimeExtensions
    {
        public static DateTime ConvertTimeZoneTo(this DateTime date, string timeZone)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.SpecifyKind(date, DateTimeKind.Utc), TimeZoneInfo.Utc.Id, timeZone ?? TimeZoneInfo.Utc.Id);
        }

        public static DateTime GetToday(string timeZone)
        {
            return DateTime.UtcNow.ConvertTimeZoneTo(timeZone).Date;
        }

        public static DateTime ConvertTimeZoneFrom(this DateTime date, string timeZone)
        {
            return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date, DateTimeKind.Unspecified), TimeZoneInfo.FindSystemTimeZoneById(timeZone ?? TimeZoneInfo.Utc.Id));
        }

        public static DateTime? AddTimeOrNull(this DateTime date, DateTime? time)
        {
            return time == null ? (DateTime?)null : date.Date.Add(time.Value.TimeOfDay);
        }

        public static DateTime? AddOrNull(this DateTime date, TimeSpan? time)
        {
            return time == null ? (DateTime?)null : date.Date.Add(time.Value);
        }
    }
}
