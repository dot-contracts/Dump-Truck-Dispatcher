using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Authorization.Roles.Dto
{
    public class GetRolesInput : PagedAndSortedInputDto, IShouldNormalize, IGetRolesInput
    {
        public string Permission { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }
    }
}
