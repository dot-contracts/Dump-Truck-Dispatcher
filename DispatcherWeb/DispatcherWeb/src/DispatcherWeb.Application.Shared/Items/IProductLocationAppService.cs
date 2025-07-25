using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Items.Dto;
using DispatcherWeb.PricingTiers.Dto;

namespace DispatcherWeb.Items
{
    public interface IProductLocationAppService : IApplicationService
    {
        Task<ProductLocationEditDto> GetProductLocationForEdit(NullableIdDto input);

        Task<List<PricingTierDto>> GetPricingTiers();

        Task<List<ProductLocationPriceDto>> GetEmptyProductLocationPrices();

        Task<PagedResultDto<ProductLocationDto>> GetRates(GetRatesInput input);

        Task EditProductLocation(ProductLocationEditDto input);

        Task DeleteProductLocation(EntityDto input);

        Task<MaterialPricingDto> GetMaterialRateForJob(GetRateForJobInput input);
    }
}
