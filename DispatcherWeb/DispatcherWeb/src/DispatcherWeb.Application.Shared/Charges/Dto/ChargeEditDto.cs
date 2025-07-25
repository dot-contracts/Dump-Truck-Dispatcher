using System;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Charges.Dto
{
    public class ChargeEditDto
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public string ItemName { get; set; }

        public int UnitOfMeasureId { get; set; }

        public string UnitOfMeasureName { get; set; }

        [StringLength(EntityStringFieldLengths.Charge.Description)]
        public string Description { get; set; }

        public decimal Rate { get; set; }

        public decimal? Quantity { get; set; }

        public bool UseMaterialQuantity { get; set; }

        public decimal ChargeAmount { get; set; }

        public int OrderLineId { get; set; }

        public bool HasInvoiceLines { get; set; }

        public bool IsBilled { get; set; }

        public DateTime? ChargeDate { get; set; }
    }
}
