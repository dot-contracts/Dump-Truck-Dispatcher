namespace DispatcherWeb.DriverAssignments.Dto
{
    public class ThereAreOpenDispatchesForDriverOnDateResult
    {
        public bool ThereAreUnacknowledgedDispatches { get; set; }
        public bool ThereAreAcknowledgedDispatches { get; set; }
    }
}
