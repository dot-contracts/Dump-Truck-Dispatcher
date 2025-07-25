using System.Threading.Tasks;
using Abp.Domain.Repositories;

namespace DispatcherWeb.PayStatements
{
    public interface IPayStatementRepository : IRepository<PayStatement>
    {
        Task<int> DeleteWithAllChildRecordsAsync(int payStatementId, int tenantId);
    }
}
