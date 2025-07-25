using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Orders
{
    [Table("OrderLineTruck")]
    public class OrderLineTruck : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int OrderLineId { get; set; }
        public OrderLine OrderLine { get; set; }

        public int TruckId { get; set; }
        public virtual Truck Truck { get; set; }

        public int? DriverId { get; set; }
        public Driver Driver { get; set; }

        public virtual int? ParentOrderLineTruckId { get; set; }
        public virtual OrderLineTruck ParentOrderLineTruck { get; set; }

        public int? TrailerId { get; set; }
        public virtual Truck Trailer { get; set; }

        public decimal Utilization { get; set; }

        public DateTime? TimeOnJob { get; set; }

        public bool IsDone { get; set; }

        [StringLength(EntityStringFieldLengths.OrderLineTruck.DriverNote)]
        public string DriverNote { get; set; }

        public virtual ICollection<Dispatch> Dispatches { get; set; }
    }
}
