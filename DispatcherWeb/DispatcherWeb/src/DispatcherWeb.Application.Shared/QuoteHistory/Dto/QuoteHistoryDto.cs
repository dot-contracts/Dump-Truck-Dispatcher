using System;

namespace DispatcherWeb.QuoteHistory.Dto
{
    public class QuoteHistoryDto
    {
        public int Id { get; set; }
        public int QuoteId { get; set; }
        public string QuoteName { get; set; }
        public DateTime DateTime { get; set; }
        public string ChangedByUserName { get; set; }
        public QuoteChangeType ChangeType { get; set; }
        public string ChangeTypeName => ChangeType.GetDisplayName();
    }
}
