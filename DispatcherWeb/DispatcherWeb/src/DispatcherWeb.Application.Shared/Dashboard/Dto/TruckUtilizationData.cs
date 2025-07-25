namespace DispatcherWeb.Dashboard.Dto
{
    public class TruckUtilizationData
    {
        public TruckUtilizationData(int utilizationPercent, string period)
        {
            UtilizationPercent = utilizationPercent;
            Period = period;
        }

        public int UtilizationPercent { get; set; }
        public string Period { get; set; }
    }
}
