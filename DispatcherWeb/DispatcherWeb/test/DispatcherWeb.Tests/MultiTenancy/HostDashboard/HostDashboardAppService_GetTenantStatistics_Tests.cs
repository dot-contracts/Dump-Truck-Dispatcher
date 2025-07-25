using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using DispatcherWeb.DailyHistory;
using DispatcherWeb.MultiTenancy.HostDashboard;
using DispatcherWeb.MultiTenancy.HostDashboard.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.MultiTenancy.HostDashboard
{
    public class HostDashboardAppService_GetTenantStatistics_Tests : AppTestBase, IAsyncLifetime
    {
        protected static DateTime _yesterday;
        private IHostDashboardAppService _hostDashboardAppService;

        static HostDashboardAppService_GetTenantStatistics_Tests()
        {
            _yesterday = DateTime.UtcNow.Date.AddDays(-1);
            Dates = new[]
            {
                new object[] { _yesterday.AddDays(-7) },
                new object[] { _yesterday },
            };
        }

        public Task InitializeAsync()
        {
            _hostDashboardAppService = Resolve<IHostDashboardAppService>();

            LoginAsHostAdmin();
            return Task.CompletedTask;
        }

        public static object[][] Dates;

        [Theory(Skip = "#14648 Historically failing")]
        [MemberData(nameof(Dates))]
        public async Task Test_GetTenantStatistics_should_return_TenantStatisticsDto_List_for_Day(DateTime date)
        {
            // Arrange
            await CreateTenantDailyHistoryEntity(1, date.AddDays(-1)); // To check filtering by date
            var tenantDailyHistory = await CreateTenantDailyHistoryEntity(1, date);
            var yesterdayTenantDailyHistory = date != _yesterday ? await CreateTenantDailyHistoryEntity(1, _yesterday) : tenantDailyHistory;

            // Act
            var tsList = await _hostDashboardAppService.GetTenantStatistics(new GetDashboardDataInput
            {
                StartDate = date,
                EndDate = date.AddDays(1).AddMilliseconds(-1),
            });

            // Assert
            tsList.Items.Count.ShouldBe(1);
            tsList.Items[0].TenantName.ShouldBe("Default");
            tsList.Items[0].NumberOfTrucks.ShouldBe(yesterdayTenantDailyHistory.ActiveTrucks);
            tsList.Items[0].NumberOfUsers.ShouldBe(yesterdayTenantDailyHistory.ActiveUsers);
            tsList.Items[0].NumberOfUsersActive.ShouldBe(yesterdayTenantDailyHistory.UsersWithActivity);
            tsList.Items[0].OrderLines.ShouldBe(tenantDailyHistory.OrderLinesScheduled);
            tsList.Items[0].TrucksScheduled.ShouldBe(tenantDailyHistory.InternalTrucksScheduled);
            tsList.Items[0].LeaseHaulersOrderLines.ShouldBe(tenantDailyHistory.LeaseHaulerScheduledDeliveries);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_GetTenantStatistics_should_return_TenantStatisticsDto_List_for_2_Days()
        {
            // Arrange
            var date = _yesterday;
            await CreateTenantDailyHistoryEntity(1, date.AddDays(-2)); // To check filtering by date
            var tenantDailyHistory2 = await CreateTenantDailyHistoryEntity(1, date.AddDays(-1));
            var tenantDailyHistory = await CreateTenantDailyHistoryEntity(1, date);

            // Act
            var tsList = await _hostDashboardAppService.GetTenantStatistics(new GetDashboardDataInput
            {
                StartDate = date.AddDays(-1),
                EndDate = date.AddDays(1).AddMilliseconds(-1),
            });

            // Assert
            tsList.Items.Count.ShouldBe(1);
            tsList.Items[0].TenantName.ShouldBe("Default");
            tsList.Items[0].NumberOfTrucks.ShouldBe(tenantDailyHistory.ActiveTrucks);
            tsList.Items[0].NumberOfUsers.ShouldBe(tenantDailyHistory.ActiveUsers);
            tsList.Items[0].NumberOfUsersActive.ShouldBe(tenantDailyHistory.UsersWithActivity);
            tsList.Items[0].OrderLines.ShouldBe(tenantDailyHistory.OrderLinesScheduled + tenantDailyHistory2.OrderLinesScheduled);
            tsList.Items[0].TrucksScheduled.ShouldBe(tenantDailyHistory.InternalTrucksScheduled + tenantDailyHistory2.InternalTrucksScheduled);
            tsList.Items[0].LeaseHaulersOrderLines.ShouldBe(tenantDailyHistory.LeaseHaulerScheduledDeliveries + tenantDailyHistory2.LeaseHaulerScheduledDeliveries);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_GetTenantStatistics_should_return_TenantStatisticsDto_List_for_Day_before_yesterday_and_include_tenant_created_yesterday()
        {
            // Arrange
            var date = _yesterday;
            await CreateTenantDailyHistoryEntity(1, date.AddDays(-2)); // To check filtering by date
            var tenantDailyHistory2 = await CreateTenantDailyHistoryEntity(1, date.AddDays(-1));
            var tenantDailyHistory3 = await CreateTenantDailyHistoryEntity(1, date);
            var tenant2 = await CreateTenant("tenant2");
            var tenantDailyHistory = await CreateTenantDailyHistoryEntity(tenant2.Id, date);

            // Act
            var tsList = await _hostDashboardAppService.GetTenantStatistics(new GetDashboardDataInput
            {
                StartDate = date.AddDays(-1),
                EndDate = date.AddMilliseconds(-1),
            });

            // Assert
            tsList.Items.Count.ShouldBe(2);
            var item1 = tsList.Items.First(x => x.TenantId == 1);
            item1.TenantName.ShouldBe("Default");
            item1.NumberOfTrucks.ShouldBe(tenantDailyHistory3.ActiveTrucks);
            item1.NumberOfUsers.ShouldBe(tenantDailyHistory3.ActiveUsers);
            item1.NumberOfUsersActive.ShouldBe(tenantDailyHistory3.UsersWithActivity);
            item1.OrderLines.ShouldBe(tenantDailyHistory2.OrderLinesScheduled);
            item1.TrucksScheduled.ShouldBe(tenantDailyHistory2.InternalTrucksScheduled);
            item1.LeaseHaulersOrderLines.ShouldBe(tenantDailyHistory2.LeaseHaulerScheduledDeliveries);

            var item2 = tsList.Items.First(x => x.TenantId == tenant2.Id);
            item2.TenantName.ShouldBe(tenant2.TenancyName);
            item2.NumberOfTrucks.ShouldBe(tenantDailyHistory.ActiveTrucks);
            item2.NumberOfUsers.ShouldBe(tenantDailyHistory.ActiveUsers);
            item2.NumberOfUsersActive.ShouldBe(tenantDailyHistory.UsersWithActivity);
            item2.OrderLines.ShouldBe(0);
            item2.TrucksScheduled.ShouldBe(0);
            item2.LeaseHaulersOrderLines.ShouldBe(0);
        }

        private async Task<TenantDailyHistory> CreateTenantDailyHistoryEntity(int tenantId, DateTime date)
        {
            return await UsingDbContextAsync(async context =>
            {
                var fixture = new Fixture();
                var tenantDailyHistory =
                    fixture.Build<TenantDailyHistory>()
                        .Without(x => x.Id)
                        .With(x => x.TenantId, tenantId)
                        .Without(x => x.Tenant)
                        .With(x => x.Date, date.Date)
                        .Create();
                await context.TenantDailyHistory.AddAsync(tenantDailyHistory);
                return tenantDailyHistory;
            });
        }


        public Task DisposeAsync() => Task.CompletedTask;
    }
}
