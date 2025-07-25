using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Drivers.Dto
{
    public class GetDriversInput : PagedAndSortedInputDto, IShouldNormalize, IGetDriverListFilter
    {
        public string Name { get; set; }

        public FilterActiveStatus Status { get; set; }

        public int? OfficeId { get; set; }

        public bool? HasUserId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "LastName,FirstName";
            }
        }
    }
}
