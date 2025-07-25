using System.Collections.Generic;

namespace DispatcherWeb.Dto
{
    public class DataMergeInput
    {
        public List<int> IdsToMerge { get; set; }
        public int MainRecordId { get; set; }
    }
}
