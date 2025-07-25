namespace DispatcherWeb.Orders.Dto
{
    public class DeleteOrderLineTrucksInput
    {
        public int OrderLineId { get; set; }

        public bool MarkAsDone { get; set; }
    }
}
