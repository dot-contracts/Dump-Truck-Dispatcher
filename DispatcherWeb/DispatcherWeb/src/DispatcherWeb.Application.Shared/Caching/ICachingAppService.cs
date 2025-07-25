using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Caching.Dto;

namespace DispatcherWeb.Caching
{
    public interface ICachingAppService : IApplicationService
    {
        Task<ListResultDto<CacheDto>> GetAllCaches();

        Task ClearCache(EntityDto<string> input);

        Task ClearAllCaches();
    }
}
