using System.Collections.Generic;
using System.Threading.Tasks;

namespace DispatcherWeb.Caching
{
    public interface ICacheInvalidationService
    {
        void SendCacheInvalidationInstruction(string cacheName, string cacheKey = null, bool hardInvalidate = false);
        Task SendCacheInvalidationInstructionAsync(string cacheName, string cacheKey = null, bool hardInvalidate = false);
        Task<IReadOnlyList<CacheInvalidationInstruction>> GetAllInvalidationInstructionsAsync();
        Task ReceiveNewPersistentInstructions();
    }
}
