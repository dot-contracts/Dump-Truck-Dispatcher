using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.TaxRates.Dto
{
    public class GetTaxRatesInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }
    }
}
