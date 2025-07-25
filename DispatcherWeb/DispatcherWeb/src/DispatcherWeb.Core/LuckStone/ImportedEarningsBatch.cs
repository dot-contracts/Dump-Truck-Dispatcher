using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.LuckStone
{
    [Table("ImportedEarningsBatch")]
    public class ImportedEarningsBatch : FullAuditedEntity, IMustHaveTenant
    {
        public int TenantId { get; set; }

        [StringLength(200)]
        public string FilePath { get; set; }

        public TicketImportType TicketImportType { get; set; }

        public List<ImportedEarnings> ImportedEarnings { get; set; }
    }
}
