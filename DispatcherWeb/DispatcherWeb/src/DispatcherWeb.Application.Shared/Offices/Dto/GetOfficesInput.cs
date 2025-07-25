using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Offices.Dto
{
    public class GetOfficesInput : PagedAndSortedInputDto, IShouldNormalize
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
