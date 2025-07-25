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
    public class DailyHistoryAppService_FillUserDailyHistory_Tests : AppTestBase, IAsyncLifetime
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
        public async Task Test_FillUserDailyHistory_should_fill_UserDailyHistory()
        {
            // Arrange
            var user1 = await CreateUser(1); // 1
            await CreateAuditLog(1, user1.Id, _yesterdayUtc);
            await CreateAuditLog(1, user1.Id, _todayUtc.AddMilliseconds(-1));
            var user2 = await CreateUser(1); // 2
            await CreateAuditLog(1, user2.Id, _yesterdayUtc);

            var user3 = await CreateUser(1); // No records in the AuditLog

            var user4 = await CreateUser(_tenant2Id); // 3
            await CreateAuditLog(_tenant2Id, user4.Id, _yesterdayUtc);

            // Act
            await _dailyHistoryAppService.FillUserDailyHistoryAsync(_todayUtc);

            // Assert
            var userDailyHistories = await UsingDbContextAsync(async context => await context.UserDailyHistory.ToListAsync());
            userDailyHistories.Count.ShouldBe(3);
            userDailyHistories.First(x => x.UserId == user1.Id).NumberOfTransactions.ShouldBe(2);
            userDailyHistories.First(x => x.UserId == user2.Id).NumberOfTransactions.ShouldBe(1);
            userDailyHistories.Any(x => x.UserId == user3.Id).ShouldBeFalse();
            userDailyHistories.First(x => x.UserId == user4.Id).NumberOfTransactions.ShouldBe(1);

        }

        [Fact]
        public async Task Test_FillUserDailyHistory_should_ignore_records_with_null_UserId()
        {
            // Arrange
            await CreateAuditLog(null, null, _yesterdayUtc);

            // Act
            await _dailyHistoryAppService.FillUserDailyHistoryAsync(_todayUtc);

            // Assert
            var userDailyHistories = await UsingDbContextAsync(async context => await context.UserDailyHistory.ToListAsync());
            userDailyHistories.Count.ShouldBe(0);
        }



        public Task DisposeAsync() => Task.CompletedTask;
    }
}
