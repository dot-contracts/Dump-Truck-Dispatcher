using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Telematics.Dto;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public abstract class TruckTelematicsServiceBase : ITruckTelematicsService
    {
        public abstract Task<List<TruckCurrentData>> GetCurrentDataForAllTrucksAsync();
        public abstract Task<bool> AreSettingsEmptyAsync();

        public virtual IQueryable<Truck> GetTrucksQueryByTruckCodesOrUniqueIds(IQueryable<Truck> query, List<string> truckCodesOrUniqueIds)
        {
            return query.Where(t => truckCodesOrUniqueIds.Contains(t.TruckCode));
        }

        public virtual Truck PickTruckByTruckCodeOrUniqueId(List<Truck> trucks, string truckCodeOrUniqueId)
        {
            return trucks.FirstOrDefault(t => t.TruckCode == truckCodeOrUniqueId);
        }
    }
}
