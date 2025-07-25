using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.Payments
{
    [Table("PaymentHeartlandKey")]
    public class PaymentHeartlandKey : FullAuditedEntity
    {
        [Required]
        [StringLength(50)] //30
        public string PublicKey { get; set; }
    }
}
