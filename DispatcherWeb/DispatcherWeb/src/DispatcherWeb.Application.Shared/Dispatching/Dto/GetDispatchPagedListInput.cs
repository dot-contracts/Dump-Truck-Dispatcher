using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Dispatching.Dto
{
    public class GetDispatchPagedListInput : PagedAndSortedInputDto, IShouldNormalize, IGetDispatchListFilter
    {
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }

        public int[] TruckIds { get; set; }

        public int[] DriverIds { get; set; }

        public DispatchStatus[] Statuses { get; set; }

        public int? CustomerId { get; set; }

        public int? OrderLineId { get; set; }

        public bool MissingTickets { get; set; }
        public int? OfficeId { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "TruckCode";
            }
        }
    }
}
