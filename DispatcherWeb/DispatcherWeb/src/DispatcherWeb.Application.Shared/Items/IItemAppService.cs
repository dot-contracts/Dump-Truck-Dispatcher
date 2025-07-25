using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Items.Dto;

namespace DispatcherWeb.Items
{
    public interface IItemAppService : IApplicationService
    {
        Task<PagedResultDto<ItemDto>> GetItems(GetItemsInput input);

        Task<PagedResultDto<SelectListDto>> GetItemsSelectList(GetItemsSelectListInput input);

        Task<ListResultDto<SelectListDto>> GetItemsByIdsSelectList(GetItemsByIdsInput input);

        Task<ItemPricingDto> GetItemPricing(GetItemPricingInput input);

        Task<List<SelectListDto<LocationRateSelectListInfoDto>>> GetLocationsWithRates(GetLocationsWithRatesInput input);

        Task<ItemEditDto> GetItemForEdit(NullableIdNameDto input);

        Task<ItemEditDto> EditItem(ItemEditDto model);

        Task<bool> CanDeleteItem(EntityDto input);

        Task DeleteItem(EntityDto input);

        Task<PagedResultDto<ItemPriceDto>> GetItemPrices(GetItemPricesInput input);

        Task<ItemPriceEditDto> GetItemPriceForEdit(NullableIdDto input);

        Task EditItemPrice(ItemPriceEditDto model);

        Task DeleteItemPrice(EntityDto input);

        Task MergeItems(DataMergeInput input);
    }
}
