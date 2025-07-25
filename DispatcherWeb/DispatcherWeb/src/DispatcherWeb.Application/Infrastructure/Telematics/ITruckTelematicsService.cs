using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.Infrastructure.Telematics.Dto;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Infrastructure.Telematics
{
    public interface ITruckTelematicsService
    {
        Task<List<TruckCurrentData>> GetCurrentDataForAllTrucksAsync();
        Task<bool> AreSettingsEmptyAsync();
        IQueryable<Truck> GetTrucksQueryByTruckCodesOrUniqueIds(IQueryable<Truck> query, List<string> truckCodesOrUniqueIds);
        Truck PickTruckByTruckCodeOrUniqueId(List<Truck> trucks, string truckCodeOrUniqueId);
    }
}
