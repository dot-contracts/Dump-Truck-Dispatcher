using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.LeaseHaulerRequests.Dto;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Notifications;
using DispatcherWeb.Url;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulerRequests
{
    public class LeaseHaulerNotifier : DispatcherWebDomainServiceBase, ILeaseHaulerNotifier
    {
        public IAppUrlService AppUrlService { get; set; }

        private readonly IRepository<LeaseHaulerContact> _leaseHaulerContactRepository;
        private readonly IRepository<LeaseHauler> _leaseHaulerRepository;
        private readonly IWebUrlService _webUrlService;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IAppNotifier _appNotifier;

        public LeaseHaulerNotifier(
            IRepository<LeaseHaulerContact> leaseHaulerContactRepository,
            IRepository<LeaseHauler> leaseHaulerRepository,
            IWebUrlService webUrlService,
            IBackgroundJobManager backgroundJobManager,
            IAppNotifier appNotifier
        )
        {
            _leaseHaulerContactRepository = leaseHaulerContactRepository;
            _leaseHaulerRepository = leaseHaulerRepository;
            _webUrlService = webUrlService;
            _backgroundJobManager = backgroundJobManager;
            _appNotifier = appNotifier;
            AppUrlService = NullAppUrlService.Instance;
        }

        public async Task NotifyLeaseHaulerDispatchers(NotifyLeaseHaulerInput input)
        {
            var leaseHaulerDispatcherContacts = await GetContacts(new int[] { input.LeaseHaulerId });
            if (leaseHaulerDispatcherContacts.Count == 0)
            {
                await NotifyLeaseHaulerHasNoDispatcherContactsError(input.LeaseHaulerId);
                return;
            }

            foreach (var leaseHaulerDispatcherContact in leaseHaulerDispatcherContacts)
            {
                var companyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);
                var linkToRequest = GetAvailableTrucksUrl(input.LeaseHaulerRequestGuid);
                var linkToSchedule = AppUrlService.CreateLinkToSchedule(leaseHaulerDispatcherContact.TenantId);
                var message = input.Message
                    .Replace("{CompanyName}", companyName)
                    .Replace("{LinkToRequest}", linkToRequest)
                    .Replace("{LinkToSchedule}", linkToSchedule);

                await SendMessageToContacts(new List<SendLeaseHaulerContactDto> { leaseHaulerDispatcherContact }, message);
            }
        }

        public async Task<List<SendLeaseHaulerContactDto>> GetContacts(int[] leaseHaulerIds)
        {
            return await (await _leaseHaulerContactRepository.GetQueryAsync())
                .Where(lhc => leaseHaulerIds.Contains(lhc.LeaseHaulerId) && lhc.IsDispatcher)
                .Select(lhc => new SendLeaseHaulerContactDto
                {
                    LeaseHaulerId = lhc.LeaseHaulerId,
                    Id = lhc.Id,
                    Name = lhc.Name,
                    Email = lhc.Email,
                    CellPhoneNumber = lhc.CellPhoneNumber,
                    NotifyPreferredFormat = lhc.NotifyPreferredFormat,
                    TenantId = lhc.TenantId,
                })
                .ToListAsync();
        }

        public string GetAvailableTrucksUrl(Guid guid)
        {
            string siteUrl = _webUrlService.GetSiteRootAddress();
            return $"{siteUrl}app/leasehaulerrequests/availabletrucks/{guid.ToShortGuid()}";
        }

        public async Task SendMessageToContacts(List<SendLeaseHaulerContactDto> contacts, string message)
        {
            var emailContacts = contacts.Where(x => x.NotifyPreferredFormat.HasFlag(OrderNotifyPreferredFormat.Email)).ToList();
            var smsContacts = contacts.Where(x => x.NotifyPreferredFormat.HasFlag(OrderNotifyPreferredFormat.Sms)).ToList();

            if (emailContacts.Any())
            {
                await _backgroundJobManager.EnqueueAsync<EmailSenderBackgroundJob, EmailSenderBackgroundJobArgs>(new EmailSenderBackgroundJobArgs
                {
                    RequestorUser = await Session.ToUserIdentifierAsync(),
                    EmailInputs = emailContacts.Select(x => new EmailSenderBackgroundJobArgsEmail
                    {
                        ToEmailAddress = x.Email,
                        Subject = "Request Lease Haulers",
                        Body = message,
                        ContactName = x.Name,
                    }).ToList(),
                });
            }

            if (smsContacts.Any())
            {
                await _backgroundJobManager.EnqueueAsync<SmsSenderBackgroundJob, SmsSenderBackgroundJobArgs>(new SmsSenderBackgroundJobArgs
                {
                    RequestorUser = await Session.ToUserIdentifierAsync(),
                    SmsInputs = smsContacts.Select(x => new SmsSendInput
                    {
                        ToPhoneNumber = x.CellPhoneNumber,
                        Body = message,
                        ContactName = x.Name,
                    }).ToList(),
                });
            }
        }

        public async Task NotifyLeaseHaulerHasNoDispatcherContactsError(int leaseHaulerId)
        {
            var leaseHaulerName = await (await _leaseHaulerRepository.GetQueryAsync())
                .Where(lh => lh.Id == leaseHaulerId)
                .Select(lh => lh.Name)
                .FirstAsync();
            await _appNotifier.SendMessageAsync(
                await Session.ToUserIdentifierAsync(),
                $"There is no dispatcher contact for lease hauler {leaseHaulerName}.",
                Abp.Notifications.NotificationSeverity.Error
            );
        }
    }
}
