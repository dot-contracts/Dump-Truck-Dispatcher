using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Orders;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Dispatching
{
    [Table("Dispatch")]
    public class Dispatch : FullAuditedEntity, IMustHaveTenant
    {
        public static DispatchStatus[] ClosedDispatchStatuses = new[] { DispatchStatus.Completed, DispatchStatus.Error, DispatchStatus.Canceled };
        public static DispatchStatus[] OutstandingDispatchStatuses = new[] { DispatchStatus.Created, DispatchStatus.Sent, /*DispatchStatus.Received, */DispatchStatus.Acknowledged };
        public static DispatchStatus[] UnacknowledgedStatuses = new[] { DispatchStatus.Created, DispatchStatus.Sent };
        public static DispatchStatus[] AcknowledgedOrLoadedStatuses = new[] { DispatchStatus.Acknowledged, DispatchStatus.Loaded };
        public static DispatchStatus[] OpenStatuses = new[] { DispatchStatus.Created, DispatchStatus.Sent, DispatchStatus.Acknowledged, DispatchStatus.Loaded };

        public int TenantId { get; set; }

        public int SortOrder { get; set; }

        public int TruckId { get; set; }
        public Truck Truck { get; set; }

        public int DriverId { get; set; }
        public Driver Driver { get; set; }

        public int OrderLineId { get; set; }
        public OrderLine OrderLine { get; set; }

        public int? OrderLineTruckId { get; set; }
        public virtual OrderLineTruck OrderLineTruck { get; set; }

        public long? UserId { get; set; }
        public User User { get; set; }

        [StringLength(EntityStringFieldLengths.Dispatch.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [StringLength(EntityStringFieldLengths.Dispatch.Email)]
        public string EmailAddress { get; set; }

        public OrderNotifyPreferredFormat OrderNotifyPreferredFormat { get; set; }

        public DateTime? Sent { get; set; }

        public DateTime? Acknowledged { get; set; }

        public DateTime? Canceled { get; set; }

        public DateTime? TimeOnJob { get; set; }

        [StringLength(EntityStringFieldLengths.Dispatch.Message)]
        public string Message { get; set; }

        public Guid Guid { get; set; }

        public DispatchStatus Status { get; set; }

        [StringLength(EntityStringFieldLengths.Dispatch.Note)]
        public string Note { get; set; }

        public bool IsMultipleLoads { get; set; }

        public bool WasMultipleLoads { get; set; }

        public int NumberOfLoadsToFinish { get; set; }

        public int NumberOfAddedLoads { get; set; }

        public decimal? MaterialQuantity { get; set; }

        public decimal? FreightQuantity { get; set; }

        public ICollection<Load> Loads { get; set; }

        public Guid? FulcrumDtdTicketGuid { get; set; }
    }
}
