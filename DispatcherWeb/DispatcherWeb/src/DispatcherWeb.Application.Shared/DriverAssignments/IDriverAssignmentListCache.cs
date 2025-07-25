using DispatcherWeb.Caching;
using DispatcherWeb.DriverAssignments.Dto;

namespace DispatcherWeb.DriverAssignments
{
    public interface IDriverAssignmentListCache : IListCache<ListCacheDateKey, DriverAssignmentCacheItem>
    {
    }
}
