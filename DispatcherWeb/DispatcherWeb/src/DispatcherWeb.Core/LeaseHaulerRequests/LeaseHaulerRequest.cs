using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;

namespace DispatcherWeb.LeaseHaulerRequests
{
    [Table("LeaseHaulerRequest")]
    public class LeaseHaulerRequest : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int? OrderLineId { get; set; }
        public OrderLine OrderLine { get; set; }

        public Guid Guid { get; set; }

        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }

        public int OfficeId { get; set; }
        public Office Office { get; set; }

        public int LeaseHaulerId { get; set; }
        public LeaseHauler LeaseHauler { get; set; }

        public DateTime? Sent { get; set; }

        public int? Available { get; set; }
        public int? Approved { get; set; }

        public int? NumberTrucksRequested { get; set; }

        public LeaseHaulerRequestStatus? Status { get; set; }

        [StringLength(EntityStringFieldLengths.LeaseHaulerRequest.Comments)]
        public string Comments { get; set; }

        [StringLength(EntityStringFieldLengths.LeaseHaulerRequest.Message)]
        public string Message { get; set; }

        public bool SuppressLeaseHaulerDispatcherNotification { get; set; }

        public virtual ICollection<RequestedLeaseHaulerTruck> RequestedLeaseHaulerTrucks { get; set; }
        public virtual ICollection<AvailableLeaseHaulerTruck> AvailableLeaseHaulerTrucks { get; set; }
    }
}
