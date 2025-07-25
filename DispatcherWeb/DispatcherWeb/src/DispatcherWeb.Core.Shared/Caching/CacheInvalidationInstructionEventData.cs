using Abp.Events.Bus;

namespace DispatcherWeb.Caching
{
    public class CacheInvalidationInstructionEventData : EventData
    {
        public CacheInvalidationInstruction Instruction { get; set; }

        public CacheInvalidationInstructionEventData()
        {
        }

        public CacheInvalidationInstructionEventData(CacheInvalidationInstruction instruction)
        {
            Instruction = instruction;
        }
    }
}
