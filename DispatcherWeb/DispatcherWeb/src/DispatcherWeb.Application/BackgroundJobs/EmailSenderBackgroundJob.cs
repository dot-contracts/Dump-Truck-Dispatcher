using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Net.Mail;
using Abp.Notifications;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.BackgroundJobs
{
    public class EmailSenderBackgroundJob : DispatcherWebAsyncBackgroundJobBase<EmailSenderBackgroundJobArgs>, ITransientDependency
    {
        private readonly IAppNotifier _appNotifier;
        private readonly IEmailSender _emailSender;

        public EmailSenderBackgroundJob(
            IAppNotifier appNotifier,
            IExtendedAbpSession session,
            IEmailSender emailSender
            ) : base(session)
        {
            _appNotifier = appNotifier;
            _emailSender = emailSender;
        }

        public override async Task ExecuteAsync(EmailSenderBackgroundJobArgs args)
        {
            using (Session.Use(args.RequestorUser))
            {
                var result = await SendEmailBatches(args.EmailInputs);
                var errors = result.Where(x => x.Exception != null).ToList();
                if (errors.Any())
                {
                    await WithUnitOfWorkAsync(args.RequestorUser, async () =>
                    {
                        foreach (var (input, exception) in errors)
                        {
                            string detailedErrorMessage = $"Unable to send the message to {input.ToEmailAddress}.";
                            if (!string.IsNullOrEmpty(input.ContactName))
                            {
                                detailedErrorMessage = $"Unable to send the message for {input.ContactName} ({input.ToEmailAddress}).";
                            }
                            await _appNotifier.SendMessageAsync(
                                args.RequestorUser,
                                detailedErrorMessage,
                                NotificationSeverity.Error
                            );
                        }
                    });
                }
            }
        }

        private async Task<List<(EmailSenderBackgroundJobArgsEmail Input, Exception Exception)>> SendEmailBatches(List<EmailSenderBackgroundJobArgsEmail> inputs)
        {
            var result = new List<(EmailSenderBackgroundJobArgsEmail Input, Exception Exception)>();
            foreach (var input in inputs)
            {
                try
                {
                    await _emailSender.SendAsync(input.ToEmailAddress, input.Subject, input.Body, false);
                    result.Add((input, null));
                }
                catch (Exception ex)
                {
                    Logger.Error("Error during batch Email sending", ex);
                    result.Add((input, ex));
                }
            }
            return result;
        }
    }
}
