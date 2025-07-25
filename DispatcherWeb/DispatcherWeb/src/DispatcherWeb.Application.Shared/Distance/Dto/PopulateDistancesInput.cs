using System.Collections.Generic;

namespace DispatcherWeb.Distance.Dto
{
    public class PopulateDistancesInput
    {
        public IEnumerable<ILocationDistance> Sources { get; set; }

        public ILocation Destination { get; set; }

        public UnitOfMeasureBaseEnum UomBaseId { get; set; }
    }
}
