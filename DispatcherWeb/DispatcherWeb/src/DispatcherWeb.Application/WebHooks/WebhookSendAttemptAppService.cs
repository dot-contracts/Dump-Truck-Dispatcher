using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Webhooks;
using Abp.Webhooks.BackgroundWorker;
using DispatcherWeb.Authorization;
using DispatcherWeb.WebHooks.Dto;

namespace DispatcherWeb.WebHooks
{
    [AbpAuthorize(AppPermissions.Pages_Administration_Webhook_ListSendAttempts)]
    public class WebhookSendAttemptAppService : DispatcherWebAppServiceBase, IWebhookAttemptAppService
    {
        private readonly IWebhookSendAttemptStore _webhookSendAttemptStore;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IWebhookEventAppService _webhookEventAppService;
        private readonly IWebhookSubscriptionAppService _webhookSubscriptionAppService;
        private readonly IRepository<WebhookSubscriptionInfo, Guid> _subscriptionRepository;

        public WebhookSendAttemptAppService(
            IWebhookSendAttemptStore webhookSendAttemptStore,
            IBackgroundJobManager backgroundJobManager,
            IWebhookEventAppService webhookEventAppService,
            IWebhookSubscriptionAppService webhookSubscriptionAppService,
            IRepository<WebhookSubscriptionInfo, Guid> subscriptionRepository
            )
        {
            _webhookSendAttemptStore = webhookSendAttemptStore;
            _backgroundJobManager = backgroundJobManager;
            _webhookEventAppService = webhookEventAppService;
            _webhookSubscriptionAppService = webhookSubscriptionAppService;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<PagedResultDto<GetAllSendAttemptsOutput>> GetAllSendAttempts(GetAllSendAttemptsInput input)
        {
            if (string.IsNullOrEmpty(input.SubscriptionId))
            {
                throw new ArgumentNullException(nameof(input.SubscriptionId));
            }

            var list = await _webhookSendAttemptStore.GetAllSendAttemptsBySubscriptionAsPagedListAsync(
                await AbpSession.GetTenantIdOrNullAsync(),
                Guid.Parse(input.SubscriptionId),
                input.MaxResultCount,
                input.SkipCount
            );

            var mapped = list.Items
                .Select(x => new GetAllSendAttemptsOutput
                {
                    Id = x.Id,
                    WebhookEventId = x.WebhookEventId,
                    WebhookName = x.WebhookEvent.WebhookName,
                    Data = x.WebhookEvent.Data,
                    Response = x.Response,
                    ResponseStatusCode = x.ResponseStatusCode,
                    CreationTime = x.CreationTime,
                })
                .ToList();

            return new PagedResultDto<GetAllSendAttemptsOutput>(list.TotalCount, mapped);
        }

        public async Task<ListResultDto<GetAllSendAttemptsOfWebhookEventOutput>> GetAllSendAttemptsOfWebhookEvent(GetAllSendAttemptsOfWebhookEventInput input)
        {
            if (string.IsNullOrEmpty(input.Id))
            {
                throw new ArgumentNullException(nameof(input.Id));
            }

            var list = await _webhookSendAttemptStore.GetAllSendAttemptsByWebhookEventIdAsync(
                await AbpSession.GetTenantIdOrNullAsync(),
                Guid.Parse(input.Id)
            );

            var mappedList = list.Select(x => new GetAllSendAttemptsOfWebhookEventOutput
            {
                Id = x.Id,
                WebhookSubscriptionId = x.WebhookSubscriptionId,
                Response = x.Response,
                ResponseStatusCode = x.ResponseStatusCode,
                CreationTime = x.CreationTime,
                LastModificationTime = x.LastModificationTime,
            }).ToList();

            var subscriptionIds = list.Select(x => x.WebhookSubscriptionId).Distinct().ToArray();

            var subscriptionUrisDictionary = (await _subscriptionRepository.GetQueryAsync()).Where(subscription => subscriptionIds.Contains(subscription.Id))
                 .Select(subscription => new { subscription.Id, subscription.WebhookUri })
                 .ToDictionary(s => s.Id, s => s.WebhookUri);

            foreach (var output in mappedList)
            {
                output.WebhookUri = subscriptionUrisDictionary[output.WebhookSubscriptionId];
            }

            return new ListResultDto<GetAllSendAttemptsOfWebhookEventOutput>(mappedList);
        }

        [AbpAuthorize(AppPermissions.Pages_Administration_Webhook_ResendWebhook)]
        public async Task Resend(string sendAttemptId)
        {
            var webhookSendAttempt = await _webhookSendAttemptStore.GetAsync(await AbpSession.GetTenantIdOrNullAsync(), Guid.Parse(sendAttemptId));
            var webhookEvent = await _webhookEventAppService.Get(webhookSendAttempt.WebhookEventId.ToString());
            var webhookSubscription = await _webhookSubscriptionAppService.GetSubscription(webhookSendAttempt.WebhookSubscriptionId.ToString());

            await _backgroundJobManager.EnqueueAsync<WebhookSenderJob, WebhookSenderArgs>(new WebhookSenderArgs
            {
                TenantId = await AbpSession.GetTenantIdOrNullAsync(),
                WebhookEventId = webhookSendAttempt.WebhookEventId,
                WebhookSubscriptionId = webhookSendAttempt.WebhookSubscriptionId,
                Data = webhookEvent.Data,
                WebhookName = webhookEvent.WebhookName,
                WebhookUri = webhookSubscription.WebhookUri,
                Headers = webhookSubscription.Headers,
                Secret = webhookSubscription.Secret,
                TryOnce = true,
            });
        }
    }
}
