using System.Threading.Tasks;
using Abp;
using Abp.BackgroundJobs;
using Abp.Notifications;
using Abp.UI;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.DriverMessages;
using DispatcherWeb.DriverMessages.Dto;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Notifications;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Twilio.Exceptions;
using Xunit;

namespace DispatcherWeb.Tests.DriverMessages
{
    public class DriverMessageAppService_Tests : AppTestBase, IAsyncLifetime
    {
        private ISmsSender _smsSender;
        private IAppNotifier _appNotifier;
        private IBackgroundJobManager _backgroundJobManager;
        private int _officeId;
        private IDriverMessageAppService _driverMessageAppService;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _smsSender = Substitute.For<ISmsSender>();
            _appNotifier = Substitute.For<IAppNotifier>();
            _backgroundJobManager = Substitute.For<IBackgroundJobManager>();
            _driverMessageAppService = Resolve<IDriverMessageAppService>(new
            {
                smsSender = _smsSender,
                appNotifier = _appNotifier,
                backgroundJobManager = _backgroundJobManager,
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
            await _driverMessageAppService.SendMessage(new SendMessageInput
            {
                DriverIds = new[] { driver.Id },
                Subject = "subject",
                Body = "body",
            });

            // Assert
            await _backgroundJobManager.ReceivedWithAnyArgs().EnqueueAsync<DriverMessageSmsSenderBackgroundJob, DriverMessageSmsSenderBackgroundJobArgs>(new DriverMessageSmsSenderBackgroundJobArgs());
            await _appNotifier.DidNotReceiveWithAnyArgs().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), Arg.Any<NotificationSeverity>());
        }

        [Fact]
        public async Task Test_SendMessage_should_successfully_send_sms_to_all_active_drivers()
        {
            // Arrange
            var driver = await CreateDriver("first", "last", _officeId, "+15005550055");
            driver = await UpdateEntity(driver, d => d.OrderNotifyPreferredFormat = OrderNotifyPreferredFormat.Sms);
            var driver2 = await CreateDriver("first2", "last2", _officeId, "+15005550056");
            driver2 = await UpdateEntity(driver2, d =>
            {
                d.OrderNotifyPreferredFormat = OrderNotifyPreferredFormat.Sms;
                d.IsInactive = true;
            });
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _driverMessageAppService.SendMessage(new SendMessageInput
            {
                OfficeIds = new[] { _officeId },
                Subject = "subject",
                Body = "body",
            });

            // Assert
            await _backgroundJobManager.Received().EnqueueAsync<DriverMessageSmsSenderBackgroundJob, DriverMessageSmsSenderBackgroundJobArgs>(Arg.Is<DriverMessageSmsSenderBackgroundJobArgs>(a =>
                a.Body == "body"
                && a.Subject == "subject"
                && a.CellPhoneNumber == driver.CellPhoneNumber
                && a.DriverId == driver.Id
                && a.DriverFullName == $"{driver.FirstName} {driver.LastName}"
            ));
            await _backgroundJobManager.DidNotReceive().EnqueueAsync<DriverMessageSmsSenderBackgroundJob, DriverMessageSmsSenderBackgroundJobArgs>(Arg.Is<DriverMessageSmsSenderBackgroundJobArgs>(a =>
                a.CellPhoneNumber == driver2.CellPhoneNumber
                || a.DriverId == driver2.Id
                || a.DriverFullName == $"{driver2.FirstName} {driver2.LastName}"
            ));
            await _appNotifier.DidNotReceiveWithAnyArgs().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), Arg.Any<NotificationSeverity>());
        }

        [Fact]
        public async Task Test_SendMessage_should_throw_Exception_when_Body_is_empty()
        {
            // Arrange
            var driver = await CreateDriver("first", "last", _officeId, "+15005550001");
            driver = await UpdateEntity(driver, d => d.OrderNotifyPreferredFormat = OrderNotifyPreferredFormat.Sms);
            _smsSender.SendAsync(new SmsSendInput()).ThrowsForAnyArgs(new ApiException("error"));

            // Act, Assert
            await _driverMessageAppService.SendMessage(new SendMessageInput
            {
                DriverIds = new[] { driver.Id },
                Subject = "subject",
                Body = " ",
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
