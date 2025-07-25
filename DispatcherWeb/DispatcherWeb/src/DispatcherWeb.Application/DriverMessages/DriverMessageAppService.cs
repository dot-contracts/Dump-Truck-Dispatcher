using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.DriverMessages.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Sms.Dto;
using Microsoft.EntityFrameworkCore;
using Twilio.Exceptions;

namespace DispatcherWeb.DriverMessages
{
    [AbpAuthorize(AppPermissions.Pages_DriverMessages)]
    public class DriverMessageAppService : DispatcherWebAppServiceBase, IDriverMessageAppService
    {
        private const int MaxLengthOfBodyAndSubject = 50;

        private readonly IRepository<DriverMessage> _driverMessageRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly ISmsSender _smsSender;
        private readonly IBackgroundJobManager _backgroundJobManager;

        public DriverMessageAppService(
            IRepository<DriverMessage> driverMessageRepository,
            IRepository<Driver> driverRepository,
            ISmsSender smsSender,
            IBackgroundJobManager backgroundJobManager
        )
        {
            _driverMessageRepository = driverMessageRepository;
            _driverRepository = driverRepository;
            _smsSender = smsSender;
            _backgroundJobManager = backgroundJobManager;
        }

        public async Task<PagedResultDto<DriverMessageDto>> GetDriverMessagePagedList(GetDriverMessagePagedListInput input)
        {
            var timeZone = await GetTimezone();
            var utcDateTimeBegin = input.DateBegin?.ConvertTimeZoneFrom(timeZone);
            var utcDateTimeEnd = input.DateEnd?.AddDays(1).ConvertTimeZoneFrom(timeZone);

            var query = (await _driverMessageRepository.GetQueryAsync())
                .WhereIf(utcDateTimeBegin.HasValue, dm => dm.TimeSent >= utcDateTimeBegin.Value)
                .WhereIf(utcDateTimeEnd.HasValue, dm => dm.TimeSent < utcDateTimeEnd.Value)
                .WhereIf(input.OfficeId.HasValue, dm => dm.Driver.OfficeId == input.OfficeId.Value)
                .WhereIf(input.DriverId.HasValue, dm => dm.DriverId == input.DriverId.Value)
                .WhereIf(input.UserId.HasValue, dm => dm.CreatorUserId == input.UserId.Value)
                ;

            var totalCount = await query.CountAsync();

            var rawItems = await query
                .Select(dm => new
                {
                    dm.Id,
                    dm.TimeSent,
                    Driver = dm.Driver.FirstName + " " + dm.Driver.LastName,
                    SentBy = dm.CreatorUser.Name + " " + dm.CreatorUser.Surname,
                    dm.Subject,
                    dm.Body,
                    dm.MessageType,
                    EmailStatus = (int?)dm.TrackableEmail.CalculatedDeliveryStatus,
                    SmsStatus = dm.SentSms != null ? dm.SentSms.Status : 0,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();
            var items = rawItems.Select(dm => new DriverMessageDto
            {
                Id = dm.Id,
                TimeSent = dm.TimeSent,
                Driver = dm.Driver,
                SentBy = dm.SentBy,
                Subject = dm.Subject.TruncateWithPostfix(MaxLengthOfBodyAndSubject),
                Body = dm.Body.TruncateWithPostfix(MaxLengthOfBodyAndSubject),
                MessageType = dm.MessageType,
                EmailStatus = (EmailDeliveryStatus)(dm.EmailStatus ?? 0),
                SmsStatus = dm.SmsStatus,
            }).ToList();

            return new PagedResultDto<DriverMessageDto>(totalCount, items);
        }

        public async Task SendMessage(SendMessageInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Body))
            {
                throw new UserFriendlyException("Body text is required to send a message.");
            }

            var drivers = await (await _driverRepository.GetQueryAsync())
                .Where(d => !d.IsInactive && d.OrderNotifyPreferredFormat != OrderNotifyPreferredFormat.Neither)
                .WhereIf(input.DriverIds?.Any() == true, d => input.DriverIds.Contains(d.Id))
                .WhereIf(input.OfficeIds?.Any() == true, d => d.OfficeId.HasValue && input.OfficeIds.Contains(d.OfficeId.Value))
                .Select(d => new
                {
                    d.Id,
                    d.OrderNotifyPreferredFormat,
                    d.CellPhoneNumber,
                    d.EmailAddress,
                    FullName = d.FirstName + " " + d.LastName,
                })
                .ToListAsync();

            foreach (var driver in drivers)
            {
                if (driver.OrderNotifyPreferredFormat.HasFlag(OrderNotifyPreferredFormat.Email))
                {
                    await _backgroundJobManager.EnqueueAsync<DriverMessageEmailSenderBackgroundJob, DriverMessageEmailSenderBackgroundJobArgs>(new DriverMessageEmailSenderBackgroundJobArgs
                    {
                        TenantId = await Session.GetTenantIdAsync(),
                        RequestorUser = await Session.ToUserIdentifierAsync(),
                        Body = input.Body,
                        Subject = input.Subject,
                        EmailAddress = driver.EmailAddress,
                        DriverId = driver.Id,
                        DriverFullName = driver.FullName,
                    });
                }
                if (driver.OrderNotifyPreferredFormat.HasFlag(OrderNotifyPreferredFormat.Sms))
                {
                    await _backgroundJobManager.EnqueueAsync<DriverMessageSmsSenderBackgroundJob, DriverMessageSmsSenderBackgroundJobArgs>(new DriverMessageSmsSenderBackgroundJobArgs
                    {
                        TenantId = await Session.GetTenantIdAsync(),
                        RequestorUser = await Session.ToUserIdentifierAsync(),
                        Body = input.Body,
                        Subject = input.Subject,
                        CellPhoneNumber = driver.CellPhoneNumber,
                        DriverId = driver.Id,
                        DriverFullName = driver.FullName,
                    });
                }
            }
        }

        public async Task<DriverMessageViewDto> GetForView(int id)
        {
            return await (await _driverMessageRepository.GetQueryAsync())
                .Where(dm => dm.Id == id)
                .Select(dm => new DriverMessageViewDto
                {
                    Id = dm.Id,
                    Subject = dm.Subject,
                    Body = dm.Body,
                })
                .FirstAsync();
        }

        public async Task TestSmsNumber(TestSmsNumberInput input)
        {
            string body = $"Test message sent {(await GetToday()):G} from Dump Truck Dispatcher. If you are reading this, you entered a good SMS number in the settings.";
            try
            {
                await _smsSender.SendAsync(new SmsSendInput
                {
                    Body = body,
                    ToPhoneNumber = input.FullPhoneNumber,
                    TrackStatus = false,
                    DisallowFallbackToHostFromPhoneNumber = true,
                });
            }
            catch (ApiException e)
            {
                Logger.Error(e.ToString);
                throw new UserFriendlyException($"We were unable to send via the configured SMS number. Please check the phone number. If that number is good, contact tech support to have them check the Twilio number configuration. \nDetails: {e.Message}");
            }
        }

    }
}
