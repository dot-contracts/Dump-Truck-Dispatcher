using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Editions.Dto;

namespace DispatcherWeb.Editions
{
    public interface IEditionAppService : IApplicationService
    {
        Task<ListResultDto<EditionListDto>> GetEditions();

        Task<GetEditionEditOutput> GetEditionForEdit(NullableIdDto input);

        Task CreateEdition(CreateEditionDto input);

        Task UpdateEdition(CreateEditionDto input);

        Task DeleteEdition(EntityDto input);

        Task MoveTenantsToAnotherEdition(MoveTenantsToAnotherEditionDto input);

        Task<List<SubscribableEditionComboboxItemDto>> GetEditionComboboxItems(int? selectedEditionId = null, bool addAllItem = false, bool onlyFree = false);

        Task<int> GetTenantCount(int editionId);

        Task CreateOrUpdateEdition(CreateEditionDto input);
    }
}
