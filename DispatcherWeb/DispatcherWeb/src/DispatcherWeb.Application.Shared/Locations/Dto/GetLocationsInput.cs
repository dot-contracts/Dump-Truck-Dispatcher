using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Locations.Dto
{
    public class GetLocationsInput : PagedAndSortedInputDto, IShouldNormalize, IGetLocationFilteredList
    {
        public string Name { get; set; }

        public int? CategoryId { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public FilterActiveStatus Status { get; set; }

        public bool WithCoordinates { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }
    }
}
