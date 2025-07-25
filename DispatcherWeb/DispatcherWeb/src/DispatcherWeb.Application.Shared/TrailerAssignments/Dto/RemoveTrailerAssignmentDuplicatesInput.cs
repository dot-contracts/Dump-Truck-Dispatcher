namespace DispatcherWeb.TrailerAssignments.Dto
{
    public class RemoveTrailerAssignmentDuplicatesInput : TrailerAssignmentInputBase
    {
        public int? TractorId { get; set; }
        public int? TrailerId { get; set; }
    }
}
