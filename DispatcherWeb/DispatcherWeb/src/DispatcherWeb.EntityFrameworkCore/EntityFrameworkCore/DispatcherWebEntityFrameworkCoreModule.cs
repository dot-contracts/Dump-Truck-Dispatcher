using System;
using System.Linq;
using System.Transactions;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Configuration;
using Abp.EntityFrameworkCore.Uow;
using Abp.IdentityServer4vNext;
using Abp.Modules;
using Abp.MultiTenancy;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using DispatcherWeb.Configuration;
using DispatcherWeb.Migrations.Seed;

namespace DispatcherWeb.EntityFrameworkCore
{
    [DependsOn(
        typeof(AbpZeroCoreEntityFrameworkCoreModule),
        typeof(DispatcherWebCoreModule),
        typeof(AbpZeroCoreIdentityServervNextEntityFrameworkCoreModule)
        )]
    public class DispatcherWebEntityFrameworkCoreModule : AbpModule
    {
        /* Used it tests to skip dbcontext registration, in order to use in-memory database of EF Core */
        public bool SkipDbContextRegistration { get; set; }

        public bool SkipDbSeed { get; set; }

        public override void PreInitialize()
        {
            if (!SkipDbContextRegistration)
            {
                Configuration.Modules.AbpEfCore().AddDbContext<DispatcherWebDbContext>(options =>
                {
                    if (options.ExistingConnection != null)
                    {
                        DispatcherWebDbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                    }
                    else
                    {
                        DispatcherWebDbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                    }
                });
            }

            // Set this setting to true for enabling entity history.
            Configuration.EntityHistory.IsEnabled = false;
            // Uncomment below line to write change logs for the entities below:
            //Configuration.EntityHistory.Selectors.Add("DispatcherWebEntities", EntityHistoryHelper.TrackedTypes);
            //Configuration.CustomConfigProviders.Add(new EntityHistoryConfigProvider(Configuration));
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(DispatcherWebEntityFrameworkCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            var configurationAccessor = IocManager.Resolve<IAppConfigurationAccessor>();
            var runMigrations = configurationAccessor.Configuration["App:RunMigrations"] == "true";
            if (!runMigrations)
            {
                return;
            }

            using (var scope = IocManager.CreateScope())
            {
                var connectionString = configurationAccessor.Configuration["ConnectionStrings:Default"];
#pragma warning disable CS0618 // Type or member is obsolete - ignore sync call during startup
                if (!SkipDbSeed && scope.Resolve<DatabaseCheckHelper>().Exist(connectionString))
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    MigrateDatabase(scope);

                    if (configurationAccessor.Configuration["App:SeedHostOnStartup"] == "true")
                    {
                        Logger.Info("Running SeedHelper.SeedHostDb...");
#pragma warning disable CS0612 // Type or member is obsolete - ignore sync call during startup
                        SeedHelper.SeedHostDb(IocManager);
#pragma warning restore CS0612 // Type or member is obsolete
                    }

                    if (configurationAccessor.Configuration["App:SeedTenantsOnStartup"] == "true") //otherwise, they will only be seeded on tenant creation
                    {
                        Logger.Info("Running MigrateTenants...");
#pragma warning disable CS0612 // Type or member is obsolete - ignore sync call during startup
                        MigrateTenants(scope);
#pragma warning restore CS0612 // Type or member is obsolete
                    }
                    Logger.Info("Completed MigrateTenants");
                }
            }
        }

        private void MigrateDatabase(IScopedIocResolver scope)
        {
            var migrateExecuter = scope.Resolve<AbpZeroDbMigrator>();
#pragma warning disable CS0618 // Type or member is obsolete - we have to use sync method during startup
            migrateExecuter.CreateOrMigrateForHost();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Obsolete]
        private void MigrateTenants(IScopedIocResolver scope)
        {
            using var uowManager = IocManager.ResolveAsDisposable<IUnitOfWorkManager>();
            using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
            {
                var context = uowManager.Object.Current.GetDbContext<DispatcherWebDbContext>(MultiTenancySides.Host);
                var migrateExecuter = scope.Resolve<AbpZeroDbMigrator>();

                var tenants = context.Tenants.Where(t => !t.IsDeleted && !string.IsNullOrEmpty(t.ConnectionString)).ToList();
                foreach (var tenant in tenants)
                {
                    migrateExecuter.CreateOrMigrateForTenant(tenant);
                }

                uow.Complete();
            }
        }
    }
}
