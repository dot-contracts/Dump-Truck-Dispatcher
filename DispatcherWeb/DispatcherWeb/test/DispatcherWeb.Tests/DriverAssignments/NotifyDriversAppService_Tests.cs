using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp;
using Abp.Net.Mail;
using Abp.Notifications;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.DriverAssignments;
using DispatcherWeb.DriverAssignments.Dto;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Notifications;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.DriverAssignments
{
    public class NotifyDriversAppService_Tests : AppTestBase, IAsyncLifetime
    {
        private int _officeId;
        private INotifyDriversAppService _notifyDriversAppService;
        private ISmsSender _smsSender;
        private IAppNotifier _appNotifier;
        private IEmailSender _emailSender;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _smsSender = Substitute.For<ISmsSender>();
            _emailSender = Substitute.For<IEmailSender>();
            _appNotifier = Substitute.For<IAppNotifier>();
            _notifyDriversAppService = Resolve<INotifyDriversAppService>(new { smsSender = _smsSender, emailSender = _emailSender, appNotifier = _appNotifier });
            SubstituteServiceDependencies(_notifyDriversAppService);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_NotifyDrivers_should_send_Email_and_Sms_when_there_is_DriverAssignment()
        {
            // Arrange
            var date = Clock.Now.Date;
            var startTime = date.AddHours(3).AddMinutes(23).AddSeconds(55);
            var phoneNumber = "+15005550011";
            var email = "test@test.com";
            await Arrange(date, Shift.Shift1, startTime, phoneNumber, email, OrderNotifyPreferredFormat.Both);

            // Act
            var result = await _notifyDriversAppService.NotifyDrivers(new NotifyDriversInput
            {
                Date = date,
                OfficeId = _officeId,
                Shift = Shift.Shift1,
            });

            // Assert
            result.ShouldBeTrue();
            var messageText = $"101|first last|{startTime:t}|{date:d}";
            await _appNotifier.DidNotReceiveWithAnyArgs().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), Arg.Any<NotificationSeverity>());
            await _smsSender.Received().SendAsync(new SmsSendInput { Body = messageText, ToPhoneNumber = phoneNumber });
            await _emailSender.Received().SendAsync(Arg.Is<MailMessage>(mm =>
                mm.Subject == "Start Time"
                && mm.Body == messageText
                && mm.To.ToString() == email
            ));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_NotifyDrivers_should_send_2_Email_and_Sms_when_there_are_2_DriverAssignments()
        {
            // Arrange
            var date = Clock.Now.Date;
            var startTime = date.AddHours(3).AddMinutes(23).AddSeconds(55);
            var phoneNumber = "+15005550011";
            var email = "test@test.com";
            var truck = await CreateTruck(officeId: _officeId);
            var truck2 = await CreateTruck("102", officeId: _officeId);
            var driver = await CreateDriver("first", "last", _officeId, phoneNumber, email);
            driver = await UpdateEntity(driver, d => d.OrderNotifyPreferredFormat = OrderNotifyPreferredFormat.Both);
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, driver.Id);
            driverAssignment = await UpdateEntity(driverAssignment, da => da.StartTime = startTime);
            var driverAssignment2 = await CreateDriverAssignmentForTruck(_officeId, truck2.Id, date, Shift.Shift1, driver.Id);
            driverAssignment2 = await UpdateEntity(driverAssignment2, da => da.StartTime = startTime);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));
            SubstituteSetting(AppSettings.DispatchingAndMessaging.DriverStartTimeTemplate, "{Truck}|{Driver}|{StartTime}|{StartDate}");
            SubstituteSetting(EmailSettingNames.DefaultFromAddress, "from@test.com");

            // Act
            var result = await _notifyDriversAppService.NotifyDrivers(new NotifyDriversInput
            {
                Date = date,
                OfficeId = _officeId,
                Shift = Shift.Shift1,
            });

            // Assert
            result.ShouldBeTrue();
            var messageText = $"101|first last|{startTime:t}|{date:d}";
            var messageText2 = $"102|first last|{startTime:t}|{date:d}";
            await _appNotifier.DidNotReceiveWithAnyArgs().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), Arg.Any<NotificationSeverity>());
            await _smsSender.Received().SendAsync(new SmsSendInput
            {
                Body = messageText,
                ToPhoneNumber = phoneNumber,
            });
            await _smsSender.Received().SendAsync(new SmsSendInput
            {
                Body = messageText2,
                ToPhoneNumber = phoneNumber,
            });
            await _emailSender.Received().SendAsync(Arg.Is<MailMessage>(mm =>
                mm.Subject == "Start Time"
                && mm.Body == messageText
                && mm.To.ToString() == email
            ));
            await _emailSender.Received().SendAsync(Arg.Is<MailMessage>(mm =>
                mm.Subject == "Start Time"
                && mm.Body == messageText2
                && mm.To.ToString() == email
            ));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_NotifyDrivers_should_send_Sms_only_for_preferred_sms_when_there_is_DriverAssignment()
        {
            // Arrange
            var date = Clock.Now.Date;
            var startTime = date.AddHours(3).AddMinutes(23).AddSeconds(55);
            var phoneNumber = "+15005550011";
            var email = "test@test.com";
            await Arrange(date, Shift.Shift1, startTime, phoneNumber, email, OrderNotifyPreferredFormat.Sms);

            // Act
            var result = await _notifyDriversAppService.NotifyDrivers(new NotifyDriversInput
            {
                Date = date,
                OfficeId = _officeId,
                Shift = Shift.Shift1,
            });

            // Assert
            result.ShouldBeTrue();
            var messageText = $"101|first last|{startTime:t}|{date:d}";
            await _appNotifier.DidNotReceiveWithAnyArgs().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), Arg.Any<NotificationSeverity>());
            await _smsSender.Received().SendAsync(new SmsSendInput
            {
                Body = messageText,
                ToPhoneNumber = phoneNumber,
            });
            await _emailSender.DidNotReceiveWithAnyArgs().SendAsync(Arg.Any<MailMessage>());
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_NotifyDrivers_should_send_Email_only_for_preferred_email_when_there_is_DriverAssignment()
        {
            // Arrange
            var date = Clock.Now.Date;
            var startTime = date.AddHours(3).AddMinutes(23).AddSeconds(55);
            var phoneNumber = "+15005550011";
            var email = "test@test.com";
            await Arrange(date, Shift.Shift1, startTime, phoneNumber, email, OrderNotifyPreferredFormat.Email);

            // Act
            var result = await _notifyDriversAppService.NotifyDrivers(new NotifyDriversInput
            {
                Date = date,
                OfficeId = _officeId,
                Shift = Shift.Shift1,
            });

            // Assert
            result.ShouldBeTrue();
            var messageText = $"101|first last|{startTime:t}|{date:d}";
            await _appNotifier.DidNotReceiveWithAnyArgs().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), Arg.Any<NotificationSeverity>());
            await _smsSender.DidNotReceiveWithAnyArgs().SendAsync(new SmsSendInput
            {
                Body = messageText,
                ToPhoneNumber = phoneNumber,
            });
            await _emailSender.Received().SendAsync(Arg.Is<MailMessage>(mm =>
                mm.Subject == "Start Time"
                && mm.Body == messageText
                && mm.To.ToString() == email
            ));
        }

        [Fact]
        public async Task Test_NotifyDrivers_should_send_error_notifications_when_there_is_DriverAssignment_but_driver_has_no_phone_and_email()
        {
            // Arrange
            var date = Clock.Now.Date;
            var startTime = date.AddHours(3).AddMinutes(23).AddSeconds(55);
            var phoneNumber = "";
            var email = "";
            await Arrange(date, Shift.Shift1, startTime, phoneNumber, email, OrderNotifyPreferredFormat.Email);

            // Act
            var result = await _notifyDriversAppService.NotifyDrivers(new NotifyDriversInput
            {
                Date = date,
                OfficeId = _officeId,
                Shift = Shift.Shift1,
            });

            // Assert
            result.ShouldBeFalse();
            await _appNotifier.Received().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), Arg.Is<NotificationSeverity>(x => x == NotificationSeverity.Warn));
            await _smsSender.DidNotReceiveWithAnyArgs().SendAsync(new SmsSendInput
            {
                Body = "",
                ToPhoneNumber = phoneNumber,
            });
            await _emailSender.DidNotReceiveWithAnyArgs().SendAsync(Arg.Any<MailMessage>());
        }

        [Fact]
        public async Task Test_NotifyDrivers_should_not_send_Email_and_Sms_when_there_is_no_DriverAssignment_for_Date_and_Shift()
        {
            // Arrange
            var date = Clock.Now.Date;
            var startTime = date.AddHours(3).AddMinutes(23).AddSeconds(55);
            var phoneNumber = "+15005550011";
            var email = "test@test.com";
            await Arrange(date, Shift.Shift2, startTime, phoneNumber, email, OrderNotifyPreferredFormat.Both);
            await Arrange(date, null, startTime, phoneNumber, email, OrderNotifyPreferredFormat.Both);
            await Arrange(date.AddDays(1), Shift.Shift1, startTime, phoneNumber, email, OrderNotifyPreferredFormat.Both);

            // Act
            var result = await _notifyDriversAppService.NotifyDrivers(new NotifyDriversInput
            {
                Date = date,
                OfficeId = _officeId,
                Shift = Shift.Shift1,
            });

            // Assert
            result.ShouldBeTrue();
            var messageText = $"101|first last|{startTime:t}|{date:d}";
            await _appNotifier.DidNotReceiveWithAnyArgs().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), Arg.Any<NotificationSeverity>());
            await _smsSender.DidNotReceiveWithAnyArgs().SendAsync(new SmsSendInput
            {
                Body = messageText,
                ToPhoneNumber = phoneNumber,
            });
            await _emailSender.DidNotReceiveWithAnyArgs().SendAsync(Arg.Any<MailMessage>());
        }

        [Fact]
        public async Task Test_NotifyDrivers_should_not_send_Email_and_Sms_when_there_is_null_DriverAssignment_for_Date_and_Shift()
        {
            // Arrange
            var date = Clock.Now.Date;
            var startTime = date.AddHours(3).AddMinutes(23).AddSeconds(55);
            var phoneNumber = "+15005550011";
            var truck = await CreateTruck(officeId: _officeId);
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1, null);

            // Act
            var result = await _notifyDriversAppService.NotifyDrivers(new NotifyDriversInput
            {
                Date = date,
                OfficeId = _officeId,
                Shift = Shift.Shift1,
            });

            // Assert
            result.ShouldBeTrue();
            await _appNotifier.DidNotReceiveWithAnyArgs().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), Arg.Any<NotificationSeverity>());
            await _smsSender.DidNotReceiveWithAnyArgs().SendAsync(new SmsSendInput
            {
                Body = "",
                ToPhoneNumber = phoneNumber,
            });
            await _emailSender.DidNotReceiveWithAnyArgs().SendAsync(Arg.Any<MailMessage>());
        }

        [Fact]
        public async Task Test_NotifyDrivers_should_not_send_Email_and_Sms_when_there_is_DriverAssignment_for_Date_and_Shift_without_StartTime()
        {
            // Arrange
            var date = Clock.Now.Date;
            var phoneNumber = "+15005550011";
            var email = "test@test.com";
            await Arrange(date, Shift.Shift1, null, phoneNumber, email, OrderNotifyPreferredFormat.Both);

            // Act
            var result = await _notifyDriversAppService.NotifyDrivers(new NotifyDriversInput
            {
                Date = date,
                OfficeId = _officeId,
                Shift = Shift.Shift1,
            });

            // Assert
            result.ShouldBeTrue();
            await _appNotifier.DidNotReceiveWithAnyArgs().SendMessageAsync(Arg.Any<UserIdentifier>(), Arg.Any<string>(), Arg.Any<NotificationSeverity>());
            await _smsSender.DidNotReceiveWithAnyArgs().SendAsync(new SmsSendInput
            {
                Body = "",
                ToPhoneNumber = phoneNumber,
            });
            await _emailSender.DidNotReceiveWithAnyArgs().SendAsync(Arg.Any<MailMessage>());
        }

        private async Task Arrange(DateTime date, Shift? shift, DateTime? startTime, string phoneNumber, string email, OrderNotifyPreferredFormat orderNotifyPreferredFormat)
        {
            var truck = await CreateTruck(officeId: _officeId);
            var driver = await CreateDriver("first", "last", _officeId, phoneNumber, email);
            driver = await UpdateEntity(driver, d => d.OrderNotifyPreferredFormat = orderNotifyPreferredFormat);
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, shift, driver.Id);
            driverAssignment = await UpdateEntity(driverAssignment, da => da.StartTime = startTime);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));
            SubstituteSetting(AppSettings.DispatchingAndMessaging.DriverStartTimeTemplate, "{Truck}|{Driver}|{StartTime}|{StartDate}");
            SubstituteSetting(EmailSettingNames.DefaultFromAddress, "from@test.com");
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
