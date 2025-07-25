using System;
using System.Collections.Generic;
using Abp.Data;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Encryption;
using Abp.Extensions;
using Abp.MultiTenancy;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Migrations.Seed;
using DispatcherWeb.MultiTenancy;

namespace DispatcherWeb.Migrator
{
    public class MultiTenantMigrateExecuter : ITransientDependency
    {
        public Log Log { get; private set; }

        private readonly AbpZeroDbMigrator _migrator;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IDbPerTenantConnectionStringResolver _connectionStringResolver;

        public MultiTenantMigrateExecuter(
            AbpZeroDbMigrator migrator,
            IRepository<Tenant> tenantRepository,
            IEncryptionService encryptionService,
            Log log,
            DbMigratorConnectionStringResolver connectionStringResolver)
        {
            Log = log;

            _migrator = migrator;
            _tenantRepository = tenantRepository;
            _encryptionService = encryptionService;
            _connectionStringResolver = connectionStringResolver;
        }

        public void Run(bool skipConnVerification)
        {
#pragma warning disable CS0612 // Type or member is obsolete - ignore sync call for now
            var hostConnStr = _connectionStringResolver.GetNameOrConnectionString(new ConnectionStringResolveArgs(MultiTenancySides.Host));
#pragma warning restore CS0612 // Type or member is obsolete
            if (hostConnStr.IsNullOrWhiteSpace())
            {
                Log.Write("Configuration file should contain a connection string named 'Default'");
                return;
            }

            Log.Write("Host database: " + ConnectionStringHelper.GetConnectionString(hostConnStr));
            if (!skipConnVerification)
            {
                Log.Write("Continue to migration for this host database and all tenants..? (Y/N): ");
                var command = Console.ReadLine();
                if (!command.IsIn("Y", "y"))
                {
                    Log.Write("Migration canceled.");
                    return;
                }
            }

            Log.Write("HOST database migration started...");

            try
            {
#pragma warning disable CS0618 // Type or member is obsolete - synchronous migration call in a synchronous method
                _migrator.CreateOrMigrateForHost(SeedHelper.SeedHostDb);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            catch (Exception ex)
            {
                Log.Write("An error occurred during migration of host database:");
                Log.Write(ex.ToString());
                Log.Write("Canceled migrations.");
                return;
            }

            Log.Write("HOST database migration completed.");
            Log.Write("--------------------------------------------------------");

            var migratedDatabases = new HashSet<string>();
#pragma warning disable CS0612 // Type or member is obsolete - async migration is not available
            var tenants = _tenantRepository.GetAllList(t => t.ConnectionString != null && t.ConnectionString != "");
#pragma warning restore CS0612 // Type or member is obsolete
            for (int i = 0; i < tenants.Count; i++)
            {
                var tenant = tenants[i];
                Log.Write(string.Format("Tenant database migration started... ({0} / {1})", (i + 1), tenants.Count));
                Log.Write("Name              : " + tenant.Name);
                Log.Write("TenancyName       : " + tenant.TenancyName);
                Log.Write("Tenant Id         : " + tenant.Id);
                Log.Write("Connection string : " + _encryptionService.DecryptIfNotEmpty(tenant.ConnectionString));

                if (!migratedDatabases.Contains(tenant.ConnectionString))
                {
                    try
                    {
#pragma warning disable CS0618 // Type or member is obsolete - synchronous migration
                        _migrator.CreateOrMigrateForTenant(tenant);
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    catch (Exception ex)
                    {
                        Log.Write("An error occurred during migration of tenant database:");
                        Log.Write(ex.ToString());
                        Log.Write("Skipped this tenant and will continue for others...");
                    }

                    migratedDatabases.Add(tenant.ConnectionString);
                }
                else
                {
                    Log.Write("This database has already migrated before (you have more than one tenant in same database). Skipping it....");
                }

                Log.Write(string.Format("Tenant database migration completed. ({0} / {1})", (i + 1), tenants.Count));
                Log.Write("--------------------------------------------------------");
            }

            Log.Write("All databases have been migrated.");
        }
    }
}
