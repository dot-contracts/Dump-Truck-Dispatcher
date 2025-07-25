using System;
using System.Threading.Tasks;
using AutoFixture;
using DispatcherWeb.DailyHistory;
using DispatcherWeb.MultiTenancy.HostDashboard;
using DispatcherWeb.MultiTenancy.HostDashboard.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.MultiTenancy.HostDashboard
{
    public class HostDashboardAppService_GetRequests_Tests : AppTestBase, IAsyncLifetime
    {
        protected DateTime _today;
        protected DateTime _yesterday;
        private IHostDashboardAppService _hostDashboardAppService;

        public Task InitializeAsync()
        {
            _hostDashboardAppService = Resolve<IHostDashboardAppService>();
            _today = DateTime.UtcNow.Date;
            _yesterday = _today.AddDays(-1);

            LoginAsHostAdmin();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Test_GetRequests_should_return_requests()
        {
            // Arrange
            await CreateTransactionDailyHistoryEntity(_yesterday, "Service1", "Method1", 10, 100);
            await CreateTransactionDailyHistoryEntity(_today.AddDays(-7), "Service1", "Method1", 20, 200);
            await CreateTransactionDailyHistoryEntity(_today.AddMonths(-1), "Service1", "Method1", 30, 300);

            // Act
            var requests = await _hostDashboardAppService.GetRequests(new GetRequestsInput
            {
                MaxResultCount = 20,
            });

            // Assert
            requests.Items.Count.ShouldBe(1);
            var request = requests.Items[0];
            request.AverageExecutionDuration.ShouldBe(100);
            request.NumberOfTransactions.ShouldBe(10);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_GetRequests_should_return_requests_when_were_no_requests_yesterday()
        {
            // Arrange
            await CreateTransactionDailyHistoryEntity(_today.AddDays(-7), "Service1", "Method1", 20, 200);
            await CreateTransactionDailyHistoryEntity(_today.AddMonths(-1), "Service1", "Method1", 30, 300);

            // Act
            var requests = await _hostDashboardAppService.GetRequests(new GetRequestsInput
            {
                MaxResultCount = 20,
            });

            // Assert
            requests.Items.Count.ShouldBe(1);
            var request = requests.Items[0];
            request.AverageExecutionDuration.ShouldBe(0);
            request.NumberOfTransactions.ShouldBe(0);
        }

        [Fact]
        public async Task Test_GetRequests_should_return_first_20_records()
        {
            // Arrange
            const int numberOfRecords = 20;
            for (int i = 0; i < numberOfRecords + 11; i++)
            {
                await CreateTransactionDailyHistoryEntity(_yesterday, $"Service{i}", $"Method{i}", i, i * 10);
            }

            // Act
            var requests = await _hostDashboardAppService.GetRequests(new GetRequestsInput
            {
                MaxResultCount = 20,
            });

            // Assert
            requests.Items.Count.ShouldBe(numberOfRecords);
            var request = requests.Items[0];
            request.AverageExecutionDuration.ShouldBe(300);
            request.NumberOfTransactions.ShouldBe(30);
        }


        private async Task<TransactionDailyHistory> CreateTransactionDailyHistoryEntity(
            DateTime date,
            string serviceName,
            string methodName,
            int numberOfTransaction,
            int averageExecutionTime
        )
        {
            return await UsingDbContextAsync(async context =>
            {
                var fixture = new Fixture();
                var transactionDailyHistory =
                    fixture.Build<TransactionDailyHistory>()
                        .Without(x => x.Id)
                        .With(x => x.Date, date.Date)
                        .With(x => x.ServiceName, serviceName)
                        .With(x => x.MethodName, methodName)
                        .With(x => x.NumberOfTransactions, numberOfTransaction)
                        .With(x => x.AverageExecutionDuration, averageExecutionTime)
                        .Create();
                await context.TransactionDailyHistory.AddAsync(transactionDailyHistory);
                return transactionDailyHistory;
            });
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
