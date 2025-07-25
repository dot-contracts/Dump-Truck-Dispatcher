using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.CustomerNotifications.Dto
{
    public class GetCustomerNotificationsInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public HostEmailType? Type { get; set; }

        public int? EditionId { get; set; }

        public int? TenantId { get; set; }

        public long? CreatedByUserId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = nameof(CustomerNotificationDto.StartDate);
            }
        }
    }
}