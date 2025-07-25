using DispatcherWeb.Configuration;
using DispatcherWeb.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class DispatcherWebDbContextFactory : IDesignTimeDbContextFactory<DispatcherWebDbContext>
    {
        public DispatcherWebDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<DispatcherWebDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder(), addUserSecrets: true);

            DispatcherWebDbContextConfigurer.Configure(builder, configuration.GetConnectionString(DispatcherWebConsts.ConnectionStringName));

            return new DispatcherWebDbContext(builder.Options);
        }
    }
}