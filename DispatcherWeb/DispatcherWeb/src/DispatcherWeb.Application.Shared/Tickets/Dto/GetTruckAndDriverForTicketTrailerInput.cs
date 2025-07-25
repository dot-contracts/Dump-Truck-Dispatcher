using System;

namespace DispatcherWeb.Tickets.Dto
{
    public class GetTruckAndDriverForTicketTrailerInput
    {
        public int TrailerId { get; set; }


        public int? OrderLineId { get; set; }
        public DateTime? OrderDate { get; set; }

        public bool ValidateInput()
        {
            if (TrailerId == 0)
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
