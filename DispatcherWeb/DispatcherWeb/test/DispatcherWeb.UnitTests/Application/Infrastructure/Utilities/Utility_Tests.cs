using System;
using DispatcherWeb.Infrastructure.Utilities;
using Shouldly;
using Xunit;

namespace DispatcherWeb.UnitTests.Application.Infrastructure.Utilities
{
    public class Utility_Tests
    {
        [Fact]
        public void Test_GetCronString_should_return_correct_cron_string()
        {
            Utility.GetCronString(DayOfWeekBitFlag.Monday, new TimeSpan(1, 10, 0))
                .ShouldBe("10 1 * * 1");
            Utility.GetCronString(DayOfWeekBitFlag.Monday | DayOfWeekBitFlag.Thursday, new TimeSpan(16, 25, 0))
                .ShouldBe("25 16 * * 1,4");
            Utility.GetCronString(DayOfWeekBitFlag.Sunday | DayOfWeekBitFlag.Monday | DayOfWeekBitFlag.Tuesday | DayOfWeekBitFlag.Wednesday | DayOfWeekBitFlag.Thursday | DayOfWeekBitFlag.Friday | DayOfWeekBitFlag.Saturday, new TimeSpan(0, 0, 0))
                .ShouldBe("0 0 * * 0,1,2,3,4,5,6");
        }

    }
}
