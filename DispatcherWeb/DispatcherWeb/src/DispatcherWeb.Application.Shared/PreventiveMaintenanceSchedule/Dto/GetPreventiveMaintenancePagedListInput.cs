using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.PreventiveMaintenanceSchedule.Dto
{
    public class GetPreventiveMaintenancePagedListInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public int? OfficeId { get; set; }
        public string TruckCode { get; set; }
        public DateTime? DueDateBegin { get; set; }
        public DateTime? DueDateEnd { get; set; }
        public PreventiveMaintenanceStatus Status { get; set; }
        public int? VehicleServiceId { get; set; }

        public bool DueForService { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "TruckCode";
            }
        }

        public enum PreventiveMaintenanceStatus
        {
            All = 1,
            Overdue = 2,
        }
    }
}
