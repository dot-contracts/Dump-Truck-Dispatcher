using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Payments
{
    [Table("Payment")]
    public class Payment : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        public int? PaymentHeartlandKeyId { get; set; }

        [StringLength(50)] //24
        public string CreditCardToken { get; set; }

        [StringLength(500)] //46
        public string CreditCardStreetAddress { get; set; }

        [StringLength(12)] //6
        public string CreditCardZipCode { get; set; }

        public long? AuthorizationUserId { get; set; }

        public DateTime? AuthorizationDateTime { get; set; }

        [Column(TypeName = DispatcherWebConsts.DbTypeDecimal19_4)]
        public decimal? AuthorizationAmount { get; set; }

        [StringLength(50)] //10
        public string AuthorizationTransactionId { get; set; }

        public long? AuthorizationCaptureUserId { get; set; }

        public DateTime? AuthorizationCaptureDateTime { get; set; }

        [Column(TypeName = DispatcherWebConsts.DbTypeDecimal19_4)]
        public decimal? AuthorizationCaptureAmount { get; set; }

        //public decimal? AuthorizationCaptureSettlementAmount { get; set; }

        [StringLength(50)] //10
        public string AuthorizationCaptureTransactionId { get; set; }

        [StringLength(5000)] //739
        public string AuthorizationCaptureResponse { get; set; }

        [StringLength(1000)]
        public string PaymentDescription { get; set; }

        public bool IsCancelledOrRefunded { get; set; }

        public long? CancelOrRefundUserId { get; set; }

        [StringLength(15)]
        public string CardType { get; set; }

        [StringLength(4)]
        public string CardLast4 { get; set; }

        [StringLength(EntityStringFieldLengths.Payment.TransactionType)]
        public string TransactionType { get; set; }

        [StringLength(EntityStringFieldLengths.Payment.BatchSummaryId)]
        public string BatchSummaryId { get; set; }

        public virtual PaymentHeartlandKey PaymentHeartlandKey { get; set; }

        public virtual User AuthorizationUser { get; set; }

        public virtual User AuthorizationCaptureUser { get; set; }

        public virtual User CancelOrRefundUser { get; set; }

        public virtual ICollection<OrderPayment> OrderPayments { get; set; }
    }
}
