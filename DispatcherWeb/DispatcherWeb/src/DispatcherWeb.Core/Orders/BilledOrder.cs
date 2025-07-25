using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Offices;

namespace DispatcherWeb.Orders
{
    [Table("BilledOrder")]
    public class BilledOrder : FullAuditedEntity
    {
        public int OrderId { get; set; }
        public int OfficeId { get; set; }
        public virtual Order Order { get; set; }
        public virtual Office Office { get; set; }
    }
}
