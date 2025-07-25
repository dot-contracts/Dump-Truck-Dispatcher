using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using JetBrains.Annotations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore.Repositories
{
    public static class DatabaseFacadeExtensions
    {
        public static async Task MergeEntitiesAsync(
            [NotNull] this DispatcherWebDbContext db,
            string entityName,
            string columnName,
            int? tenantId,
            int mainEntityId,
            IList<int> mergingEntityIds
        )
        {
            string tenantCondition = tenantId.HasValue ? $"= {tenantId.Value}" : "IS NULL";
            string parameterPlaceHolders = Enumerable.Range(1, mergingEntityIds.Count).Select(x => $"@p{x}").JoinAsString(",");
            string sql = $"UPDATE [{entityName}] SET [{columnName}] = @p0 WHERE TenantId {tenantCondition} AND [{columnName}] IN ({parameterPlaceHolders})";
            var parameters = CreateSqlParameters(mainEntityId, mergingEntityIds);
#pragma warning disable EF1000
            await db.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
#pragma warning restore EF1000
        }

        private static List<SqlParameter> CreateSqlParameters(int mainEntityId, IList<int> mergingEntityIds)
        {
            List<SqlParameter> parameters = new List<SqlParameter> { new SqlParameter("p0", mainEntityId) };
            int i = 1;
            foreach (int recordId in mergingEntityIds)
            {
                parameters.Add(new SqlParameter($"p{i++}", recordId));
            }

            return parameters;
        }

    }
}
