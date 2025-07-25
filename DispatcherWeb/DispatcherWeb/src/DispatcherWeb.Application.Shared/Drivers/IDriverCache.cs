using System.Threading.Tasks;
using DispatcherWeb.Drivers.Dto;

namespace DispatcherWeb.Drivers
{
    public interface IDriverCache
    {
        Task<DriverCacheItem> GetDriverFromCacheOrDefault(int driverId);
        Task InvalidateCache();
        Task InvalidateCache(int driverId);
    }
}
