using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Common.Dto
{
    public class FindUsersInput : PagedSortedAndFilteredInputDto, IShouldNormalize
    {
        public int? TenantId { get; set; }
        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }

        public bool ExcludeCurrentUser { get; set; }
    }
}
