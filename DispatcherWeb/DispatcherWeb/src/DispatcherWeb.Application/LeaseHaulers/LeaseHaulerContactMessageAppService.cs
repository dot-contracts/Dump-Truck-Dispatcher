using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.LeaseHaulers.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulers
{
    [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_Edit)]
    public class LeaseHaulerContactMessageAppService : DispatcherWebAppServiceBase
    {
        private readonly IRepository<LeaseHaulerContact> _leaseHaulerContactRepository;
        private readonly IBackgroundJobManager _backgroundJobManager;

        public LeaseHaulerContactMessageAppService(
            IRepository<LeaseHaulerContact> leaseHaulerContactRepository,
            IBackgroundJobManager backgroundJobManager
        )
        {
            _leaseHaulerContactRepository = leaseHaulerContactRepository;
            _backgroundJobManager = backgroundJobManager;
        }

        public async Task SendMessage(SendMessageInput input)
        {
            if (input.Body.IsNullOrWhiteSpace())
            {
                throw new UserFriendlyException("Body text is required to send a message.");
            }

            var leaseHaulerContactQuery = await _leaseHaulerContactRepository.GetQueryAsync();
            leaseHaulerContactQuery = leaseHaulerContactQuery.Where(d => input.ContactIds.Contains(d.Id));
            var contacts = await leaseHaulerContactQuery
                .Select(c => new
                {
                    Id = c.Id,
                    PhoneNumber = c.CellPhoneNumber,
                    Email = c.Email,
                    FullName = c.Name,
                })
                .ToListAsync();

            switch (input.MessageType)
            {
                case LeaseHaulerMessageType.Sms:
                    await _backgroundJobManager.EnqueueAsync<SmsSenderBackgroundJob, SmsSenderBackgroundJobArgs>(new SmsSenderBackgroundJobArgs
                    {
                        RequestorUser = await Session.ToUserIdentifierAsync(),
                        SmsInputs = contacts.Select(x => new SmsSendInput
                        {
                            ToPhoneNumber = x.PhoneNumber,
                            Body = input.Body,
                            ContactName = x.FullName,
                        }).ToList(),
                    });
                    break;

                case LeaseHaulerMessageType.Email:
                    await _backgroundJobManager.EnqueueAsync<EmailSenderBackgroundJob, EmailSenderBackgroundJobArgs>(new EmailSenderBackgroundJobArgs
                    {
                        RequestorUser = await Session.ToUserIdentifierAsync(),
                        EmailInputs = contacts.Select(x => new EmailSenderBackgroundJobArgsEmail
                        {
                            ToEmailAddress = x.Email,
                            Subject = input.Subject,
                            Body = input.Body,
                            ContactName = x.FullName,
                        }).ToList(),
                    });
                    break;

                default:
                    throw new ArgumentException($"Wrong LeaseHaulerMessageType: {input.MessageType}");
            }
        }


    }
}
