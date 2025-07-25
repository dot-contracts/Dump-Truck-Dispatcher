namespace DispatcherWeb.Dashboard.Dto
{
    public class GetTenantDashboardStatusDataOutput
    {
        public int NoData { get; set; }
        public int Overdue { get; set; }
        public int Due { get; set; }
        public int Ok { get; set; }
    }
}
