using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Sessions.Dto;

namespace DispatcherWeb.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();

        Task<UpdateUserSignInTokenOutput> UpdateUserSignInToken();
    }
}
