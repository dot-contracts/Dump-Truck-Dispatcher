using System.Collections.Generic;

namespace DispatcherWeb.Dashboard.RevenueGraph.Dto
{
    public class GetRevenueGraphDataOutput
    {
        public GetRevenueGraphDataOutput(List<RevenueGraphData> revenueGraphData)
        {
            RevenueGraphData = revenueGraphData;
        }

        public List<RevenueGraphData> RevenueGraphData { get; set; }
    }
}
