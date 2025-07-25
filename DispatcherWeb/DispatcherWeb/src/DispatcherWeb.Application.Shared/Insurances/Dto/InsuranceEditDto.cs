using System;
using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Insurances.Dto
{
    public class InsuranceEditDto
    {
        public int Id { get; set; }

        public int LeaseHaulerId { get; set; }

        public int InsuranceTypeId { get; set; }

        public DocumentType? DocumentType { get; set; }

        public string InsuranceTypeName { get; set; }

        public bool IsActive { get; set; }

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
    }
}
