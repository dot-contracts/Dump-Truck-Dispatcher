using System.Collections.Generic;

namespace DispatcherWeb.BackgroundJobs
{
    public class MigrateToSeparateMaterialAndFreightItemsBackgroundJobArgs
    {
        public List<int> TenantIds { get; set; }
        public bool SeparateMaterialAndFreightItems { get; set; }
    }
}
