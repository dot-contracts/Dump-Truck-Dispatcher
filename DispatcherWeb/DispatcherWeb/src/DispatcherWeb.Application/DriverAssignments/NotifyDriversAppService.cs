using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Notifications;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Configuration;
using DispatcherWeb.DriverAssignments.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Notifications;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverAssignments
{
    [AbpAuthorize(AppPermissions.Pages_DriverAssignment)]
    public class NotifyDriversAppService : DispatcherWebAppServiceBase, INotifyDriversAppService
    {
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IAppNotifier _appNotifier;

        public NotifyDriversAppService(
            IRepository<DriverAssignment> driverAssignmentRepository,
            IBackgroundJobManager backgroundJobManager,
            IAppNotifier appNotifier
        )
        {
            _driverAssignmentRepository = driverAssignmentRepository;
            _backgroundJobManager = backgroundJobManager;
            _appNotifier = appNotifier;
        }

        public async Task<bool> NotifyDrivers(NotifyDriversInput input)
        {
            var driversToNofify = await GetDriversToNotify(input);

            bool result = true;
            foreach (var driver in driversToNofify)
            {
                result = await SendEmailToDriver(driver) && result;

                result = await SendSmsToDriver(driver) && result;
            }
            return result;
        }

        private async Task<List<NotifyDriverDto>> GetDriversToNotify(NotifyDriversInput input)
        {
            var drivers = await (await GetDriverAssignmentQueryAsync(input))
                .Select(da => new NotifyDriverDto
                {
                    TruckCode = da.Truck.TruckCode,
                    DriverFullName = da.Driver.FirstName + " " + da.Driver.LastName,
                    StartTime = da.StartTime,
                    Date = da.Date,
                    OrderNotifyPreferredFormat = da.Driver.OrderNotifyPreferredFormat,
                    EmailAddress = da.Driver.EmailAddress,
                    CellPhoneNumber = da.Driver.CellPhoneNumber,
                })
                .ToListAsync();

            var timezone = await GetTimezone();
            drivers.ForEach(x => x.StartTime = x.StartTime?.ConvertTimeZoneTo(timezone));

            return drivers;
        }

        private async Task<bool> SendEmailToDriver(NotifyDriverDto driver)
        {
            bool success = true;
            if (driver.OrderNotifyPreferredFormat.HasFlag(OrderNotifyPreferredFormat.Email))
            {
                if (driver.EmailAddress.IsNullOrEmpty())
                {
                    await _appNotifier.SendMessageAsync(
                        await Session.ToUserIdentifierAsync(),
                        $"An email to {driver.DriverFullName} for {driver.Date.ToShortDateString()} wasn't sent because the driver is missing an email address.",
                        NotificationSeverity.Warn
                    );
                    return false;
                }
                else
                {
                    await _backgroundJobManager.EnqueueAsync<EmailSenderBackgroundJob, EmailSenderBackgroundJobArgs>(new EmailSenderBackgroundJobArgs
                    {
                        RequestorUser = await Session.ToUserIdentifierAsync(),
                        EmailInputs = new List<EmailSenderBackgroundJobArgsEmail>
                        {
                            new EmailSenderBackgroundJobArgsEmail
                            {
                                ToEmailAddress = driver.EmailAddress,
                                Subject = "Start Time",
                                Body = await GetDriverStartTimeTemplate(driver),
                                ContactName = driver.DriverFullName,
                            },
                        },
                    });
                }
            }
            return success;
        }

        private async Task<bool> SendSmsToDriver(NotifyDriverDto driver)
        {
            bool success = true;
            if (driver.OrderNotifyPreferredFormat.HasFlag(OrderNotifyPreferredFormat.Sms))
            {
                if (driver.CellPhoneNumber.IsNullOrEmpty())
                {
                    await _appNotifier.SendMessageAsync(
                        await Session.ToUserIdentifierAsync(),
                        $"An SMS to {driver.DriverFullName} for {driver.Date.ToShortDateString()} wasn't sent because the driver is missing a cell phone number.",
                        NotificationSeverity.Warn
                    );
                    success = false;
                }
                else
                {
                    await _backgroundJobManager.EnqueueAsync<SmsSenderBackgroundJob, SmsSenderBackgroundJobArgs>(new SmsSenderBackgroundJobArgs
                    {
                        RequestorUser = await Session.ToUserIdentifierAsync(),
                        SmsInputs = new List<SmsSendInput>
                        {
                            new SmsSendInput
                            {
                                ToPhoneNumber = driver.CellPhoneNumber,
                                Body = await GetDriverStartTimeTemplate(driver),
                                ContactName = driver.DriverFullName,
                            },
                        },
                    });
                }
            }
            return success;
        }

        private async Task<string> GetDriverStartTimeTemplate(NotifyDriverDto notifyDriverDto)
        {
            var template = await SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DriverStartTimeTemplate);
            return template.ReplaceTokens(
                new Dictionary<string, string>
                {
                        { TemplateTokens.Truck, notifyDriverDto.TruckCode },
                        { TemplateTokens.Driver, notifyDriverDto.DriverFullName },
                        { TemplateTokens.StartTime, notifyDriverDto.StartTime?.ToString("t") ?? "" },
                        { TemplateTokens.StartDate, notifyDriverDto.Date.ToShortDateString() },
                }
            );
        }

        private async Task<IQueryable<DriverAssignment>> GetDriverAssignmentQueryAsync(NotifyDriversInput input)
        {
            return (await _driverAssignmentRepository.GetQueryAsync())
                .WhereIf(input.OfficeId.HasValue, da => da.OfficeId == input.OfficeId)
                .Where(da => da.DriverId.HasValue
                    && da.Date == input.Date
                    && da.Shift == input.Shift
                    && da.StartTime.HasValue
                    && da.Driver.OrderNotifyPreferredFormat != OrderNotifyPreferredFormat.Neither
                );
        }

        public async Task<bool> ThereAreDriversToNotify(NotifyDriversInput input)
        {
            return await (await GetDriverAssignmentQueryAsync(input)).AnyAsync();
        }

        private static class TemplateTokens
        {
            public const string Truck = "{Truck}";
            public const string Driver = "{Driver}";
            public const string StartTime = "{StartTime}";
            public const string StartDate = "{StartDate}";
        }
    }
}
