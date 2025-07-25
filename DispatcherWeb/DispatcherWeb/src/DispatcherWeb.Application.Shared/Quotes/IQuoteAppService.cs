using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Quotes.Dto;

namespace DispatcherWeb.Quotes
{
    public interface IQuoteAppService : IApplicationService
    {
        Task<PagedResultDto<QuoteDto>> GetQuotes(GetQuotesInput input);
        Task<ListResultDto<SelectListDto>> GetQuotesForCustomer(GetQuotesForCustomerInput input);
        Task<QuoteEditDto> GetQuoteForEdit(NullableIdDto input);
        Task<int> EditQuote(QuoteEditDto model);
        Task<int> CopyQuote(EntityDto input);
        Task<int> CreateQuoteFromOrder(CreateQuoteFromOrderInput input);
        Task<bool> CanDeleteQuote(EntityDto input);
        Task SetQuoteStatus(SetQuoteStatusInput model);
        Task DeleteQuote(EntityDto input);
        Task InactivateQuote(EntityDto input);

        Task<PagedResultDto<QuoteLineDto>> GetQuoteLines(GetQuoteLinesInput input);
        Task<QuoteLineEditDto> GetQuoteLineForEdit(GetQuoteLineForEditInput input);
        Task EditQuoteLine(QuoteLineEditDto model);
        Task DeleteQuoteLines(IdListInput input);

        Task<byte[]> GetQuoteReport(GetQuoteReportInput input);
        Task<EmailQuoteReportDto> GetEmailQuoteReportModel(EntityDto input);
        Task<EmailQuoteReportResult> EmailQuoteReport(EmailQuoteReportDto input);

        Task ActivateQuote(EntityDto input);
    }
}
