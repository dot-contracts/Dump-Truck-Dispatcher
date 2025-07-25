using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Invoices.Dto;
using MigraDocCore.DocumentObjectModel;

namespace DispatcherWeb.Invoices
{
    public interface IInvoiceAppService : IApplicationService
    {
        Task<PagedResultDto<InvoiceDto>> GetInvoices(GetInvoicesInput input);

        Task<InvoiceEditDto> GetInvoiceForEdit(NullableIdDto input);

        Task<Document> GetInvoicePrintOut(GetInvoicePrintOutInput input);
        Task<EmailInvoicePrintOutDto> GetEmailInvoicePrintOutModel(EntityDto input);
        Task<EmailApprovedInvoicesInput> GetEmailOrPrintApprovedInvoicesModalModel();
        Task<EmailInvoicePrintOutResult> EmailApprovedInvoices(EmailApprovedInvoicesInput input);
        Task<Document> PrintApprovedInvoices();
        Task<Document> PrintDraftInvoices(PrintDraftInvoicesInput input);
        DateTime CalculateDueDate(CalculateDueDateInput input);
        Task ValidateInvoiceStatusChange(ValidateInvoiceStatusChangeInput input);
    }
}
