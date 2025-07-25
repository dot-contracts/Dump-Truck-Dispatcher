namespace DispatcherWeb.Distance.Dto
{
    public interface ILocationDistance : ILocation
    {
        decimal? Distance { get; set; }
    }
}
