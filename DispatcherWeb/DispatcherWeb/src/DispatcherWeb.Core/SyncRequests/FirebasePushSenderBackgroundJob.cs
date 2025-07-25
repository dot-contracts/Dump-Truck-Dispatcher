using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Timing;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.WebPush;

namespace DispatcherWeb.SyncRequests
{
    public class FirebasePushSenderBackgroundJob : DispatcherWebAsyncBackgroundJobBase<FirebasePushSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IFirebasePushSender _firebasePushSender;
        private readonly IRepository<FcmPushMessage, Guid> _fcmPushMessageRepository;
        private readonly IRepository<FcmRegistrationToken> _fcmRegistrationTokenRepository;

        public FirebasePushSenderBackgroundJob(
            IExtendedAbpSession session,
            IFirebasePushSender firebasePushSender,
            IRepository<FcmPushMessage, Guid> fcmPushMessageRepository,
            IRepository<FcmRegistrationToken> fcmRegistrationTokenRepository
            ) : base(session)
        {
            _firebasePushSender = firebasePushSender;
            _fcmPushMessageRepository = fcmPushMessageRepository;
            _fcmRegistrationTokenRepository = fcmRegistrationTokenRepository;
        }

        public override async Task ExecuteAsync(FirebasePushSenderBackgroundJobArgs args)
        {
            await WithUnitOfWorkAsync(args.RequestorUser, async () =>
            {
                if (args.TenantId.HasValue)
                {
                    CurrentUnitOfWork.SetTenantId(args.TenantId);
                }

                var pushMessage = await _fcmPushMessageRepository.FirstOrDefaultAsync(x => x.Id == args.PushMessageGuid);
                try
                {
                    await _firebasePushSender.SendAsync(args.RegistrationToken, args.JsonPayload);
                    if (pushMessage != null)
                    {
                        pushMessage.SentAtDateTime = Clock.Now;
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message, ex);
                    if (pushMessage != null)
                    {
                        pushMessage.Error = (ex.Message ?? ex.ToString())?.Truncate(EntityStringFieldLengths.FcmPushMessage.Error);
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    //await _appNotifier.SendMessageAsync(args.RequestorUser, "Sending sync request failed", NotificationSeverity.Error);
                    if (ex is not FirebaseAdmin.Messaging.FirebaseMessagingException)
                    {
                        throw;
                    }
                    else
                    {
                        if (ex.Message?.IsIn("Requested entity was not found.", "The registration token is not a valid FCM registration token") == true)
                        {
                            Logger.Info($"Removing registration token {args.RegistrationToken.Id} because it was rejected by FCM");
                            await _fcmRegistrationTokenRepository.DeleteAsync(x => x.Id == args.RegistrationToken.Id);
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }
                }
            });
        }
    }
}
