using System.Threading.Tasks;

namespace DispatcherWeb.Drivers
{
    public interface IDriverInactivatorService
    {
        Task InactivateDriverAsync(Driver driver, int? leaseHaulerId);
    }
}
