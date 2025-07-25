using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Orders.Dto
{
    public class GetOrdersInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? OfficeId { get; set; }
        public int? CustomerId { get; set; }
        public int? ItemId { get; set; }
        public string JobNumber { get; set; }
        public string Misc { get; set; }
        public int? LoadAtId { get; set; }
        public int? DeliverToId { get; set; }
        public bool ShowPendingOrders { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "DeliveryDate";
            }

            //if (Sorting.Contains("DateTime"))
            //{
            //    Sorting = Sorting.Replace("DateTime", "@DateTime");
            //}
        }
    }
}
