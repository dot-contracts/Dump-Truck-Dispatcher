using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp;
using Abp.Domain.Entities;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.Storage
{
    [Table("AppBinaryObjects")]
    public class BinaryObject : Entity<Guid>, IMayHaveTenant
    {
        public virtual int? TenantId { get; set; }

        [StringLength(EntityStringFieldLengths.BinaryObject.Description)]
        public virtual string Description { get; set; }

        [Required]
        [MaxLength(BinaryObjectConsts.BytesMaxSize)]
        public virtual byte[] Bytes { get; set; }

        public BinaryObject()
        {
            Id = SequentialGuidGenerator.Instance.Create();
        }

        public BinaryObject(int? tenantId, byte[] bytes, string description = null)
            : this()
        {
            TenantId = tenantId;
            Bytes = bytes;
            Description = description;
        }
    }
}
