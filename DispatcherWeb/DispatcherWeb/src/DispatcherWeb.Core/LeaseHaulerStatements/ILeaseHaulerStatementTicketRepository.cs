using System.Threading.Tasks;
using Abp.Domain.Repositories;

namespace DispatcherWeb.LeaseHaulerStatements
{
    public interface ILeaseHaulerStatementTicketRepository : IRepository<LeaseHaulerStatementTicket>
    {
        Task<int> DeleteAllForLeaseHaulerStatementIdAsync(int leaseHaulerStatementId, int tenantId);
    }
}
