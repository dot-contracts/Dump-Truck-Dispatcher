using System.Threading.Tasks;
using DispatcherWeb.DriverAssignments.Dto;

namespace DispatcherWeb.DriverAssignments
{
    public interface INotifyDriversAppService
    {
        Task<bool> NotifyDrivers(NotifyDriversInput input);
    }
}