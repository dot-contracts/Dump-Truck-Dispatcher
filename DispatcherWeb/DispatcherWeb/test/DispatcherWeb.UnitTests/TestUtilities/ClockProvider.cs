using System;
using Abp.Timing;

namespace DispatcherWeb.UnitTests.TestUtilities
{
    public class ClockProvider : IClockProvider
    {
        private static readonly DateTime dateTimeNow = DateTime.UtcNow;
        public static DateTime DateTimeNow => dateTimeNow;

        static ClockProvider()
        {
            Clock.Provider = ClockProviders.Utc;
        }

        public DateTime Normalize(DateTime dateTime) => dateTime;
        public DateTime Now => dateTimeNow;
        public DateTimeKind Kind => DateTimeKind.Utc;
        public bool SupportsMultipleTimezone => false;
    }
}
