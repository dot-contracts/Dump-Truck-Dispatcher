using System.Collections.Generic;
using Abp.Events.Bus;

namespace DispatcherWeb.Caching
{
    public class CacheInvalidationInstructionsEventData : EventData
    {
        public List<CacheInvalidationInstruction> Instructions { get; set; }
    }
}
