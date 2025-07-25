using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Customers;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders.TaxDetails;
using DispatcherWeb.Payments;
using DispatcherWeb.Quotes;

namespace DispatcherWeb.Orders
{
    [Table("Receipt")]
    public class Receipt : FullAuditedEntity, IMustHaveTenant, IOrderTaxDetails
    {
        public Receipt()
        {
            ReceiptLines = new HashSet<ReceiptLine>();
            OrderPayments = new HashSet<OrderPayment>();
        }

        public int TenantId { get; set; }

        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        public bool IsFreightTotalOverridden { get; set; }

        public bool IsMaterialTotalOverridden { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime ReceiptDate { get; set; }
        public Shift? Shift { get; set; }
        public int OfficeId { get; set; }
        public virtual Office Office { get; set; }
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public int? QuoteId { get; set; }
        public Quote Quote { get; set; }

        [StringLength(EntityStringFieldLengths.Order.PoNumber)]
        public string PoNumber { get; set; }

        public decimal SalesTaxRate { get; set; }




        public decimal FreightTotal { get; set; }

        public decimal MaterialTotal { get; set; }

        [Column(TypeName = "money")]
        public decimal SalesTax { get; set; }

        [Column(TypeName = "money")]
        public decimal Total { get; set; }

        [NotMapped]
        decimal IOrderTaxDetails.CODTotal { get => Total; set => Total = value; }

        public virtual ICollection<ReceiptLine> ReceiptLines { get; set; }

        public virtual ICollection<OrderPayment> OrderPayments { get; set; }

        public static Receipt FromOrder(Order order, int officeId)
        {
            return new Receipt
            {
                CustomerId = order.CustomerId,
                DeliveryDate = order.DeliveryDate,
                //MaterialTotal
                //FreightTotal = 
                //IsFreightTotalOverridden = 
                //IsMaterialTotalOverridden = 
                //Total
                OfficeId = officeId,
                OrderId = order.Id,
                PoNumber = order.PONumber,
                QuoteId = order.QuoteId,
                SalesTax = order.SalesTax,
                SalesTaxRate = order.SalesTaxRate,
                Shift = order.Shift,
                TenantId = order.TenantId,
                ReceiptDate = order.DeliveryDate,
            };
        }
    }
}
