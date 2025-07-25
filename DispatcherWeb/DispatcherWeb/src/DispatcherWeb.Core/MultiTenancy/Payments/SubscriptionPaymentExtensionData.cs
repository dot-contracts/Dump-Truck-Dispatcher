using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.MultiTenancy;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.MultiTenancy.Payments
{
    [Table("AppSubscriptionPaymentsExtensionData")]
    [MultiTenancySide(MultiTenancySides.Host)]
    public class SubscriptionPaymentExtensionData : Entity<long>, ISoftDelete
    {
        public long SubscriptionPaymentId { get; set; }

        [StringLength(EntityStringFieldLengths.SubscriptionPaymentExtensionData.Key)]
        public string Key { get; set; }

        [StringLength(EntityStringFieldLengths.SubscriptionPaymentExtensionData.Value)]
        public string Value { get; set; }

        public bool IsDeleted { get; set; }
    }
}
