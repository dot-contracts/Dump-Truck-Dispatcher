using Azure.Data.Tables;

namespace DispatcherWeb.Infrastructure.AzureTables
{
    public interface IAzureTableManager
    {
        TableClient GetTableClient(string tableName);
    }
}
