using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.VehicleUsages.Dto
{
    public class GetVehicleUsagePagedListInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public int? OfficeId { get; set; }
        public DateTime? ReadingDateTimeBegin { get; set; }
        public DateTime? ReadingDateTimeEnd { get; set; }
        public ReadingType? ReadingType { get; set; }

        public int? TruckId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "TruckCode";
            }
        }
    }
}
