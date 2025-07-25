using Abp.Application.Services.Dto;

namespace DispatcherWeb.Quotes.Dto
{
    public class GetQuotesForCustomerInput : NullableIdDto
    {
        public bool HideInactive { get; set; }
    }
}
