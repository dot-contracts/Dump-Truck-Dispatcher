using System.Threading.Tasks;
using Abp;
using Abp.Notifications;
using Abp.Runtime.Session;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Notifications;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Twilio.Exceptions;
using Xunit;

namespace DispatcherWeb.Tests.BackgroundJobs
{
    public class DriverMessageSmsSenderBackgroundJob_Tests : AppTestBase, IAsyncLifetime
    {
        private ISmsSender _smsSender;
        private IAppNotifier _appNotifier;
        private int _officeId;
        private DriverMessageSmsSenderBackgroundJob _driverMessageSmsSenderBackgroundJob;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _smsSender = Substitute.For<ISmsSender>();
            _appNotifier = Substitute.For<IAppNotifier>();
            _driverMessageSmsSenderBackgroundJob = Resolve<DriverMessageSmsSenderBackgroundJob>(new
            {
                smsSender = _smsSender,
                appNotifier = _appNotifier,
            });
        }

        [Fact]
        public async Task Test_SendMessage_should_successfully_send_sms()
        {
            // Arrange
            var driver = await CreateDriver("first", "last", _officeId, "+15005550055");
            driver = await UpdateEntity(driver, d => d.OrderNotifyPreferredFormat = OrderNotifyPreferredFormat.Sms);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _driverMessageSmsSenderBackgroundJob.ExecuteAsync(new DriverMessageSmsSenderBackgroundJobArgs
            {
                TenantId = await Session.GetTenantIdAsync(),
                RequestorUser = await Session.ToUserIdentifierAsync(),
                Body = "body",
                Subject = "subject",
                CellPhoneNumber = driver.CellPhoneNumber,
                DriverId = driver.Id,
                DriverFullName = $"{driver.FirstName} {driver.LastName}",
            });

            // Assert
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
            await _appNotifier.DidNotReceiveWithAnyArgs().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), Arg.Any<NotificationSeverity>());
        }

        [Fact]
        public async Task Test_SendMessage_should_send_error_notification_when_there_is_error_when_sending_sms()
        {
            // Arrange
            var driver = await CreateDriver("first", "last", _officeId, "+15005550001");
            driver = await UpdateEntity(driver, d => d.OrderNotifyPreferredFormat = OrderNotifyPreferredFormat.Sms);
            _smsSender.SendAsync(new SmsSendInput()).ThrowsForAnyArgs(new ApiException("error"));

            // Act
            await _driverMessageSmsSenderBackgroundJob.ExecuteAsync(new DriverMessageSmsSenderBackgroundJobArgs
            {
                TenantId = await Session.GetTenantIdAsync(),
                RequestorUser = await Session.ToUserIdentifierAsync(),
                Body = "body",
                Subject = "subject",
                CellPhoneNumber = driver.CellPhoneNumber,
                DriverId = driver.Id,
                DriverFullName = $"{driver.FirstName} {driver.LastName}",
            });

            // Assert
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
            await _appNotifier.Received().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), NotificationSeverity.Error);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
