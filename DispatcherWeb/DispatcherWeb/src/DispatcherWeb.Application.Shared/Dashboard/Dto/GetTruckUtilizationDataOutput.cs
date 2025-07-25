using System.Collections.Generic;

namespace DispatcherWeb.Dashboard.Dto
{
    public class GetTruckUtilizationDataOutput
    {
        public GetTruckUtilizationDataOutput(List<TruckUtilizationData> truckUtilizationData)
        {
            TruckUtilizationData = truckUtilizationData;
        }
        public List<TruckUtilizationData> TruckUtilizationData { get; set; }
    }
}
