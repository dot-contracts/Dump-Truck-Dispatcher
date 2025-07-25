using System.ComponentModel.DataAnnotations;
using DispatcherWeb.Infrastructure;

namespace DispatcherWeb.FuelSurchargeCalculations.Dto
{
    public class FuelSurchargeCalculationEditDto
    {
        public int Id { get; set; }

        [StringLength(EntityStringFieldLengths.FuelSurchargeCalculation.Name)]
        public string Name { get; set; }

        public FuelSurchargeCalculationType Type { get; set; }

        public decimal BaseFuelCost { get; set; }

        public bool CanChangeBaseFuelCost { get; set; }

        public decimal Increment { get; set; }

        public decimal FreightRatePercent { get; set; }

        public bool Credit { get; set; }
    }
}
