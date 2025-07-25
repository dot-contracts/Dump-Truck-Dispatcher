using System.Collections.Generic;
using System.Threading.Tasks;
using DispatcherWeb.Caching;

namespace DispatcherWeb.Tests.TestInfrastructure
{
    public class NullCacheInvalidationService : ICacheInvalidationService
    {
        public void SendCacheInvalidationInstruction(string cacheName, string cacheKey = null, bool hardInvalidate = false)
        {
        }

        public Task SendCacheInvalidationInstructionAsync(string cacheName, string cacheKey = null, bool hardInvalidate = false)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<CacheInvalidationInstruction>> GetAllInvalidationInstructionsAsync()
        {
            return Task.FromResult<IReadOnlyList<CacheInvalidationInstruction>>(new List<CacheInvalidationInstruction>());
        }

        public Task ReceiveNewPersistentInstructions()
        {
            return Task.CompletedTask;
        }
    }
}
