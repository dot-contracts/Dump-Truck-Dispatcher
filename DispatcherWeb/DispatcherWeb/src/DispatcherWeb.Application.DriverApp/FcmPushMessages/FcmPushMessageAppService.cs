using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.DriverApp.FcmPushMessages.Dto;
using DispatcherWeb.WebPush;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.FcmPushMessages
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp)]
    public class FcmPushMessageAppService : DispatcherWebDriverAppAppServiceBase, IFcmPushMessageAppService
    {
        private readonly IRepository<FcmPushMessage, Guid> _fcmPushMessageRepository;

        public FcmPushMessageAppService(
            IRepository<FcmPushMessage, Guid> fcmPushMessageRepository
            )
        {
            _fcmPushMessageRepository = fcmPushMessageRepository;
        }

        public async Task<IListResult<FcmPushMessageDto>> Get(GetInput input)
        {
            var result = await (await _fcmPushMessageRepository.GetQueryAsync())
                .Where(x => x.ReceiverUserId == Session.UserId && x.InvalidatedAtDateTime == null)
                .WhereIf(!input.Token.IsNullOrEmpty(), x => x.FcmRegistrationToken.Token == input.Token)
                .WhereIf(input.Guid.HasValue, x => x.Id == input.Guid)
                .WhereIf(input.Received == false, x => !x.ReceivedAtDateTime.HasValue)
                .WhereIf(input.Received == true, x => x.ReceivedAtDateTime.HasValue)
                .Select(x => new FcmPushMessageDto
                {
                    Guid = x.Id,
                    JsonPayload = x.JsonPayload,
                    SentAtDateTime = x.SentAtDateTime,
                    ReceivedAtDateTime = x.ReceivedAtDateTime,
                })
                .ToListAsync(CancellationTokenProvider.Token);

            return new ListResultDto<FcmPushMessageDto>(result);
        }

        public async Task MarkAsReceived(MarkAsReceivedInput input)
        {
            if (input.Guids == null || !input.Guids.Any())
            {
                throw new UserFriendlyException("Guids parameter is required");
            }

            var pushMessages = await (await _fcmPushMessageRepository.GetQueryAsync())
                //.Where(x => x.ReceiverUserId == Session.UserId)
                .Where(x => input.Guids.Contains(x.Id) && x.ReceivedAtDateTime == null)
                .ToListAsync(CancellationTokenProvider.Token);

            var now = Clock.Now;
            foreach (var pushMessage in pushMessages)
            {
                if (pushMessage.ReceiverUserId != Session.UserId)
                {
                    pushMessage.InvalidatedAtDateTime = now;
                }
                pushMessage.ReceivedAtDateTime = now;
            }
        }
    }
}
