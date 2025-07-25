using DispatcherWeb.Dto;

namespace DispatcherWeb.FuelSurchargeCalculations.Dto
{
    public class GetFuelSurchargeCalculationsSelectListInput : GetSelectListInput
    {
        public bool? CanChangeBaseFuelCost { get; set; }
    }
}
