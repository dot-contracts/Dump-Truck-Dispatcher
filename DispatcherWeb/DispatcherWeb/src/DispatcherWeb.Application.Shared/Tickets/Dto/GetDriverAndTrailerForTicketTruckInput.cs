using System;

namespace DispatcherWeb.Tickets.Dto
{
    public class GetDriverAndTrailerForTicketTruckInput
    {
        public int? TruckId { get; set; }
        public string TruckCode { get; set; }

        public int? OrderLineId { get; set; }
        public DateTime? OrderDate { get; set; }

        public bool ValidateInput()
        {
            if (TruckId == null && string.IsNullOrEmpty(TruckCode))
            {
                return false;
            }

            if (OrderLineId == null && OrderDate == null)
            {
                return false;
            }

            return true;
        }
    }
}
