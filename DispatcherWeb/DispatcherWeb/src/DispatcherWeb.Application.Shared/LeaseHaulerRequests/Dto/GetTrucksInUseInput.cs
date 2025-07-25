using System;
using System.Collections.Generic;

namespace DispatcherWeb.LeaseHaulerRequests.Dto
{
    public class GetTrucksInUseInput
    {
        public List<int> TruckIds { get; set; }
        public List<int> DriverIds { get; set; }
        public DateTime? Date { get; set; }
        public Shift? Shift { get; set; }
        public int LeaseHaulerId { get; set; }
        public int OfficeId { get; set; }

        public GetTrucksInUseInput FillFrom(LeaseHaulerRequestEditDto model)
        {
            Date = model.Date;
            Shift = model.Shift;
            LeaseHaulerId = model.LeaseHaulerId;
            OfficeId = model.OfficeId;

            return this;
        }

        public GetTrucksInUseInput FillFrom(AvailableTrucksEditDto model)
        {
            Date = model.Date;
            Shift = model.Shift;
            LeaseHaulerId = model.LeaseHaulerId;
            OfficeId = model.OfficeId;

            return this;
        }
    }
}
