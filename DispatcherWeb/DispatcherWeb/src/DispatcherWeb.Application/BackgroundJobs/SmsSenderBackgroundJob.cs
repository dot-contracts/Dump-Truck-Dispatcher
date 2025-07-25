using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Notifications;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Sms;
using Twilio.Exceptions;

namespace DispatcherWeb.BackgroundJobs
{
    public class SmsSenderBackgroundJob : DispatcherWebAsyncBackgroundJobBase<SmsSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IAppNotifier _appNotifier;
        private readonly ISmsSender _smsSender;
        private readonly IRepository<SentSms> _sentSmsRepository;

        public SmsSenderBackgroundJob(
            IAppNotifier appNotifier,
            IExtendedAbpSession session,
            ISmsSender smsSender,
            IRepository<SentSms> sentSmsRepository
            ) : base(session)
        {
            _appNotifier = appNotifier;
            _smsSender = smsSender;
            _sentSmsRepository = sentSmsRepository;
        }

        public override async Task ExecuteAsync(SmsSenderBackgroundJobArgs args)
        {
            using (Session.Use(args.RequestorUser.TenantId, args.RequestorUser.UserId))
            {
                var resultList = await SendSmsBatches(args.SmsInputs);

                await WithUnitOfWorkAsync(args.RequestorUser, async () =>
                {
                    foreach (var sentSms in resultList
                        .Where(x => x.sendResult.SentSmsEntityIsInserted == false)
                        .Select(x => x.sendResult.SentSmsEntity)
                        .OfType<SentSms>()
                        .ToList())
                    {
                        await _sentSmsRepository.InsertAsync(sentSms);
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();

                    var errorList = resultList.Where(x => x.sendResult.ErrorCode.HasValue).ToList();
                    if (errorList.Any())
                    {
                        foreach (var (input, sendResult) in errorList)
                        {
                            string detailedErrorMessage = $"Unable to send the message to {input.ToPhoneNumber}. {sendResult.ErrorMessage}";
                            if (!string.IsNullOrEmpty(input.ContactName))
                            {
                                detailedErrorMessage = $"Unable to send the message for {input.ContactName} with phone number {input.ToPhoneNumber}. {sendResult.ErrorMessage}";
                            }
                            if (sendResult.ErrorCode == 21211)
                            {
                                detailedErrorMessage = $"An SMS wasn't sent to {input.ContactName} because they have a bad phone number ({input.ToPhoneNumber}).";
                            }
                            Logger.Error($"There was an error while sending the sms to contact {input.ContactName} {input.ToPhoneNumber}, {sendResult.ErrorMessage}");
                            await _appNotifier.SendMessageAsync(
                                args.RequestorUser,
                                detailedErrorMessage,
                                NotificationSeverity.Error
                            );
                        }
                    }

                    return true;
                });
            }
        }

        private async Task<List<(SmsSendInput input, SmsSendResult sendResult)>> SendSmsBatches(List<SmsSendInput> inputs)
        {
            var resultList = new List<(SmsSendInput input, SmsSendResult sendResult)>();
            foreach (var input in inputs)
            {
                try
                {
                    var result = await _smsSender.SendAsync(input);
                    resultList.Add((input, result));
                }
                catch (ApiException ex)
                {
                    Logger.Error("Error during batch sms sending", ex);
                    resultList.Add((input, new SmsSendResult(null, SmsStatus.Failed, ex.Code, ex.Message)));
                }
                catch (Exception ex)
                {
                    Logger.Error("Error during batch sms sending", ex);
                    resultList.Add((input, new SmsSendResult(null, SmsStatus.Failed, 0, ex.Message)));
                }
            }
            return resultList;
        }
    }
}
