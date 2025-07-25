using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Net.Mail;
using Abp.Notifications;
using Abp.Timing;
using DispatcherWeb.Drivers;
using DispatcherWeb.Emailing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.BackgroundJobs
{
    public class DriverMessageEmailSenderBackgroundJob : DispatcherWebAsyncBackgroundJobBase<DriverMessageEmailSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IRepository<DriverMessage> _driverMessageRepository;
        private readonly ITrackableEmailSender _trackableEmailSender;
        private readonly IAppNotifier _appNotifier;

        public DriverMessageEmailSenderBackgroundJob(
            IRepository<DriverMessage> driverMessageRepository,
            ITrackableEmailSender trackableEmailSender,
            IExtendedAbpSession session,
            IAppNotifier appNotifier
            ) : base(session)
        {
            _driverMessageRepository = driverMessageRepository;
            _trackableEmailSender = trackableEmailSender;
            _appNotifier = appNotifier;
        }

        public override async Task ExecuteAsync(DriverMessageEmailSenderBackgroundJobArgs args)
        {
            try
            {
                using (Session.Use(args.RequestorUser.TenantId, args.RequestorUser.UserId))
                {
                    var message = new MailMessage(await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromAddress), args.EmailAddress)
                    {
                        Subject = args.Subject,
                        Body = args.Body,
                        IsBodyHtml = false,
                    };
                    var trackableEmailId = await _trackableEmailSender.SendTrackableAsync(message);

                    await WithUnitOfWorkAsync(args.RequestorUser, async () =>
                    {
                        var driverMessage = new DriverMessage
                        {
                            TimeSent = Clock.Now,
                            DriverId = args.DriverId,
                            MessageType = DriverMessageType.Email,
                            Subject = args.Subject,
                            Body = args.Body.Truncate(EntityStringFieldLengths.DriverMessage.Body),
                            TrackableEmailId = trackableEmailId,
                        };

                        await _driverMessageRepository.InsertAsync(driverMessage);
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in DriverMessageEmailSenderBackgroundJob: " + ex.Message, ex);
                await WithUnitOfWorkAsync(args.RequestorUser, async () =>
                {
                    await _appNotifier.SendMessageAsync(args.RequestorUser, $"Failed to send the message to {args.DriverFullName} ({args.EmailAddress})", NotificationSeverity.Error);
                });
            }
        }
    }
}
