using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.MultiTenancy.HostDashboard
{
    public abstract class HostDashboardAppService_GetDashboardStatisticsData_Tests
    {
        protected HostDashboardAppService_GetDashboardStatisticsData_Fixture _fixture;

        [Fact]
        public void Test_GetDashboardStatisticsData_should_return_KPI_Tenants() => _fixture.HostDashboardKpiData.ActiveTenants.ShouldBe(2);

        [Fact]
        public void Test_GetDashboardStatisticsData_should_return_KPI_ActiveTrucks() => _fixture.HostDashboardKpiData.ActiveTrucks.ShouldBe(_fixture.ActiveTrucks);

        [Fact]
        public void Test_GetDashboardStatisticsData_should_return_KPI_ActiveUsers() => _fixture.HostDashboardKpiData.ActiveUsers.ShouldBe(_fixture.ActiveUsers);

        [Fact(Skip = "#14648 Historically failing")]
        public void Test_GetDashboardStatisticsData_should_return_KPI_UsersWithActivity() => _fixture.HostDashboardKpiData.UsersWithActivity.ShouldBe(_fixture.UsersWithActivity);

        [Fact]
        public void Test_GetDashboardStatisticsData_should_return_KPI_OrderLinesCreated() => _fixture.HostDashboardKpiData.OrderLinesCreated.ShouldBe(_fixture.OrderLinesCreated);

        [Fact]
        public void Test_GetDashboardStatisticsData_should_return_KPI_InternalTrucksScheduled() => _fixture.HostDashboardKpiData.InternalTrucksScheduled.ShouldBe(_fixture.InternalTrucksScheduled);

        [Fact]
        public void Test_GetDashboardStatisticsData_should_return_KPI_InternalScheduledDeliveries() => _fixture.HostDashboardKpiData.InternalScheduledDeliveries.ShouldBe(_fixture.InternalScheduledDeliveries);

        [Fact]
        public void Test_GetDashboardStatisticsData_should_return_KPI_LeaseHaulerScheduledDeliveries() => _fixture.HostDashboardKpiData.LeaseHaulerScheduledDeliveries.ShouldBe(_fixture.LeaseHaulerScheduledDeliveries);
    }
}
