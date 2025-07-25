using System;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.Zero.EntityFrameworkCore;
using Castle.Core.Logging;
using DispatcherWeb.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.EntityFrameworkCore
{
    public class DbMigratorConnectionStringResolver : DbPerTenantConnectionStringResolver, IDbPerTenantConnectionStringResolver, ITransientDependency
    {
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IAbpStartupConfiguration _startupConfiguration;
        private readonly ILogger _logger;

        public DbMigratorConnectionStringResolver(
            IWebHostEnvironment env,
            IAbpStartupConfiguration startupConfiguration,
            ICurrentUnitOfWorkProvider currentUnitOfWorkProvider,
            ITenantCache tenantCache,
            ILogger logger)
            : base(startupConfiguration, currentUnitOfWorkProvider, tenantCache)
        {
            _appConfiguration = env.GetAppConfiguration();
            _startupConfiguration = startupConfiguration;
            _logger = logger;
        }

        [Obsolete]
        public override string GetNameOrConnectionString(DbPerTenantConnectionStringResolveArgs args)
        {
            if (args.TenantId == null)
            {
                var connectionString = _appConfiguration.GetConnectionString("Migrations");
                //_logger.Debug("(DbMigratorConnectionStringResolver) Migrations connection string is: " + connectionString);
                if (connectionString != null)
                {
                    return connectionString;
                }

                var defaultConnectionString = _startupConfiguration.DefaultNameOrConnectionString;
                //_logger.Debug("(DbMigratorConnectionStringResolver) DefaultNameOrConnectionString connection string is: " + defaultConnectionString);
                if (!string.IsNullOrWhiteSpace(defaultConnectionString))
                {
                    return defaultConnectionString;
                }

                connectionString = _appConfiguration.GetConnectionString("Default");
                //_logger.Debug("(DbMigratorConnectionStringResolver) Default connection string is: " + connectionString);
                if (connectionString != null)
                {
                    return connectionString;
                }

                if (System.Configuration.ConfigurationManager.ConnectionStrings.Count == 1)
                {
                    //_logger.Debug("(DbMigratorConnectionStringResolver) The only connection string is: " + ConfigurationManager.ConnectionStrings[0].ConnectionString);
                    return System.Configuration.ConfigurationManager.ConnectionStrings[0].ConnectionString;
                }
            }

            return base.GetNameOrConnectionString(args);
        }
    }
}
