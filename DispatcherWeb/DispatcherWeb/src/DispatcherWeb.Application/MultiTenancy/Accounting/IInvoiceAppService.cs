using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.MultiTenancy.Accounting.Dto;

namespace DispatcherWeb.MultiTenancy.Accounting
{
    public interface IInvoiceAppService
    {
        Task<InvoiceDto> GetInvoiceInfo(EntityDto<long> input);

        Task CreateInvoice(CreateInvoiceDto input);
    }
}
