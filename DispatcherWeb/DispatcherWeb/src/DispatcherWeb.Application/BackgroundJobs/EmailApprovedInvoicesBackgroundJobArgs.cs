using Abp;
using DispatcherWeb.Invoices.Dto;

namespace DispatcherWeb.BackgroundJobs
{
    public class EmailApprovedInvoicesBackgroundJobArgs
    {
        public int TenantId { get; set; }
        public UserIdentifier RequestorUser { get; set; }
        public EmailApprovedInvoicesInput Input { get; set; }
    }
}
