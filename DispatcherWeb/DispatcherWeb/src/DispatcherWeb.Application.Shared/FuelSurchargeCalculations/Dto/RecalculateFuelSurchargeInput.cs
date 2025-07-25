using System;

namespace DispatcherWeb.FuelSurchargeCalculations.Dto
{
    public class RecalculateFuelSurchargeInput
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? OrderLineId { get; set; }
        public int? OrderId { get; set; }

        public void ValidateInput()
        {
            if (StartDate == null && OrderLineId == null && OrderId == null)
            {
                throw new ArgumentNullException(nameof(StartDate), "At least one of (StartDate, OrderLineId, OrderId) has to be provided");
            }
        }
    }
}
