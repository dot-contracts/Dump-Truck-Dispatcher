using System.Collections.Generic;

namespace DispatcherWeb.QuoteHistory.Dto
{
    public class QuoteHistoryDetailsDto : QuoteHistoryDto
    {
        public List<QuoteFieldDiffDto> FieldDiffs { get; set; }
    }
}
