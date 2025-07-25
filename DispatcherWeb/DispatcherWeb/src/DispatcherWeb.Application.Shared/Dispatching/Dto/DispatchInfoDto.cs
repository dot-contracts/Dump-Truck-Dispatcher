using System;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Orders;
using DispatcherWeb.Tickets.Dto;
using Newtonsoft.Json;

namespace DispatcherWeb.Dispatching.Dto
{
    public class DispatchInfoBaseDto
    {

    }

    public class DriverInfoNotFoundDto : DispatchInfoBaseDto
    {

    }

    public class DispatchInfoErrorAndRedirect : DispatchInfoBaseDto
    {
        public string Message { get; set; }

        public string RedirectUrl { get; set; }

        public string UrlText { get; set; }
    }

    public abstract class DispatchInfoDto : DispatchInfoBaseDto
    {
        public int DispatchId { get; set; }

        public Guid Guid { get; set; }

        public string CustomerName { get; set; }

        public string ContactName { get; set; }

        public string ContactPhoneNumber { get; set; }

        public DispatchStatus DispatchStatus { get; set; }

        public bool IsMultipleLoads { get; set; }

        public bool WasMultipleLoads { get; set; }

        public int TenantId { get; set; }
    }

    public class DispatchInfoCompletedDto : DispatchInfoDto
    {

    }

    public class DispatchInfoCanceledDto : DispatchInfoDto
    {

    }

    public class DispatchInfoExpiredDto : DispatchInfoDto
    {

    }

    public class DispatchLoadInfoDto : DispatchInfoDto
    {
        public string Item { get; set; }

        public DesignationEnum Designation { get; set; }

        public string PickupAt => LoadAtName + ". " + LoadAt?.FormattedAddress;

        [JsonIgnore]
        public string LoadAtName { get; set; }

        [JsonIgnore]
        public LocationAddressDto LoadAt { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public string MaterialUomName { get; set; }

        public string FreightUomName { get; set; }

        public DateTime Date { get; set; }

        public Shift? Shift { get; set; }

        public string Note { get; set; }

        public string ChargeTo { get; set; }

        public int? LoadId => LastLoad?.Id;

        public Guid? SignatureId => LastLoad?.SignatureId;

        public bool CreateNewTicket { get; set; }

        public DispatchCompleteInfoLoadDto LastLoad { get; set; }

        public bool RequireTicket { get; set; }

        public TicketControlVisibilityDto VisibleTicketControls { get; set; }
    }

    public class DispatchDestinationInfoDto : DispatchInfoDto
    {
        public string CustomerAddress => DeliverToName + ". " + DeliverTo?.FormattedAddress;

        [JsonIgnore]
        public string DeliverToName { get; set; }

        [JsonIgnore]
        public LocationAddressDto DeliverTo { get; set; }

        public DateTime Date { get; set; }

        public Shift? Shift { get; set; }

        public string Note { get; set; }

        public Guid? SignatureId { get; set; }
    }

    public class DispatchCompleteInfoDto : DispatchInfoDto, IOrderLineItemWithQuantity
    {
        public string Item => FreightItemName;

        public string FreightItemName { get; set; }

        public string MaterialItemName { get; set; }

        public DesignationEnum Designation { get; set; }

        public DateTime? TimeOnJobUtc { get; set; }

        public string LoadAtName { get; set; }

        public string LoadAtAddress => LoadAt?.FormattedAddress;

        [JsonIgnore]
        public LocationAddressDto LoadAt { get; set; }

        public decimal? LoadAtLatitude { get; set; }

        public decimal? LoadAtLongitude { get; set; }

        public DispatchCompleteInfoLoadDto LastLoad { get; set; }

        public string TicketNumber => LastLoad?.LastTicket?.TicketNumber;

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public decimal? Amount => TicketMaterialQuantity;
        public decimal? TicketQuantity => TicketMaterialQuantity;
        public decimal? TicketFreightQuantity => LastLoad?.LastTicket?.FreightQuantity;
        public decimal? TicketMaterialQuantity => LastLoad?.LastTicket?.MaterialQuantity;

        public int? LoadCount => LastLoad?.LastTicket?.LoadCount;

        public string MaterialUomName { get; set; }

        public string FreightUomName { get; set; }

        public DateTime Date { get; set; }

        public Shift? Shift { get; set; }

        public string JobNumber { get; set; }

        public string Note { get; set; }

        public string ChargeTo { get; set; }

        public Guid? SignatureId => LastLoad?.SignatureId;

        public DateTime? AcknowledgedDateTimeUtc { get; set; }

        public DateTime? LoadedDateTimeUtc => LastLoad?.SourceDateTime;

        public string DeliverToName { get; set; }

        public string DeliverToAddress => DeliverTo?.FormattedAddress;

        [JsonIgnore]
        public LocationAddressDto DeliverTo { get; set; }

        public decimal? DeliverToLatitude { get; set; }

        public decimal? DeliverToLongitude { get; set; }

        public DateTime LastUpdateDateTime { get; set; }

        public int Id { get; set; }

        public int SortOrder { get; set; }

        public int NumberOfLoadsToFinish { get; set; }

        public int NumberOfAddedLoads { get; set; }

        public bool ProductionPay { get; set; }

        public bool RequireTicket { get; set; }

        public string TruckCode { get; set; }

        public string TrailerTruckCode { get; set; }

        public bool HasTickets { get; set; }

        public bool IsTicketAdded => LastLoad?.LastTicket != null;

        public bool IsTicketPhotoAdded => LastLoad?.LastTicket?.TicketPhotoId != null;

        public bool IsSignatureAdded => LastLoad?.SignatureId != null;

        public int? OrderLineTruckId { get; set; }

        public int? FreightUomId { get; set; }

        public int? MaterialUomId { get; set; }

        public int? MaterialItemId { get; set; }

        public int? FreightItemId { get; set; }

        public string QuantityWithItem { get; set; }

        public TicketControlVisibilityDto VisibleTicketControls { get; set; }
    }
}
