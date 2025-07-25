namespace DispatcherWeb.Scheduling.Dto
{
    public class MoveTruckResult
    {
        public bool Success => !OrderLineTruckExists;
        public bool OrderLineTruckExists { get; set; }
    }
}
