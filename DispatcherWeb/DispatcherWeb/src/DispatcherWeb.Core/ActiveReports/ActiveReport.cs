using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.ActiveReports
{
    [Table("ActiveReport")]
    public partial class ActiveReport : FullAuditedEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }

        public int CategoryId { get; set; }
        public virtual ActiveReportCategory Category { get; set; }
    }
}
