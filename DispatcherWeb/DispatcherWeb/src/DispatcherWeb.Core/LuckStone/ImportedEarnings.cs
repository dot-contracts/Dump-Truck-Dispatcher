using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.LuckStone
{
    [Table("ImportedEarnings")]
    public class ImportedEarnings : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [StringLength(EntityStringFieldLengths.ImportedEarnings.TicketNumber)]
        public string TicketNumber { get; set; }

        public DateTime TicketDateTime { get; set; }

        [StringLength(EntityStringFieldLengths.ImportedEarnings.Site)]
        public string Site { get; set; }

        [StringLength(EntityStringFieldLengths.ImportedEarnings.HaulerRef)]
        public string HaulerRef { get; set; }

        [StringLength(EntityStringFieldLengths.ImportedEarnings.CustomerName)]
        public string CustomerName { get; set; }

        [StringLength(EntityStringFieldLengths.ImportedEarnings.LicensePlate)]
        public string LicensePlate { get; set; }

        public decimal HaulPaymentRate { get; set; }

        public decimal NetTons { get; set; }

        public decimal HaulPayment { get; set; }

        [StringLength(EntityStringFieldLengths.ImportedEarnings.Uom)]
        public string HaulPaymentRateUom { get; set; }

        public decimal FscAmount { get; set; }

        [StringLength(EntityStringFieldLengths.ImportedEarnings.ProductDescription)]
        public string ProductDescription { get; set; }


        public int BatchId { get; set; }

        public virtual ImportedEarningsBatch Batch { get; set; }
    }
}
