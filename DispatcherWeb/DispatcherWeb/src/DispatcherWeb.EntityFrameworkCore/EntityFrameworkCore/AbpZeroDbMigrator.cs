using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Transactions;
using Abp.Data;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.MultiTenancy;
using Abp.Zero.EntityFrameworkCore;
using Castle.Core.Logging;
using DispatcherWeb.Migrations.Seed.Tenants;

namespace DispatcherWeb.EntityFrameworkCore
{
    public class AbpZeroDbMigrator : AbpZeroDbMigrator<DispatcherWebDbContext>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly DbMigratorConnectionStringResolver _connectionStringResolver;
        private readonly IDbContextResolver _dbContextResolver;

        public AbpZeroDbMigrator(
            ILogger logger,
            IUnitOfWorkManager unitOfWorkManager,
            DbMigratorConnectionStringResolver connectionStringResolver,
            IDbContextResolver dbContextResolver) :
            base(
                unitOfWorkManager,
                connectionStringResolver,
                dbContextResolver)
        {
            _logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
            _connectionStringResolver = connectionStringResolver;
            _dbContextResolver = dbContextResolver;
        }

        [Obsolete("Use CreateOrMigrateForTenantAsync instead")]
        public override void CreateOrMigrateForTenant(AbpTenantBase tenant, Action<DispatcherWebDbContext> seedAction)
        {
            _logger.Info("Running CreateOrMigrateForTenant for tenantId " + tenant.Id);
            base.CreateOrMigrateForTenant(tenant, seedAction);
            SeedTenant(tenant);
            _logger.Info("Finished CreateOrMigrateForTenant");
        }

        public override async Task CreateOrMigrateForTenantAsync(AbpTenantBase tenant, Func<DispatcherWebDbContext, Task> seedAction)
        {
            _logger.Info("Running CreateOrMigrateForTenant for tenantId " + tenant.Id);
            await base.CreateOrMigrateForTenantAsync(tenant, seedAction);
            await SeedTenantAsync(tenant);
            _logger.Info("Finished CreateOrMigrateForTenant");
        }

        [Obsolete]
        private void SeedTenant(AbpTenantBase tenant)
        {
            var args = new DbPerTenantConnectionStringResolveArgs(
                tenant == null ? (int?)null : (int?)tenant.Id,
                tenant == null ? MultiTenancySides.Host : MultiTenancySides.Tenant
            );

            args["DbContextType"] = typeof(DispatcherWebDbContext);
            args["DbContextConcreteType"] = typeof(DispatcherWebDbContext);

            var nameOrConnectionString = ConnectionStringHelper.GetConnectionString(
                _connectionStringResolver.GetNameOrConnectionString(args)
            );

            using (var uow = _unitOfWorkManager.Begin(TransactionScopeOption.Suppress))
            {
                using (var context = _dbContextResolver.Resolve<DispatcherWebDbContext>(nameOrConnectionString, null))
                {
                    Debug.Assert(tenant != null, nameof(tenant) + " != null");
                    new DefaultLocationsCreator(context, tenant.Id).Create();
                    new DefaultServiceCreator(context, tenant.Id).Create();
                    new DefaultUnitOfMeasureCreator(context, tenant.Id).Create();
                    new DefaultTimeClassificationCreator(context, tenant.Id).Create();
                }

                _unitOfWorkManager.Current.SaveChanges();
                uow.Complete();
            }
        }

        private async Task SeedTenantAsync(AbpTenantBase tenant)
        {
            var args = new DbPerTenantConnectionStringResolveArgs(
                tenant == null ? (int?)null : (int?)tenant.Id,
                tenant == null ? MultiTenancySides.Host : MultiTenancySides.Tenant
            );

            args["DbContextType"] = typeof(DispatcherWebDbContext);
            args["DbContextConcreteType"] = typeof(DispatcherWebDbContext);

            var nameOrConnectionString = ConnectionStringHelper.GetConnectionString(
                await _connectionStringResolver.GetNameOrConnectionStringAsync(args)
            );

            await _unitOfWorkManager.WithUnitOfWorkAsync(TransactionScopeOption.Suppress, async () =>
            {
                using (var context = _dbContextResolver.Resolve<DispatcherWebDbContext>(nameOrConnectionString, null))
                {
                    Debug.Assert(tenant != null, nameof(tenant) + " != null");
                    await new DefaultLocationsCreator(context, tenant.Id).CreateAsync();
                    await new DefaultServiceCreator(context, tenant.Id).CreateAsync();
                    await new DefaultUnitOfMeasureCreator(context, tenant.Id).CreateAsync();
                    await new DefaultTimeClassificationCreator(context, tenant.Id).CreateAsync();
                }

                await _unitOfWorkManager.Current.SaveChangesAsync();
            });
        }
    }
}
