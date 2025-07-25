namespace DispatcherWeb.Dispatching.Dto
{
    public class AddDriverNoteInput
    {
        public int OrderLineTruckId { get; set; }

        public string DriverNote { get; set; }
    }
}
