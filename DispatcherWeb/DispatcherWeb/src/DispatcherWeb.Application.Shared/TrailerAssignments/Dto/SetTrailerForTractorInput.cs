namespace DispatcherWeb.TrailerAssignments.Dto
{
    public class SetTrailerForTractorInput : TrailerAssignmentInputBase
    {
        public int TractorId { get; set; }
        public int? TrailerId { get; set; }
        public bool UpdateExistingOrderLineTrucks { get; set; }
    }
}
