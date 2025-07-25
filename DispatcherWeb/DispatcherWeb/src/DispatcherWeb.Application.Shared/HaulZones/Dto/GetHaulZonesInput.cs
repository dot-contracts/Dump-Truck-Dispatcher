using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.HaulZones.Dto
{
    public class GetHaulZonesInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public string Name { get; set; }

        public FilterActiveStatus Status { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }
    }
}
