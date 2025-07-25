namespace DispatcherWeb.Orders.Dto
{
    public class ResetOverriddenOrderLineValuesInput
    {
        public int Id { get; set; }
        public bool OverrideReadOnlyState { get; set; }
    }
}
