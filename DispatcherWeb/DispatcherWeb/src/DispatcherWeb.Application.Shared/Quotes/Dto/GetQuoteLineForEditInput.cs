using Abp.Application.Services.Dto;

namespace DispatcherWeb.Quotes.Dto
{
    public class GetQuoteLineForEditInput : NullableIdDto
    {
        public int? QuoteId { get; set; }
    }
}
