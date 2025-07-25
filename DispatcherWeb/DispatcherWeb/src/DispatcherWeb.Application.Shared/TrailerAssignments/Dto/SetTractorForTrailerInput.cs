namespace DispatcherWeb.TrailerAssignments.Dto
{
    public class SetTractorForTrailerInput : TrailerAssignmentInputBase
    {
        public int TrailerId { get; set; }
        public int? TractorId { get; set; }
        public string TractorTruckCode { get; set; }
    }
}
