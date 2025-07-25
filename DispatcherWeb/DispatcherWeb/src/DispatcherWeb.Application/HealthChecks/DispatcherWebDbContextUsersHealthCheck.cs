using System;
using System.Threading;
using System.Threading.Tasks;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using DispatcherWeb.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DispatcherWeb.HealthChecks
{
    public class DispatcherWebDbContextUsersHealthCheck : IHealthCheck
    {
        private readonly IDbContextProvider<DispatcherWebDbContext> _dbContextProvider;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public DispatcherWebDbContextUsersHealthCheck(
            IDbContextProvider<DispatcherWebDbContext> dbContextProvider,
            IUnitOfWorkManager unitOfWorkManager
            )
        {
            _dbContextProvider = dbContextProvider;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                return await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    // Switching to host is necessary for single tenant mode.
                    using (_unitOfWorkManager.Current.SetTenantId(null))
                    {
                        var dbContext = await _dbContextProvider.GetDbContextAsync();
                        if (!await dbContext.Database.CanConnectAsync(cancellationToken))
                        {
                            return HealthCheckResult.Unhealthy(
                                "DispatcherWebDbContext could not connect to database"
                            );
                        }

                        var user = await dbContext.Users.AnyAsync(cancellationToken);

                        if (user)
                        {
                            return HealthCheckResult.Healthy("DispatcherWebDbContext connected to database and checked whether user added");
                        }

                        return HealthCheckResult.Unhealthy("DispatcherWebDbContext connected to database but there is no user.");
                    }
                });
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy("DispatcherWebDbContext could not connect to database.", e);
            }
        }
    }
}
