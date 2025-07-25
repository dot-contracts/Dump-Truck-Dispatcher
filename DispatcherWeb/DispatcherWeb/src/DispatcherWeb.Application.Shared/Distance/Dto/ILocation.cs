namespace DispatcherWeb.Distance.Dto
{
    public interface ILocation
    {
        string PlaceId { get; }

        decimal? Latitude { get; }
        decimal? Longitude { get; }

        string StreetAddress { get; }
        string City { get; }
        string State { get; }
        string ZipCode { get; }
        string CountryCode { get; }
    }
}
