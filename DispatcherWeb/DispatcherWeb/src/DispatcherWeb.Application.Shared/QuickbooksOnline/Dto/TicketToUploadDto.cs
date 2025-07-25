using System;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.QuickbooksOnline.Dto
{
    public class TicketToUploadDto : ITicketQuantity
    {
        public DateTime? OrderDeliveryDate { get; set; }
        public DateTime? TicketDateTimeUtc { get; set; }
        public int? OrderLineMaterialUomId { get; set; }
        public int? OrderLineFreightUomId { get; set; }
        public int? TicketUomId { get; set; }
        public string TicketUomName { get; set; }
        public string TicketFreightUomName { get; set; }
        public string TicketMaterialUomName { get; set; }
        public bool? IsOrderLineMaterialTotalOverridden { get; set; }
        public bool? IsOrderLineFreightTotalOverridden { get; set; }
        public decimal? OrderLineMaterialTotal { get; set; }
        public decimal? OrderLineFreightTotal { get; set; }
        public int? OrderId { get; set; }
        public DesignationEnum? Designation { get; set; }
        DesignationEnum ITicketQuantity.Designation => Designation ?? DesignationEnum.MaterialOnly;
        public LocationNameDto LoadAt { get; set; }
        public LocationNameDto DeliverTo { get; set; }

        public bool HasOrderLine { get; set; }

        public decimal? FreightQuantity { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public string TruckCode { get; set; }

        public int? CarrierId { get; set; }

        public string CarrierName { get; set; }

        public decimal? LeaseHaulerRate { get; set; }

        public decimal? LeaseHaulerCost => FreightQuantity * LeaseHaulerRate;

        public TicketToUploadDto Clone()
        {
            return new TicketToUploadDto
            {
                OrderDeliveryDate = OrderDeliveryDate,
                TicketDateTimeUtc = TicketDateTimeUtc,
                OrderLineFreightUomId = OrderLineFreightUomId,
                OrderLineMaterialUomId = OrderLineMaterialUomId,
                TicketUomId = TicketUomId,
                TicketUomName = TicketUomName,
                TicketFreightUomName = TicketFreightUomName,
                TicketMaterialUomName = TicketMaterialUomName,
                IsOrderLineMaterialTotalOverridden = IsOrderLineMaterialTotalOverridden,
                IsOrderLineFreightTotalOverridden = IsOrderLineFreightTotalOverridden,
                OrderLineMaterialTotal = OrderLineMaterialTotal,
                OrderLineFreightTotal = OrderLineFreightTotal,
                Designation = Designation,
                LoadAt = LoadAt,
                DeliverTo = DeliverTo,
                HasOrderLine = HasOrderLine,
                FreightQuantity = FreightQuantity,
                MaterialQuantity = MaterialQuantity,
                TruckCode = TruckCode,
                CarrierId = CarrierId,
                CarrierName = CarrierName,
                LeaseHaulerRate = LeaseHaulerRate,
                OrderId = OrderId,
            };
        }
    }
}
