using System.Collections.Generic;

namespace DispatcherWeb.Orders.Dto
{
    public class RecalculateStaggeredTimeForOrdersInput
    {
        public List<int> OrderIds { get; set; }
    }
}
