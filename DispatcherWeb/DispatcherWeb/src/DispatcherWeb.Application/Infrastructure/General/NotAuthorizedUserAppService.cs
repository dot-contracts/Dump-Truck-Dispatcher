using System.Threading.Tasks;
using Abp.MultiTenancy;

namespace DispatcherWeb.Infrastructure.General
{
    public class NotAuthorizedUserAppService : DispatcherWebDomainServiceBase, INotAuthorizedUserAppService
    {
        private readonly ITenantCache _tenantCache;

        public NotAuthorizedUserAppService(
            ITenantCache tenantCache
        )
        {
            _tenantCache = tenantCache;
        }

        public async Task<string> GetTenancyNameOrNullAsync(int? tenantId)
        {
            if (tenantId == null)
            {
                return null;
            }
            return (await _tenantCache.GetAsync(tenantId.Value)).TenancyName;
        }


    }
}
