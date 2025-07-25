using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.TempFiles.Dto
{
    public class TempFileDto : FullAuditedEntity, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid FileGuid { get; set; }

        [Required]
        [StringLength(EntityStringFieldLengths.TempFile.FileName)]
        public string FileName { get; set; }

        [Required]
        [StringLength(EntityStringFieldLengths.TempFile.MimeType)]
        public string MimeType { get; set; }

        [Required]
        public DateTime ExpirationDateTime { get; set; }
    }
}
