using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Editions.Dto;
using DispatcherWeb.MultiTenancy.Dto;

namespace DispatcherWeb.MultiTenancy
{
    public interface ITenantRegistrationAppService : IApplicationService
    {
        Task<RegisterTenantOutput> RegisterTenant(RegisterTenantInput input);

        Task<EditionsSelectOutput> GetEditionsForSelect();

        Task<EditionSelectDto> GetEdition(int editionId);
    }
}