using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.General
{
    public interface INotAuthorizedUserAppService
    {
        Task<string> GetTenancyNameOrNullAsync(int? tenantId);
    }
}
