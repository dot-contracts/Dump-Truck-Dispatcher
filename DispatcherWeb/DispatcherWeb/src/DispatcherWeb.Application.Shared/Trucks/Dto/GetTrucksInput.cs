using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Trucks.Dto
{
    public class GetTrucksInput : PagedAndSortedInputDto, IShouldNormalize, IGetTruckListFilter
    {
        public string TruckCode { get; set; }

        public int? OfficeId { get; set; }

        public int? VehicleCategoryId { get; set; }

        public FilterActiveStatus Status { get; set; }

        public bool? IsOutOfService { get; set; }
        public bool PlatesExpiringThisMonth { get; set; }
        public bool IncludeSold { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "TruckCode";
            }
        }
    }
}
