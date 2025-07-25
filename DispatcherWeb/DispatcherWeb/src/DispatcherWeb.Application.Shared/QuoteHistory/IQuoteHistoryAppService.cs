using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.QuoteHistory.Dto;

namespace DispatcherWeb.QuoteHistory
{
    public interface IQuoteHistoryAppService : IApplicationService
    {
        Task<PagedResultDto<QuoteHistoryDto>> GetQuoteHistory(GetQuoteHistoryInput input);
        Task<QuoteHistoryDto> GetQuoteHistoryDetails(EntityDto input);
    }
}
