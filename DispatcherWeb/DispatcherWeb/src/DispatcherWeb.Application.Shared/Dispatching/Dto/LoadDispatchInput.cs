using System;
using Abp.Auditing;
using DispatcherWeb.Tickets;

namespace DispatcherWeb.Dispatching.Dto
{
    public class LoadDispatchInput : ITicketEditQuantity
    {
        public int? Id { get; set; }

        public Guid? Guid { get; set; } //deprecated, temporarily kept for backwards compatibility

        public int? LoadId { get; set; }

        public bool IsEdit { get; set; }

        public DispatchStatus? DispatchStatus { get; set; }

        public int? TimeClassificationId { get; set; }

        public string TicketNumber { get; set; }

        public decimal? Amount { get; set; }

        public int? LoadCount { get; set; }

        public double? SourceLatitude { get; set; }

        public double? SourceLongitude { get; set; }

        public Guid? TicketPhotoId { get; set; }

        public Guid? DeferredPhotoId { get; set; }

        public string TicketPhotoFilename { get; set; }

        [DisableAuditing]
        public string TicketPhotoBase64 { get; set; }

        public bool CreateNewTicket { get; set; }

        public bool TicketControlsWereHidden { get; set; }

        public DriverApplicationActionInfo Info { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public int? FreightUomId { get; set; }

        public int? FreightItemId { get; set; }

        public int? MaterialItemId { get; set; }
    }
}
