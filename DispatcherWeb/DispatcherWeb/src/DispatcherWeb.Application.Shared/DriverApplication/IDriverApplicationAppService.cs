using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.DriverApplication.Dto;

namespace DispatcherWeb.DriverApplication
{
    public interface IDriverApplicationAppService
    {
        Task<DriverAppInfo> GetDriverAppInfo(GetDriverAppInfoInput input);

        [RemoteService(false)]
        Task RemoveOldDriverApplicationLogs();
    }
}
