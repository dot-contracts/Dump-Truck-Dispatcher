using System.Collections.Generic;

namespace DispatcherWeb.Trucks.Dto
{
    public class SyncWithWialonInput
    {
        /// <summary>
        /// If any LocalTruckIds are specified, only these trucks will be added to wialon, and none of wialon trucks will be added locally
        /// </summary>
        public List<int> LocalTruckIds { get; set; }
        public bool IncreaseNumberOfTrucksIfNeeded { get; set; }
    }
}
