using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.MultiTenancy.Accounting
{
    [Table("AppInvoices")]
    public class Invoice : Entity<int>
    {
        [StringLength(EntityStringFieldLengths.AppInvoices.InvoiceNo)]
        public string InvoiceNo { get; set; }

        public DateTime InvoiceDate { get; set; }

        [StringLength(EntityStringFieldLengths.AppInvoices.TenantLegalName)]
        public string TenantLegalName { get; set; }

        [StringLength(EntityStringFieldLengths.AppInvoices.TenantAddress)]
        public string TenantAddress { get; set; }

        [StringLength(EntityStringFieldLengths.AppInvoices.TenantTaxNo)]
        public string TenantTaxNo { get; set; }
    }
}
