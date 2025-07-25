using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.PricingTiers.Dto
{
    public class GetPricingTiersInput : PagedAndSortedInputDto, IShouldNormalize
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
