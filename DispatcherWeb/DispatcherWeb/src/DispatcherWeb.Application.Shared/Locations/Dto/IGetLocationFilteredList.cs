namespace DispatcherWeb.Locations.Dto
{
    public interface IGetLocationFilteredList
    {
        string Name { get; set; }
        int? CategoryId { get; set; }
        string City { get; set; }
        string State { get; set; }
        FilterActiveStatus Status { get; set; }
        bool WithCoordinates { get; set; }
    }
}
