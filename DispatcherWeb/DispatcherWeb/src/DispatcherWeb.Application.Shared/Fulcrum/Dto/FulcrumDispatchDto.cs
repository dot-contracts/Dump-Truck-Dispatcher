using System;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.Fulcrum.Dto
{
    public class FulcrumDispatchDto : ITicketEditQuantity
    {
        public int TenantId { get; set; }

        public int OrderLineId { get; set; }

        public int OfficeId { get; set; }

        public int? LoadAtId { get; set; }

        public int? DeliverToId { get; set; }

        public int TruckId { get; set; }

        public string TruckCode { get; set; }

        public int? TrailerId { get; set; }

        public int? LeaseHaulerId { get; set; }

        public int CustomerId { get; set; }

        public int? FreightItemId { get; set; }

        public int DriverId { get; set; }

        public DesignationEnum Designation { get; set; }

        public int? MaterialUomId { get; set; }

        public int? FreightUomId { get; set; }

        public decimal? FreightQuantity { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public int? MaterialItemId { get; set; }

        public Guid? FulcrumDtdTicketGuid { get; set; }


    }
}
