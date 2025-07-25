using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace DispatcherWeb.Quotes
{
    [Table("QuoteFieldDiff")]
    public class QuoteFieldDiff : FullAuditedEntity, IMustHaveTenant
    {
        public QuoteFieldDiff()
        {
        }

        public QuoteFieldDiff(QuoteFieldEnum field, string oldValue, string newValue)
        {
            Field = field;
            OldDisplayValue = oldValue;
            NewDisplayValue = newValue;
        }

        public QuoteFieldDiff(QuoteFieldEnum field, int? oldValue, int? newValue)
        {
            Field = field;
            OldId = oldValue;
            NewId = newValue;
        }

        public QuoteFieldDiff(QuoteFieldEnum field, int? oldId, string oldValue, int? newId, string newValue)
            : this(field, oldId, newId)
        {
            OldDisplayValue = oldValue;
            NewDisplayValue = newValue;
        }

        public int TenantId { get; set; }

        public int QuoteHistoryRecordId { get; set; }

        public QuoteFieldEnum Field { get; set; }

        public int? OldId { get; set; }

        public int? NewId { get; set; }

        public string OldDisplayValue { get; set; }

        public string NewDisplayValue { get; set; }

        public virtual QuoteHistoryRecord QuoteHistoryRecord { get; set; }
    }
}
