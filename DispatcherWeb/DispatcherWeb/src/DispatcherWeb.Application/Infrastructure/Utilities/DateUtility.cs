using System;
using DispatcherWeb.Infrastructure.Extensions;

namespace DispatcherWeb.Infrastructure.Utilities
{
    public class DateUtility
    {
        public static DateTime Max(DateTime d1, DateTime d2)
        {
            return d1 > d2 ? d1 : d2;
        }

        public static DateTime Min(DateTime d1, DateTime d2)
        {
            return d1 < d2 ? d1 : d2;
        }

        public static int NumberOfWeeksBetweenDates(DateTime beginDate, DateTime endDate)
        {
            if (beginDate.Year == endDate.Year)
            {
                return endDate.GetWeekOfYear() - beginDate.GetWeekOfYear();
            }

            int numberOfWeeks = 0;
            for (int y = beginDate.Year; y < endDate.Year; y++)
            {
                var lastDay = new DateTime(y, 12, 31);
                numberOfWeeks += endDate.GetWeekOfYear() - beginDate.Year == lastDay.Year ? endDate.GetWeekOfYear() : 0;
            }

            return numberOfWeeks + endDate.GetWeekOfYear();
        }

        public static int NumberOfMonthsBetweenDates(DateTime beginDate, DateTime endDate)
        {
            return (endDate.Year - beginDate.Year) * 12 + endDate.Month - beginDate.Month;
        }
    }
}
