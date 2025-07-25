using System;
using System.Collections.Generic;
using DispatcherWeb.Offices.Dto;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class LeaseHaulerRequestEditDto : IOfficeIdNameDto
    {
        public int Id { get; set; }
        public int? OrderLineId { get; set; }

        public DateTime? Date { get; set; }
        public Shift? Shift { get; set; }
        public int OfficeId { get; set; }
        public string OfficeName { get; set; }
        public bool IsSingleOffice { get; set; }

        public int LeaseHaulerId { get; set; }
        public string LeaseHaulerName { get; set; }

        public string Message { get; set; }
        public string Comments { get; set; }

        public int? Available { get; set; }
        public int? Approved { get; set; }
        public int? NumberTrucksRequested { get; set; }
        public LeaseHaulerRequestStatus? Status { get; set; }
        public bool SuppressLeaseHaulerDispatcherNotification { get; set; }
        public List<RequestedLeaseHaulerTruckEditDto> RequestedTrucks { get; set; }
        public List<AvailableTrucksTruckEditDto> AvailableTrucks { get; set; }
    }
}
