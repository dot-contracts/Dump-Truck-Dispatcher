using System;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Notifications;
using DispatcherWeb.Invoices;
using DispatcherWeb.Notifications;
using DispatcherWeb.Runtime.Session;

namespace DispatcherWeb.BackgroundJobs
{
    public class EmailApprovedInvoicesBackgroundJob : AsyncBackgroundJob<EmailApprovedInvoicesBackgroundJobArgs>, ITransientDependency
    {
        private readonly IExtendedAbpSession _session;
        private readonly IInvoiceAppService _invoiceAppService;
        private readonly IAppNotifier _appNotifier;

        public EmailApprovedInvoicesBackgroundJob(
            IExtendedAbpSession session,
            IInvoiceAppService invoiceAppService,
            IAppNotifier appNotifier
        )
        {
            _session = session;
            _invoiceAppService = invoiceAppService;
            _appNotifier = appNotifier;
        }

        [UnitOfWork]
        public override async Task ExecuteAsync(EmailApprovedInvoicesBackgroundJobArgs args)
        {
            using (_session.Use(args.RequestorUser.TenantId, args.RequestorUser.UserId))
            using (UnitOfWorkManager.Current.SetTenantId(args.TenantId))
            {
                string errorText = null;
                try
                {
                    var result = await _invoiceAppService.EmailApprovedInvoices(args.Input);
                    if (!result.Success)
                    {
                        if (result.FromEmailAddressIsNotVerifiedError)
                        {
                            errorText = "Failed to send approved invoices: This domain and email address are not verified. You must be verified to use this email functionality. If you want to use this functionality, please send a support request";
                        }
                        else if (result.SomeEmailsWereNotSentError)
                        {
                            errorText = "Failed to send approved invoices: Some emails were not sent successfully, please try again";
                        }
                        else
                        {
                            errorText = "Failed to send approved invoices";
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    Logger.Error("Error during EmailApprovedInvoicesBackgroundJob execution", ex);
                    errorText = "Failed to send approved invoices";
                }

                if (errorText != null)
                {
                    await _appNotifier.SendMessageAsync(args.RequestorUser,
                        errorText,
                        NotificationSeverity.Error
                    );
                }
                else
                {
                    await _appNotifier.SendMessageAsync(args.RequestorUser,
                        "Finished sending approved invoices",
                        NotificationSeverity.Success
                    );
                }
            }
        }
    }
}
