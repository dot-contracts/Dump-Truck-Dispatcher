namespace DispatcherWeb.Trucks.Dto
{
    public class EditTruckResult
    {
        public int Id { get; set; }

        public bool ThereWereCanceledDispatches { get; set; }
        public bool ThereWereNotCanceledDispatches { get; set; }
        public bool ThereAreOrdersInTheFuture { get; set; }
        public bool ThereWereAssociatedOrders { get; set; }
        public bool ThereWereAssociatedTractors { get; set; }
        public int NeededBiggerNumberOfTrucks { get; set; }
    }
}
