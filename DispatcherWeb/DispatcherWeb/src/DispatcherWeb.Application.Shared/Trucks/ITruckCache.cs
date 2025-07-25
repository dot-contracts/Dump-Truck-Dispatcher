using System.Threading.Tasks;
using DispatcherWeb.Trucks.Dto;

namespace DispatcherWeb.Trucks
{
    public interface ITruckCache
    {
        Task<TruckCacheItem> GetTruckFromCacheOrDbOrDefault(int truckId);
        Task InvalidateCache();
        Task InvalidateCache(int truckId);
    }
}
