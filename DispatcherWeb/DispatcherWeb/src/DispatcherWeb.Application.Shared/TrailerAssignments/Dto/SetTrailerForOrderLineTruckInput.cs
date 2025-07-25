namespace DispatcherWeb.TrailerAssignments.Dto
{
    public class SetTrailerForOrderLineTruckInput
    {
        public int OrderLineTruckId { get; set; }
        public int? TrailerId { get; set; }
    }
}
