using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Events.Bus.Handlers;
using Abp.Runtime.Caching;

namespace DispatcherWeb.Caching
{
    public class CacheInvalidationEventHandler : ISingletonDependency,
        IAsyncEventHandler<CacheInvalidationInstructionEventData>,
        IAsyncEventHandler<CacheInvalidationInstructionsEventData>
    {
        private readonly ICacheManager _cacheManager;

        public CacheInvalidationEventHandler(
            ICacheManager cacheManager
        )
        {
            _cacheManager = cacheManager;
        }

        public async Task HandleEventAsync(CacheInvalidationInstructionEventData eventData)
        {
            await InvalidateCache(eventData.Instruction);
        }

        public async Task HandleEventAsync(CacheInvalidationInstructionsEventData eventData)
        {
            foreach (var instruction in eventData.Instructions)
            {
                await InvalidateCache(instruction);
            }
        }

        private async Task InvalidateCache(CacheInvalidationInstruction instruction)
        {
            if (_cacheManager is RedisInvalidatableInMemoryCacheManager cacheManager)
            {
                await cacheManager.InvalidateCache(instruction);
            }
        }
    }
}
