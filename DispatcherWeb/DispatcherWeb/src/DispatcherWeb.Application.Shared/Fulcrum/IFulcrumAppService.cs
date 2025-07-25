using System.Threading.Tasks;

namespace DispatcherWeb.Fulcrum
{
    public interface IFulcrumAppService
    {
        Task SyncFulcrumEntityAsync(FulcrumEntity entity);
        Task CreateDtdTicketToToFulcrum(int id);
        Task DeleteDtdTicketFromFulcrum(int id);

    }
}
