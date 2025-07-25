using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;

namespace DispatcherWeb.Payments
{
    [Table("OrderPayment")]
    public class OrderPayment : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int? OfficeId { get; set; }

        public int OrderId { get; set; }

        public int PaymentId { get; set; }

        public int? ReceiptId { get; set; }

        public virtual Order Order { get; set; }

        public virtual Payment Payment { get; set; }

        public virtual Receipt Receipt { get; set; }

        public virtual Office Office { get; set; }
    }
}
