using System.Threading;
using System.Threading.Tasks;
using DispatcherWeb.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DispatcherWeb.HealthChecks
{
    public class DispatcherWebDbContextHealthCheck : IHealthCheck
    {
        private readonly DatabaseCheckHelper _checkHelper;

        public DispatcherWebDbContextHealthCheck(DatabaseCheckHelper checkHelper)
        {
            _checkHelper = checkHelper;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            if (await _checkHelper.ExistAsync("db"))
            {
                return HealthCheckResult.Healthy("DispatcherWebDbContext connected to database.");
            }

            return HealthCheckResult.Unhealthy("DispatcherWebDbContext could not connect to database");
        }
    }
}
