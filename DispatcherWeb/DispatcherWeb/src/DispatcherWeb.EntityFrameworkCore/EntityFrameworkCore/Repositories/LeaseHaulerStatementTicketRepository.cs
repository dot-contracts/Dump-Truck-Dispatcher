using System.Threading.Tasks;
using Abp.EntityFrameworkCore;
using DispatcherWeb.LeaseHaulerStatements;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore.Repositories
{
    public class LeaseHaulerStatementTicketRepository : DispatcherWebRepositoryBase<LeaseHaulerStatementTicket>, ILeaseHaulerStatementTicketRepository
    {
        public LeaseHaulerStatementTicketRepository(IDbContextProvider<DispatcherWebDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<int> DeleteAllForLeaseHaulerStatementIdAsync(int leaseHaulerStatementId, int tenantId)
        {
            var context = await GetContextAsync();
            var rowsAffected = await context.Database.ExecuteSqlInterpolatedAsync($@"
                Delete from dbo.[LeaseHaulerStatementTicket] where TenantId = {tenantId} and LeaseHaulerStatementId = {leaseHaulerStatementId}
            ");

            return rowsAffected;
        }
    }
}
