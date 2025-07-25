using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.LeaseHaulers;

namespace DispatcherWeb.Insurances
{
    [Table("Insurance")]
    public class Insurance : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int LeaseHaulerId { get; set; }

        public bool IsActive { get; set; }

        public int InsuranceTypeId { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime ExpirationDate { get; set; }

        [StringLength(EntityStringFieldLengths.Insurance.IssuedBy)]
        public string IssuedBy { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string IssuerPhone { get; set; }

        [StringLength(EntityStringFieldLengths.Insurance.BrokerName)]
        public string BrokerName { get; set; }

        [StringLength(EntityStringFieldLengths.General.PhoneNumber)]
        public string BrokerPhone { get; set; }

        public int? CoverageLimit { get; set; }

        [StringLength(EntityStringFieldLengths.Insurance.Comments)]
        public string Comments { get; set; }

        public Guid? FileId { get; set; }

        [StringLength(EntityStringFieldLengths.Insurance.FileName)]
        public string FileName { get; set; }

        public virtual InsuranceType InsuranceType { get; set; }

        public virtual LeaseHauler LeaseHauler { get; set; }
    }
}
