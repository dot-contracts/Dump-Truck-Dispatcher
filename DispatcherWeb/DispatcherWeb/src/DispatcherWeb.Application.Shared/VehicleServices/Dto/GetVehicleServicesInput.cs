using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.VehicleServices.Dto
{
    public class GetVehicleServicesInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public string Name { get; set; }
        public int? VehicleServiceTypeId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Name";
            }
        }

    }
}
