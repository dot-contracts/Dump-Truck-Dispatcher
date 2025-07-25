using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.DriverApp.FcmRegistrationTokens.Dto;
using DispatcherWeb.WebPush;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.FcmRegistrationTokens
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp)]
    public class FcmRegistrationTokenAppService : DispatcherWebDriverAppAppServiceBase, IFcmRegistrationTokenAppService
    {
        private readonly IRepository<FcmRegistrationToken> _fcmRegistrationTokenRepository;
        private readonly IRepository<FcmPushMessage, Guid> _fcmPushMessageRepository;

        public FcmRegistrationTokenAppService(
            IRepository<FcmRegistrationToken> fcmRegistrationTokenRepository,
            IRepository<FcmPushMessage, Guid> fcmPushMessageRepository
            )
        {
            _fcmRegistrationTokenRepository = fcmRegistrationTokenRepository;
            _fcmPushMessageRepository = fcmPushMessageRepository;
        }

        public async Task Post(PostInput input)
        {
            if (input.MobilePlatform == 0)
            {
                throw new UserFriendlyException("MobilePlatform is required");
            }

            var userId = AbpSession.GetUserId();
            var existingTokens = await (await _fcmRegistrationTokenRepository.GetQueryAsync())
                .Where(x => x.Token == input.Token)
                .ToListAsync(CancellationTokenProvider.Token);
            var matchingToken = existingTokens.FirstOrDefault(x => x.UserId == userId);
            var otherTokens = existingTokens.Where(x => x.UserId != userId).ToList();

            if (matchingToken != null)
            {
                matchingToken.LastModificationTime = Clock.Now;
                matchingToken.LastModifierUserId = userId;
                matchingToken.MobilePlatform = input.MobilePlatform;
                matchingToken.Version = input.Version;
            }
            else
            {
                await _fcmRegistrationTokenRepository.InsertAsync(new FcmRegistrationToken
                {
                    UserId = userId,
                    TenantId = await AbpSession.GetTenantIdOrNullAsync(),
                    Token = input.Token,
                    MobilePlatform = input.MobilePlatform,
                    Version = input.Version,
                });
            }

            if (otherTokens.Any())
            {
                var otherTokenIds = otherTokens.Select(x => x.Id).ToArray();
                await InvalidateSentButUndeliveredMessages(otherTokenIds);
                await _fcmRegistrationTokenRepository.DeleteRangeAsync(otherTokens);
            }
        }

        public async Task Delete(DeleteInput input)
        {
            var userId = AbpSession.GetUserId();
            var existingToken = await (await _fcmRegistrationTokenRepository.GetQueryAsync())
                .Where(x => x.UserId == userId && x.Token == input.Token)
                .FirstOrDefaultAsync(CancellationTokenProvider.Token);

            if (existingToken == null)
            {
                return;
            }

            await InvalidateSentButUndeliveredMessages(existingToken.Id);

            await _fcmRegistrationTokenRepository.DeleteAsync(existingToken);
        }

        private async Task InvalidateSentButUndeliveredMessages(params int[] fcmRegistrationTokenIds)
        {
            var undeliveredMessages = await (await _fcmPushMessageRepository.GetQueryAsync())
                .Where(x => x.FcmRegistrationTokenId.HasValue
                    && fcmRegistrationTokenIds.Contains(x.FcmRegistrationTokenId.Value)
                    && x.SentAtDateTime != null
                    && x.ReceivedAtDateTime == null
                    && x.InvalidatedAtDateTime == null)
                .ToListAsync(CancellationTokenProvider.Token);
            undeliveredMessages.ForEach(x => x.InvalidatedAtDateTime = Clock.Now);
        }
    }
}
