using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Encryption;
using Abp.Linq.Extensions;
using DispatcherWeb.Authorization;
using DispatcherWeb.Customers;
using DispatcherWeb.Emailing.Dto;
using DispatcherWeb.Invoices;
using DispatcherWeb.Notifications;
using DispatcherWeb.Orders;
using DispatcherWeb.Quotes;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Emailing
{
    [AbpAuthorize]
    public class EmailAppService : DispatcherWebAppServiceBase, IEmailAppService
    {
        private readonly IRepository<TrackableEmail, Guid> _emailRepository;
        private readonly IRepository<TrackableEmailEvent> _eventRepository;
        private readonly IRepository<TrackableEmailReceiver> _receiverRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Quote> _quoteRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IAppNotifier _appNotifier;

        public EmailAppService(
            IRepository<TrackableEmail, Guid> emailRepository,
            IRepository<TrackableEmailEvent> eventRepository,
            IRepository<TrackableEmailReceiver> receiverRepository,
            IRepository<Customer> customerRepository,
            IRepository<Quote> quoteRepository,
            IRepository<Order> orderRepository,
            IRepository<Invoice> invoiceRepository,
            IEncryptionService encryptionService,
            IAppNotifier appNotifier
            )
        {
            _emailRepository = emailRepository;
            _eventRepository = eventRepository;
            _receiverRepository = receiverRepository;
            _customerRepository = customerRepository;
            _quoteRepository = quoteRepository;
            _orderRepository = orderRepository;
            _invoiceRepository = invoiceRepository;
            _encryptionService = encryptionService;
            _appNotifier = appNotifier;
        }

        [AbpAllowAnonymous]
        public async Task TrackEvents(List<TrackEventInput> inputList)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
            {
                var emailIds = inputList.Select(x => x.TrackableEmailId).ToList();
                var emails = await (await _emailRepository.GetQueryAsync())
                    .Include(x => x.Receivers)
                    .Include(x => x.QuoteEmails)
                    .Include(x => x.OrderEmails)
                    .Include(x => x.InvoiceEmails)
                    .Include(x => x.CreatorUser)
                    .Where(x => emailIds.Contains(x.Id))
                    .ToListAsync();

                foreach (var input in inputList
                    .OrderBy(x => x.TrackableEmailId)
                    .ThenBy(x => x.Email)
                    .ThenBy(x => x.Timestamp)
                    .ThenBy(x => x.DeliveryStatus.GetOrder())
                    .ToList())
                {
                    var email = emails.FirstOrDefault(x => x.Id == input.TrackableEmailId && x.TenantId == input.TrackableEmailTenantId);
                    var receiver = email?.Receivers.FirstOrDefault(x => !string.IsNullOrEmpty(x.Email) && x.Email.Equals(input.Email, StringComparison.InvariantCultureIgnoreCase));

                    if (receiver != null)
                    {
                        if (receiver.DeliveryStatus == EmailDeliveryStatus.Opened)
                        {
                            continue;
                        }

                        receiver.DeliveryStatus = input.DeliveryStatus;

                        if (input.DeliveryStatus.IsFailed())
                        {
                            if (email.CreatorUser != null)
                            {
                                if (email.QuoteEmails.Any())
                                {
                                    var quoteId = email.QuoteEmails.First().QuoteId;
                                    var quote = await _quoteRepository.FirstOrDefaultAsync(quoteId);
                                    if (quote != null)
                                    {
                                        var user = email.CreatorUser.ToUserIdentifier();
                                        await _appNotifier.QuoteEmailDeliveryFailed(user, quote, receiver);
                                    }
                                }

                                if (email.OrderEmails.Any())
                                {
                                    var orderId = email.OrderEmails.First().OrderId;
                                    var order = await _orderRepository.FirstOrDefaultAsync(orderId);
                                    if (order != null)
                                    {
                                        var user = email.CreatorUser.ToUserIdentifier();
                                        await _appNotifier.OrderEmailDeliveryFailed(user, order, receiver);
                                    }
                                }

                                if (email.InvoiceEmails.Any())
                                {
                                    var invoiceId = email.InvoiceEmails.First().InvoiceId;
                                    var user = email.CreatorUser.ToUserIdentifier();
                                    await _appNotifier.InvoiceEmailDeliveryFailed(user, invoiceId, receiver);
                                }
                            }
                        }
                    }

                    var trackEvent = new TrackableEmailEvent
                    {
                        Email = input.Email,
                        EmailDeliveryStatus = input.DeliveryStatus,
                        Event = input.Event,
                        SendGridEventId = input.SendGridEventId,
                        FailReason = input.Reason,
                        SendGridEventTimestamp = input.Timestamp,
                        TrackableEmailId = email?.Id,
                        TrackableEmailReceiverId = receiver?.Id,
                    };
                    trackEvent.TruncateFieldsIfNeeded();
                    await _eventRepository.InsertAsync(trackEvent);
                }

                foreach (var email in emails)
                {
                    CalculateEmailDeliveryStatus(email);
                }
            }
        }

        private void CalculateEmailDeliveryStatus(TrackableEmail email)
        {
            email.CalculatedDeliveryStatus = email.Receivers
                                                 .Where(x => !x.IsSender)
                                                 .Select(x => x.DeliveryStatus)
                                                 .GetLowestStatus() ?? EmailDeliveryStatus.NotProcessed;
        }

        [AbpAllowAnonymous]
        public async Task TrackEmailOpen(TrackEmailOpenInput input)
        {
            input.ResolveParameters(_encryptionService);

            if (string.IsNullOrEmpty(input.Email)
                || !input.Id.HasValue)
            {
                return;
            }

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
            {
                var receiver = await (await _receiverRepository.GetQueryAsync())
                    .Include(x => x.TrackableEmail.Receivers)
                    .Include(x => x.TrackableEmail.InvoiceEmails)
                    .Where(x => x.TrackableEmailId == input.Id && x.Email == input.Email)
                    .FirstOrDefaultAsync();
                if (receiver == null || receiver.DeliveryStatus == EmailDeliveryStatus.Opened)
                {
                    return;
                }

                receiver.DeliveryStatus = EmailDeliveryStatus.Opened;

                //to keep track of the date when it was opened
                await _eventRepository.InsertAsync(new TrackableEmailEvent
                {
                    TrackableEmailId = input.Id,
                    TrackableEmailReceiverId = receiver.Id,
                    Email = input.Email,
                    EmailDeliveryStatus = EmailDeliveryStatus.Opened,
                });

                if (!receiver.IsSender && receiver.TrackableEmail.InvoiceEmails.Any())
                {
                    var invoiceId = receiver.TrackableEmail.InvoiceEmails.First().InvoiceId;
                    var invoice = await _invoiceRepository.FirstOrDefaultAsync(invoiceId);
                    if (invoice != null)
                    {
                        if (invoice.Status == InvoiceStatus.Viewed)
                        {
                            return;
                        }

                        invoice.Status = InvoiceStatus.Viewed;
                    }
                }

                CalculateEmailDeliveryStatus(receiver.TrackableEmail);
            }
        }

        [AbpAllowAnonymous]
        [RemoteService(false)]
        [UnitOfWork(IsDisabled = true)]
        public async Task<Guid> AddTrackableEmailAsync(MailMessage mail)
        {
            return await UnitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions { IsTransactional = false }, async () =>
            {
                var trackableEmail = await CreateTrackableEmailAsync(mail);
                var id = await _emailRepository.InsertAndGetIdAsync(trackableEmail);
                var receivers = mail.GetTrackableEmailReceivers(id).SetTenantId(await Session.GetTenantIdOrNullAsync());
                await _receiverRepository.InsertRangeAsync(receivers);
                return id;
            });
        }

        private async Task<TrackableEmail> CreateTrackableEmailAsync(MailMessage mail)
        {
            var email = new TrackableEmail
            {
                Subject = mail.Subject,
                TenantId = await Session.GetTenantIdOrNullAsync(),
            };

            email.TruncateFieldsIfNeeded();

            return email;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<GetEmailHistoryInput> GetEmailHistoryInput(GetEmailHistoryInput input)
        {
            if (input.QuoteId.HasValue)
            {
                var quote = await (await _quoteRepository.GetQueryAsync())
                    .Where(x => x.Id == input.QuoteId)
                    .Select(x => new
                    {
                        QuoteName = x.Name,
                        CustomerName = x.Customer.Name,
                        x.CustomerId,
                    })
                    .FirstOrDefaultAsync();

                input.CustomerId = quote.CustomerId;
                input.CustomerName = quote.CustomerName;
                input.QuoteName = quote.QuoteName;
            }
            else if (input.CustomerId.HasValue)
            {
                input.CustomerName = await (await _customerRepository.GetQueryAsync())
                    .Where(x => x.Id == input.CustomerId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync();
            }

            return input;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<PagedResultDto<EmailHistoryDto>> GetEmailHistory(GetEmailHistoryInput input)
        {
            var query = (await _receiverRepository.GetQueryAsync())
                .Include(x => x.TrackableEmail)
                .Include(x => x.Events)
                .Where(x => !x.IsSender)
                .WhereIf(input.CustomerId.HasValue, x => x.TrackableEmail.QuoteEmails.Any(q => q.Quote.CustomerId == input.CustomerId)
                                                        || x.TrackableEmail.OrderEmails.Any(o => o.Order.CustomerId == input.CustomerId))
                .WhereIf(input.QuoteId.HasValue, x => x.TrackableEmail.QuoteEmails.Any(q => q.QuoteId == input.QuoteId)
                                                        || x.TrackableEmail.OrderEmails.Any(o => o.Order.QuoteId == input.QuoteId))
                .WhereIf(input.OrderId.HasValue, x => x.TrackableEmail.OrderEmails.Any(o => o.OrderId == input.OrderId));

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new EmailHistoryDto
                {
                    EmailId = x.TrackableEmail.Id,
                    EmailSubject = x.TrackableEmail.Subject,
                    ReceiverId = x.Id,
                    ReceiverEmail = x.Email,
                    ReceiverKind = x.ReceiverKind,
                    EmailCreationTime = x.TrackableEmail.CreationTime,
                    EmailCreatorUserName = x.TrackableEmail.CreatorUser.Name + " " + x.TrackableEmail.CreatorUser.Surname,
                    ReceiverDeliveryStatus = x.DeliveryStatus,
                    Events = x.Events.Select(e => new EmailHistoryEventDto
                    {
                        Id = e.Id,
                        CreationTime = e.CreationTime,
                        EmailDeliveryStatus = e.EmailDeliveryStatus,
                    }).ToList(),
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<EmailHistoryDto>(
                totalCount,
                items);
        }
    }
}
