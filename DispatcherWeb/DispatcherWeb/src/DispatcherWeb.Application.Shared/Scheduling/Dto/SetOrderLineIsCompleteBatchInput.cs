using System.Collections.Generic;

namespace DispatcherWeb.Scheduling.Dto
{
    public class SetOrderLineIsCompleteBatchInput
    {
        public List<int> OrderLineIds { get; set; }
        public bool IsComplete { get; set; }
        public bool IsCancelled { get; set; }
    }
} 