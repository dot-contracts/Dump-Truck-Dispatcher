namespace DispatcherWeb.Drivers.Dto
{
    public class ThereAreActiveDriversWithSameEmailInput
    {
        public string Email { get; set; }
        public int? ExceptDriverId { get; set; }
    }
}
