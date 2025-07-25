using System.Threading.Tasks;
using DispatcherWeb.Sessions.Dto;

namespace DispatcherWeb.Web.Session
{
    public interface IPerRequestSessionCache
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformationsAsync();
    }
}
