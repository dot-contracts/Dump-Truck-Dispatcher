using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Quotes.Dto
{
    public class GetQuotesInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public int? CustomerId { get; set; }

        public int? SalesPersonId { get; set; }

        public string Misc { get; set; }

        public int? QuoteId { get; set; }

        public QuoteStatus? Status { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }
    }
}
