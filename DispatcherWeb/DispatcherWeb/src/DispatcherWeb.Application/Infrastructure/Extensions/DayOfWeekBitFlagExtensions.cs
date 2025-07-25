using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Collections.Extensions;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class DayOfWeekBitFlagExtensions
    {
        public static IEnumerable<DayOfWeek> ToDayOfWeekEnumerable(this DayOfWeekBitFlag dayOfWeekBitFlag)
        {
            if ((dayOfWeekBitFlag & DayOfWeekBitFlag.Sunday) == DayOfWeekBitFlag.Sunday)
            {
                yield return DayOfWeek.Sunday;
            }
            if ((dayOfWeekBitFlag & DayOfWeekBitFlag.Monday) == DayOfWeekBitFlag.Monday)
            {
                yield return DayOfWeek.Monday;
            }
            if ((dayOfWeekBitFlag & DayOfWeekBitFlag.Tuesday) == DayOfWeekBitFlag.Tuesday)
            {
                yield return DayOfWeek.Tuesday;
            }
            if ((dayOfWeekBitFlag & DayOfWeekBitFlag.Wednesday) == DayOfWeekBitFlag.Wednesday)
            {
                yield return DayOfWeek.Wednesday;
            }
            if ((dayOfWeekBitFlag & DayOfWeekBitFlag.Thursday) == DayOfWeekBitFlag.Thursday)
            {
                yield return DayOfWeek.Thursday;
            }
            if ((dayOfWeekBitFlag & DayOfWeekBitFlag.Friday) == DayOfWeekBitFlag.Friday)
            {
                yield return DayOfWeek.Friday;
            }
            if ((dayOfWeekBitFlag & DayOfWeekBitFlag.Saturday) == DayOfWeekBitFlag.Saturday)
            {
                yield return DayOfWeek.Saturday;
            }
        }

        public static bool IsDaySet(this DayOfWeekBitFlag dayOfWeekBitFlag, DayOfWeek dayOfWeek)
        {
            DayOfWeekBitFlag day = DayOfWeekToDayOfWeekBitFlag(dayOfWeek);
            return (dayOfWeekBitFlag & day) == day;
        }

        private static DayOfWeekBitFlag DayOfWeekToDayOfWeekBitFlag(DayOfWeek dayOfWeek)
        {
            return (DayOfWeekBitFlag)(int)Math.Pow(2, (int)dayOfWeek);
        }

        public static DayOfWeekBitFlag GetDayOfWeekBitFlag(this int[] dayOfWeekArray)
        {
            DayOfWeekBitFlag result = 0;
            foreach (int i in dayOfWeekArray)
            {
                result |= DayOfWeekToDayOfWeekBitFlag((DayOfWeek)i);
            }
            return result;
        }

        public static string ToCronDayOfWeek(this DayOfWeekBitFlag dayOfWeek)
        {
            return dayOfWeek.ToDayOfWeekEnumerable().Select(d => ((int)d).ToString("N0")).JoinAsString(",");
        }

    }
}
