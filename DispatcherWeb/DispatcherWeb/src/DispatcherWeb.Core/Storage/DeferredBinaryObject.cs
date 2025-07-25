using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace DispatcherWeb.Storage
{
    [Table("DeferredBinaryObject")]
    public class DeferredBinaryObject : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public DeferredBinaryObjectDestination Destination { get; set; }
        public Guid BinaryObjectId { get; set; }
    }
}
