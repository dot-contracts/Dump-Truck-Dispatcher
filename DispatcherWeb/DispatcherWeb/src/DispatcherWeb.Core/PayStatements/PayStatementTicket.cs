using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Orders;
using DispatcherWeb.TimeClassifications;

namespace DispatcherWeb.PayStatements
{
    [Table("PayStatementTicket")]
    public class PayStatementTicket : CreationAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }
        public int PayStatementDetailId { get; set; }
        public virtual PayStatementDetail PayStatementDetail { get; set; }
        public int TicketId { get; set; }
        public virtual Ticket Ticket { get; set; }
        public DriverIsPaidForLoadBasedOnEnum DriverIsPaidForLoadBasedOn { get; set; }
        public decimal Quantity { get; set; }
        public decimal FreightRate { get; set; }
        public decimal Total { get; set; }
        public int TimeClassificationId { get; set; }
        public virtual TimeClassification TimeClassification { get; set; }
        public decimal DriverPayRate { get; set; }
    }
}
