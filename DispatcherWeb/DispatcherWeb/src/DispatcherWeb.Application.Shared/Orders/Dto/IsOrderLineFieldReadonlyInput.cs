namespace DispatcherWeb.Orders.Dto
{
    public class IsOrderLineFieldReadonlyInput
    {
        public int OrderLineId { get; set; }
        public string FieldName { get; set; }
    }
}
