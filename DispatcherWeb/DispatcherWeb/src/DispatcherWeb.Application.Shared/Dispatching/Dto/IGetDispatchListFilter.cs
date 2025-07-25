using System;

namespace DispatcherWeb.Dispatching.Dto
{
    public interface IGetDispatchListFilter
    {
        DateTime? DateBegin { get; set; }
        DateTime? DateEnd { get; set; }
        int[] TruckIds { get; set; }
        int[] DriverIds { get; set; }
        DispatchStatus[] Statuses { get; set; }
        int? CustomerId { get; set; }
        int? OrderLineId { get; set; }
        bool MissingTickets { get; set; }
        int? OfficeId { get; set; }
    }
}