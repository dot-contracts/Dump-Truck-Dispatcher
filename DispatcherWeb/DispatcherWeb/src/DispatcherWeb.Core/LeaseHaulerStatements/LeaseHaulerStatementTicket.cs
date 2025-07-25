using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Orders;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.LeaseHaulerStatements
{
    [Table("LeaseHaulerStatementTicket")]
    public class LeaseHaulerStatementTicket : AuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int LeaseHaulerStatementId { get; set; }

        public int TicketId { get; set; }

        public int LeaseHaulerId { get; set; }

        public int? TruckId { get; set; }

        public decimal Quantity { get; set; }

        public decimal? Rate { get; set; }

        public decimal BrokerFee { get; set; }

        [Column(TypeName = "money")]
        public decimal FuelSurcharge { get; set; }

        public decimal ExtendedAmount { get; set; }

        public virtual LeaseHaulerStatement LeaseHaulerStatement { get; set; }

        public virtual Ticket Ticket { get; set; }

        public virtual LeaseHauler LeaseHauler { get; set; }

        public virtual Truck Truck { get; set; }
    }
}
