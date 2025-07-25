using System.Collections.Generic;

namespace DispatcherWeb.Scheduling.Dto
{
    public class CopyOrderTrucksResult
    {
        public List<string> ConflictingTrucks { get; set; }
        public bool Completed { get; set; }
        public bool SomeTrucksAreNotCopied { get; set; }
    }
}
