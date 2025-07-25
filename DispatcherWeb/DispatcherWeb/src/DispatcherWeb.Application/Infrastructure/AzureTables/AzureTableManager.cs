using Azure.Data.Tables;
using DispatcherWeb.Configuration;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Infrastructure.AzureTables
{
    public class AzureTableManager : DispatcherWebDomainServiceBase, IAzureTableManager
    {
        private readonly IAppConfigurationAccessor _configurationAccessor;

        public AzureTableManager(IAppConfigurationAccessor configurationAccessor)
        {
            _configurationAccessor = configurationAccessor;
        }

        private static string GetConnectionString(IConfigurationRoot configuration)
        {
            var connectionString = configuration["Abp:AzureTableConnectionString"];
            if (!string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }
            return configuration["Abp:StorageConnectionString"];
        }

        private static TableServiceClient GetTableServiceClient(IConfigurationRoot configuration)
        {
            return new TableServiceClient(
                connectionString: GetConnectionString(configuration)
            );
        }

        public TableClient GetTableClient(string tableName)
        {
            return GetTableClient(tableName, _configurationAccessor.Configuration);
        }

        private static TableClient GetTableClient(string tableName, IConfigurationRoot configuration)
        {
            var tableServiceClient = GetTableServiceClient(configuration);
            var tableClient = tableServiceClient.GetTableClient(
                tableName: tableName
            );

            return tableClient;
        }

        public static void CreateAllTables(IConfigurationRoot configuration)
        {
            foreach (var tableName in AzureTableNames.All)
            {
                var tableClient = GetTableClient(tableName, configuration);
                tableClient.CreateIfNotExists();
            }
        }
    }
}
