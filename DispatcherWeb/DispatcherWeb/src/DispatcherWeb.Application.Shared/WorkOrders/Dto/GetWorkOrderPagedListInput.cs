using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.WorkOrders.Dto
{
    public class GetWorkOrderPagedListInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime? IssueDateBegin { get; set; }
        public DateTime? IssueDateEnd { get; set; }

        public DateTime? StartDateBegin { get; set; }
        public DateTime? StartDateEnd { get; set; }

        public DateTime? CompletionDateBegin { get; set; }
        public DateTime? CompletionDateEnd { get; set; }

        public int? TruckId { get; set; }
        public long? AssignedToId { get; set; }

        public WorkOrderStatus? Status { get; set; }
        public int? WorkOrderId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "IssueDate";
            }
        }
    }
}
