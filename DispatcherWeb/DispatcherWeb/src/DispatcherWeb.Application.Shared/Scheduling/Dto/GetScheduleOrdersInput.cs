using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Scheduling.Dto
{
    public class GetScheduleOrdersInput : SortedInputDto, IShouldNormalize, IGetScheduleInput
    {
        public int? OfficeId { get; set; }
        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }
        public bool HideCompletedOrders { get; set; }
        public bool HideProgressBar { get; set; }
        public int? TruckCategoryId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Date";
            }

            if (Sorting.Contains("Date"))
            {
                Sorting = Sorting.Replace("Date", "@Date");
            }
        }
    }
}
