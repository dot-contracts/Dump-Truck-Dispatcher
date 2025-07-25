namespace DispatcherWeb.Scheduling.Dto
{
    public class AddOrderLineTruckInternalInput : AddOrderLineTruckInput
    {
        public AddOrderLineTruckInternalInput()
        {

        }
        public AddOrderLineTruckInternalInput(AddOrderLineTruckInput addOrderLineTruckInput, decimal utilization)
        {
            base.TruckId = addOrderLineTruckInput.TruckId;
            base.OrderLineId = addOrderLineTruckInput.OrderLineId;
            base.ParentId = addOrderLineTruckInput.ParentId;
            base.DriverId = addOrderLineTruckInput.DriverId;
            base.TrailerId = addOrderLineTruckInput.TrailerId;
            Utilization = utilization;
        }
        public decimal Utilization { get; set; }
    }
}
