using System.Collections.Generic;
using System.Threading.Tasks;

namespace DispatcherWeb.DriverApplication
{
    public interface IDriverApplicationLogger
    {
        Task LogInfo(int driverId, string message);
        Task LogInfo(List<int> driverIds, string message);
        Task LogWarn(int driverId, string message);
        Task LogWarn(List<int> driverIds, string message);
        Task LogError(int driverId, string message);
        Task LogError(List<int> driverIds, string message);
    }
}
