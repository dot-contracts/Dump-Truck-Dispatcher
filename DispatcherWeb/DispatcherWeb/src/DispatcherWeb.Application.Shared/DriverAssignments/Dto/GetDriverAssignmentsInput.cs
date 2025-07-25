using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.DriverAssignments.Dto
{
    public class GetDriverAssignmentsInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime Date { get; set; }

        public Shift? Shift { get; set; }

        public int? OfficeId { get; set; }

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
