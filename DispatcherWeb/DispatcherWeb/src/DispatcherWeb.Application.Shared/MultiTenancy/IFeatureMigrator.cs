using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;

namespace DispatcherWeb.MultiTenancy
{
    public interface IFeatureMigrator
    {
        Task MigrateTenantIfNeeded(int tenantId, IReadOnlyCollection<NameValue> oldFeatureValues);
    }
}
