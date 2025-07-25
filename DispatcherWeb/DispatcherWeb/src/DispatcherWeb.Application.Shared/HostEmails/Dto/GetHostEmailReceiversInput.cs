using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.HostEmails.Dto
{
    public class GetHostEmailReceiversInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public int HostEmailId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = nameof(HostEmailReceiverDto.TenantName);
            }
        }
    }
}
