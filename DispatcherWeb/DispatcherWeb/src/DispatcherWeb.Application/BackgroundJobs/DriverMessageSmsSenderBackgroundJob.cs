using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Notifications;
using Abp.Timing;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;
using Twilio.Exceptions;

namespace DispatcherWeb.BackgroundJobs
{
    public class DriverMessageSmsSenderBackgroundJob : DispatcherWebAsyncBackgroundJobBase<DriverMessageSmsSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IRepository<DriverMessage> _driverMessageRepository;
        private readonly ISmsSender _smsSender;
        private readonly IAppNotifier _appNotifier;

        public DriverMessageSmsSenderBackgroundJob(
            IRepository<DriverMessage> driverMessageRepository,
            ISmsSender smsSender,
            IExtendedAbpSession session,
            IAppNotifier appNotifier
        ) : base(session)
        {
            _driverMessageRepository = driverMessageRepository;
            _smsSender = smsSender;
            _appNotifier = appNotifier;
        }

        public override async Task ExecuteAsync(DriverMessageSmsSenderBackgroundJobArgs args)
        {
            try
            {
                using (Session.Use(args.RequestorUser))
                {
                    SmsSendResult smsSendResult;
                    try
                    {
                        smsSendResult = await _smsSender.SendAsync(new SmsSendInput
                        {
                            ToPhoneNumber = args.CellPhoneNumber,
                            Body = args.Body,
                            TrackStatus = true,
                        });
                    }
                    catch (ApiException e)
                    {
                        Logger.Error(e.ToString);
                        //save the changes before sending the notification to avoid "Can not set TenantId to 0 for IMustHaveTenant entities!" exception
                        await WithUnitOfWorkAsync(args.RequestorUser, async () =>
                        {
                            await CurrentUnitOfWork.SaveChangesAsync();
                            await _appNotifier.SendMessageAsync(
                                args.RequestorUser,
                                $"Unable to send the message to {args.DriverFullName}. Details: {e.Message}",
                                NotificationSeverity.Error
                            );
                        });
                        return;
                    }

                    await WithUnitOfWorkAsync(args.RequestorUser, async () =>
                    {
                        var driverMessage = new DriverMessage
                        {
                            TimeSent = Clock.Now,
                            DriverId = args.DriverId,
                            MessageType = DriverMessageType.Sms,
                            Subject = args.Subject?.Truncate(EntityStringFieldLengths.DriverMessage.Subject),
                            Body = args.Body?.Truncate(EntityStringFieldLengths.DriverMessage.Body),
                            SentSmsId = smsSendResult.SentSmsEntity?.Id,
                        };

                        await _driverMessageRepository.InsertAsync(driverMessage);
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in DriverMessageSmsSenderBackgroundJob: " + ex.Message, ex);
                await WithUnitOfWorkAsync(args.RequestorUser, async () =>
                {
                    await _appNotifier.SendMessageAsync(args.RequestorUser, $"Unable to send the message to {args.DriverFullName}", NotificationSeverity.Error);
                });
            }
        }
    }
}
