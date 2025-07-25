using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Locations.Dto;

namespace DispatcherWeb.Locations
{
    public interface ILocationAppService : IApplicationService
    {
        Task<PagedResultDto<LocationDto>> GetLocations(GetLocationsInput input);
        Task<PagedResultDto<SelectListDto>> GetLocationsSelectList(GetLocationsSelectListInput input);
        Task<ListResultDto<SelectListDto>> GetLocationsByIdsSelectList(GetItemsByIdsInput input);
        Task<LocationEditDto> GetLocationForEdit(GetLocationForEditInput input);
        Task<LocationEditDto> EditLocation(LocationEditDto model);
        Task<bool> CanDeleteLocation(EntityDto input);
        Task DeleteLocation(EntityDto input);

        Task<PagedResultDto<LocationContactDto>> GetLocationContacts(GetLocationContactsInput input);
        Task<LocationContactEditDto> GetLocationContactForEdit(NullableIdDto input);
        Task EditLocationContact(LocationContactEditDto model);
        Task DeleteLocationContact(EntityDto input);

        Task MergeLocations(DataMergeInput input);
        Task MergeLocationContacts(DataMergeInput input);
    }
}
