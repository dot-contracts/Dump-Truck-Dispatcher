namespace DispatcherWeb.Trucks.Dto
{
    public class SetTruckIsOutOfServiceResult
    {
        public bool ThereWereAssociatedOrders { get; set; }
        public bool ThereWereCanceledDispatches { get; set; }
        public bool ThereWereNotCanceledDispatches { get; set; }
        public bool ThereWereAssociatedTractors { get; set; }
    }
}
