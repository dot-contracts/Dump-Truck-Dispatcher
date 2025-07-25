using System.Collections.Generic;

namespace DispatcherWeb.Orders.Dto
{
    public class SetOrderDateResult
    {
        public bool Completed { get; set; }

        public List<string> NotAvailableTrucks { get; set; }
    }
}
