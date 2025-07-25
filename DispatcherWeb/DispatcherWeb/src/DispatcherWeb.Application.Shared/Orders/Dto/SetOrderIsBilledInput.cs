namespace DispatcherWeb.Orders.Dto
{
    public class SetOrderIsBilledInput
    {
        public int OrderId { get; set; }
        public bool IsBilled { get; set; }
    }
}
