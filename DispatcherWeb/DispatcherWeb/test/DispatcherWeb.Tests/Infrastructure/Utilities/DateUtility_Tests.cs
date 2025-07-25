using System;
using DispatcherWeb.Infrastructure.Utilities;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Infrastructure.Utilities
{
    public class DateUtility_Tests
    {
        [Fact]
        public void Test_NumberOfWeeksBetweenDates_should_return_1_Week_between_Saturday_and_Sunday() =>
            DateUtility.NumberOfWeeksBetweenDates(new DateTime(2019, 07, 20), new DateTime(2019, 07, 21)).ShouldBe(1);

        [Fact]
        public void Test_NumberOfWeeksBetweenDates_should_return_5_Week_between_20190301_and_20190331() =>
            DateUtility.NumberOfWeeksBetweenDates(new DateTime(2019, 3, 1), new DateTime(2019, 3, 31)).ShouldBe(5);

        [Fact]
        public void Test_NumberOfWeeksBetweenDates_should_return_0_Week_between_Sunday_and_Monday() =>
            DateUtility.NumberOfWeeksBetweenDates(new DateTime(2019, 07, 21), new DateTime(2019, 07, 22)).ShouldBe(0);

        [Fact]
        public void Test_NumberOfWeeksBetweenDates_should_return_0_Week_between_Sunday_and_Saturday() =>
            DateUtility.NumberOfWeeksBetweenDates(new DateTime(2019, 07, 21), new DateTime(2019, 07, 27)).ShouldBe(0);

        [Fact]
        public void Test_NumberOfWeeksBetweenDates_should_return_3_Week_between_Saturday_and_Sunday_from_3_weeks() =>
            DateUtility.NumberOfWeeksBetweenDates(new DateTime(2019, 07, 20), new DateTime(2019, 07, 28)).ShouldBe(2);

        [Fact]
        public void Test_NumberOfWeeksBetweenDates_should_return_53_Week_between_20190101_and_20191231() =>
            DateUtility.NumberOfWeeksBetweenDates(new DateTime(2019, 01, 01), new DateTime(2019, 12, 31)).ShouldBe(52);

        [Fact]
        public void Test_NumberOfWeeksBetweenDates_should_return_0_Week_between_20191231_and_20200101() =>
            DateUtility.NumberOfWeeksBetweenDates(new DateTime(2019, 01, 01), new DateTime(2019, 12, 31)).ShouldBe(52);

        [Fact]
        public void Test_NumberOfWeeksBetweenDates_should_return_0_Week_between_20191231_and_20210101() =>
            DateUtility.NumberOfWeeksBetweenDates(new DateTime(2019, 01, 01), new DateTime(2019, 12, 31)).ShouldBe(52);

    }
}
