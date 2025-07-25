using System;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public interface IGetLeaseHaulerRequestPagedListInput
    {
        DateTime DateBegin { get; set; }
        DateTime DateEnd { get; set; }
        Shift? Shift { get; set; }
        int? LeaseHaulerId { get; set; }
        int? OfficeId { get; set; }
    }
}