using System;
using System.Threading.Tasks;
using AutoFixture;
using DispatcherWeb.DailyHistory;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.MultiTenancy.HostDashboard;
using DispatcherWeb.MultiTenancy.HostDashboard.Dto;
using Xunit;

namespace DispatcherWeb.Tests.MultiTenancy.HostDashboard
{
    public abstract class HostDashboardAppService_GetDashboardStatisticsData_Fixture : AppTestBase, IAsyncLifetime
    {
        protected DateTime _yesterday;
        private (DateTime startDate, DateTime endDate) _interval;
        private Tenant _tenant;

        public async Task InitializeAsync()
        {
            // Arrange
            var hostDashboardAppService = Resolve<IHostDashboardAppService>();
            _yesterday = DateTime.UtcNow.Date.AddDays(-1);
            _interval = GetInterval();
            _tenant = await CreateTenant("tenant2");

            await FillData();

            var tenantToDelete = await CreateTenant("tenant404");
            await UsingDbContextAsync(async context => await CreateTenantDailyHistory(new Fixture(), context, tenantToDelete.Id, _yesterday));
            await UpdateEntity(tenantToDelete, t => t.IsDeleted = true);

            LoginAsHostAdmin();

            // Act
            HostDashboardKpiData = await hostDashboardAppService.GetDashboardKpiData(new GetDashboardDataInput
            {
                StartDate = _interval.startDate,
                EndDate = _interval.endDate,
                IncomeStatisticsDateInterval = ChartDateInterval.Daily,
            });
        }

        protected abstract (DateTime startDate, DateTime endDate) GetInterval();


        public int ActiveTrucks { get; protected set; }
        public int ActiveUsers { get; protected set; }
        public int UsersWithActivity { get; protected set; }
        public int OrderLinesCreated { get; protected set; }
        public int InternalTrucksScheduled { get; protected set; }
        public int InternalScheduledDeliveries { get; protected set; }
        public int LeaseHaulerScheduledDeliveries { get; protected set; }

        public HostDashboardKpiDto HostDashboardKpiData { get; private set; }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual async Task FillData()
        {
            var startDate = _interval.startDate.Add(_interval.startDate - _interval.endDate); // Expand interval to ensure that data are properly filtered by date interval
            while (startDate < _interval.endDate)
            {
                await FillData(startDate);
                startDate = startDate.AddDays(1);
            }
        }

        private async Task FillData(DateTime date)
        {
            var fixture = new Fixture();
            await UsingDbContextAsync(async context =>
            {
                await CreateTenantDailyHistoryEntity(1, date);

                await CreateTenantDailyHistoryEntity(_tenant.Id, date);

                // Local functions
                async Task CreateTenantDailyHistoryEntity(int tenantId, DateTime tdhDate)
                {
                    var tenantDailyHistory = await CreateTenantDailyHistory(fixture, context, tenantId, tdhDate);

                    if (tdhDate >= _interval.startDate && tdhDate < _interval.endDate)
                    {
                        if (tdhDate >= _yesterday && tdhDate < _yesterday.AddDays(1))
                        {
                            ActiveTrucks += tenantDailyHistory.ActiveTrucks;
                            ActiveUsers += tenantDailyHistory.ActiveUsers;
                            UsersWithActivity += tenantDailyHistory.UsersWithActivity;
                        }
                        OrderLinesCreated += tenantDailyHistory.OrderLinesCreated;
                        InternalTrucksScheduled += tenantDailyHistory.InternalTrucksScheduled;
                        InternalScheduledDeliveries += tenantDailyHistory.InternalScheduledDeliveries;
                        LeaseHaulerScheduledDeliveries += tenantDailyHistory.LeaseHaulerScheduledDeliveries;
                    }
                }
            });
        }

        private static async Task<TenantDailyHistory> CreateTenantDailyHistory(Fixture fixture, DispatcherWebDbContext context, int tenantId, DateTime tdhDate)
        {
            var tenantDailyHistory =
                fixture.Build<TenantDailyHistory>()
                    .Without(x => x.Id)
                    .With(x => x.TenantId, tenantId)
                    .Without(x => x.Tenant)
                    .With(x => x.Date, tdhDate.Date)
                    .Create();
            await context.TenantDailyHistory.AddAsync(tenantDailyHistory);
            return tenantDailyHistory;
        }
    }
}
