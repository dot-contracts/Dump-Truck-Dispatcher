using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Receipts.Dto;

namespace DispatcherWeb.Receipts
{
    public interface IReceiptAppService : IApplicationService
    {
        Task<ReceiptEditDto> GetReceiptForEdit(GetReceiptForEditInput input);
        Task<int> EditReceipt(ReceiptEditDto model);
        Task<PagedResultDto<ReceiptLineEditDto>> GetReceiptLines(GetReceiptLinesInput input);
        Task<ReceiptLineEditDto> GetReceiptLineForEdit(GetReceiptLineForEditInput input);
        Task<EditReceiptLineOutput> EditReceiptLine(ReceiptLineEditDto model);
        Task<DeleteReceiptLineOutput> DeleteReceiptLines(IdListInput input);
    }
}
