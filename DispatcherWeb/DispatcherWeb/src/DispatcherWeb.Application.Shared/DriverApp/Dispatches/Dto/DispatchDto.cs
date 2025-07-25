using System;
using System.Collections.Generic;
using DispatcherWeb.DriverApp.Loads.Dto;
using DispatcherWeb.DriverApp.Locations.Dto;
using DispatcherWeb.DriverApp.Tickets.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.Tickets.Dto;

namespace DispatcherWeb.DriverApp.Dispatches.Dto
{
    public class DispatchDto : DispatchEditDto, IOrderLineItemWithQuantity
    {
        public int TenantId { get; set; }
        public string CustomerName { get; set; }
        public CustomerContactDto CustomerContact { get; set; }
        public DateTime OrderDate { get; set; }
        public Shift? Shift { get; set; }
        public DesignationEnum Designation { get; set; }
        public int? FreightItemId { get; set; }
        public string FreightItem { get; set; }
        public int? MaterialItemId { get; set; }
        public string MaterialItem { get; set; }
        public string Item => FreightItem; //backwards compatibility
        public LocationDto LoadAt { get; set; }
        public LocationDto DeliverTo { get; set; }
        public CustomerNotificationDto CustomerNotification { get; set; }
        public decimal? MaterialQuantity { get; set; }
        public decimal? FreightQuantity { get; set; }
        public short? TravelTime { get; set; }
        public string JobNumber { get; set; }
        public string Note { get; set; }
        public bool IsCOD { get; set; }
        public string ChargeTo { get; set; }
        public int? MaterialUomId { get; set; }
        public string MaterialUOM { get; set; }
        public int? FreightUomId { get; set; }
        public string FreightUOM { get; set; }
        public DateTime LastModifiedDateTime { get; set; }

        public DateTime? TimeOnJob { get; set; }

        public bool ProductionPay { get; set; }
        public bool RequireTicket { get; set; }
        public List<LoadDto> Loads { get; set; }
        public List<TicketDto> Tickets { get; set; }
        public int TruckId { get; set; }
        public string TruckCode { get; set; }
        public int? TrailerId { get; set; }
        public string TrailerTruckCode { get; set; }
        public int? DriverId { get; set; }
        public int? TimeClassificationId { get; set; }
        public bool EnableDriverAppGps { get; set; }
        public int OrderLineId { get; set; }
        public int? OrderLineTruckId { get; set; }
        public int SortOrder { get; set; }
        public TicketControlVisibilityDto VisibleTicketControls { get; set; }

        public string QuantityWithItem { get; set; }

        string IOrderLineItemWithQuantity.MaterialItemName => MaterialItem;

        string IOrderLineItemWithQuantity.FreightItemName => FreightItem;

        string IOrderLineItemWithQuantity.MaterialUomName => MaterialUOM;

        string IOrderLineItemWithQuantity.FreightUomName => FreightUOM;
    }
}
