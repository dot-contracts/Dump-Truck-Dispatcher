using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.DailyHistory;
using DispatcherWeb.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.DailyHistory
{
    public class DailyHistoryAppService_FillTransactionDailyHistory_Tests : AppTestBase, IAsyncLifetime
    {
        private IDailyHistoryAppService _dailyHistoryAppService;
        private DateTime _todayUtc;
        private DateTime _yesterdayUtc;
        private int _tenant2Id;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _dailyHistoryAppService = Resolve<IDailyHistoryAppService>();
            _todayUtc = Clock.Now.Date;
            _yesterdayUtc = _todayUtc.AddDays(-1);
            var yesterdayUtc = _yesterdayUtc;
            var tenant2 = await UsingDbContextAsync(async context =>
            {
                var tenant = new Tenant("tenant2", "tenant2");
                await context.Tenants.AddAsync(tenant);
                return tenant;
            });
            _tenant2Id = tenant2.Id;
        }

        [Fact]
        public async Task Test_FillTransactionDailyHistory_should_fill_TransactionDailyHistory()
        {
            // Arrange
            const string service1Name = "Service1Name";
            const string service2Name = "Service2Name";
            const string method1Name = "Method1Name";
            const string method2Name = "Method2Name";
            var user1 = await CreateUser(1);
            await CreateAuditLog(1, user1.Id, _yesterdayUtc, 100, service1Name, method1Name); // 1
            await CreateAuditLog(1, user1.Id, _todayUtc.AddMilliseconds(-1), 500, service2Name, method2Name); // 2
            await CreateAuditLog(1, user1.Id, _todayUtc, 0, service1Name, method1Name);
            var user2 = await CreateUser(1);
            await CreateAuditLog(1, user2.Id, _yesterdayUtc, 300, service1Name, method1Name); // 1

            // Act
            await _dailyHistoryAppService.FillTransactionDailyHistoryAsync(_todayUtc);

            // Assert
            var transactionDailyHistories = await UsingDbContextAsync(async context => await context.TransactionDailyHistory.ToListAsync());
            transactionDailyHistories.Count.ShouldBe(2);
            transactionDailyHistories.First(x => x.ServiceName == service1Name).NumberOfTransactions.ShouldBe(2);
            transactionDailyHistories.First(x => x.ServiceName == service1Name).AverageExecutionDuration.ShouldBe(200);
            transactionDailyHistories.First(x => x.ServiceName == service2Name).NumberOfTransactions.ShouldBe(1);
            transactionDailyHistories.First(x => x.ServiceName == service2Name).AverageExecutionDuration.ShouldBe(500);
            transactionDailyHistories.First(x => x.MethodName == method1Name).NumberOfTransactions.ShouldBe(2);
            transactionDailyHistories.First(x => x.MethodName == method1Name).AverageExecutionDuration.ShouldBe(200);
            transactionDailyHistories.First(x => x.MethodName == method2Name).NumberOfTransactions.ShouldBe(1);
            transactionDailyHistories.First(x => x.MethodName == method2Name).AverageExecutionDuration.ShouldBe(500);
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
