using DispatcherWeb.Dto;

namespace DispatcherWeb.Locations.Dto
{
    public class GetLocationForEditInput : NullableIdNameDto
    {
        public bool Temporary { get; set; }

        public bool MergeWithDuplicateSilently { get; set; } // will be set to true for quick location adds, so that app.createOrEditLocationModalSaved event is fired with a found duplicate info and no warning is shown to the user
    }
}
