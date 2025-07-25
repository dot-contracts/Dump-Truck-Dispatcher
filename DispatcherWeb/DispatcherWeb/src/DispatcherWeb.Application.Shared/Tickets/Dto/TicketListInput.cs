using System;
using Abp.Extensions;
using Abp.Runtime.Validation;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Tickets.Dto
{
    public class TicketListInput : PagedAndSortedInputDto, IShouldNormalize
    {
        public DateTime? TicketDateRangeBegin { get; set; }
        public DateTime? TicketDateRangeEnd { get; set; }
        public DateTime? OrderDateRangeBegin { get; set; }
        public DateTime? OrderDateRangeEnd { get; set; }
        public int? OfficeId { get; set; }
        public int? InvoiceId { get; set; }
        public int? CarrierId { get; set; }
        public int? ItemId { get; set; }
        public string TicketNumber { get; set; }
        public int? TruckId { get; set; }
        public Shift[] Shifts { get; set; }
        public BillingStatus? BillingStatus { get; set; }
        public bool? IsVerified { get; set; }
        public int? CustomerId { get; set; }
        public int? LoadAtId { get; set; }
        public int? DeliverToId { get; set; }
        public string JobNumber { get; set; }
        public TicketListStatusFilterEnum? TicketStatus { get; set; }
        public int[] TicketIds { get; set; }
        public int? DriverId { get; set; }
        public int? OrderId { get; set; }
        public bool? IsImported { get; set; }
        public string PONumber { get; set; }
        public int? TruckLeaseHaulerId { get; set; }
        public bool? HasImage { get; set; }

        public void Normalize()
        {
            if (Sorting.IsNullOrEmpty())
            {
                Sorting = "Date";
            }

        }

    }
}
