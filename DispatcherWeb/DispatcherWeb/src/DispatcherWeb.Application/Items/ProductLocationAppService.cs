using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Items.Dto;
using DispatcherWeb.PricingTiers.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Items
{
    [AbpAuthorize]
    public class ProductLocationAppService : DispatcherWebAppServiceBase, IProductLocationAppService
    {
        private readonly IRepository<ProductLocation> _productLocationRepository;
        private readonly IRepository<PricingTier> _pricingTierRepository;
        private readonly IRepository<ProductLocationPrice> _productLocationPriceRepository;

        public ProductLocationAppService(
            IRepository<ProductLocation> productLocationRepository,
            IRepository<PricingTier> pricingTierRepository,
            IRepository<ProductLocationPrice> productLocationPriceRepository
        )
        {
            _productLocationRepository = productLocationRepository;
            _pricingTierRepository = pricingTierRepository;
            _productLocationPriceRepository = productLocationPriceRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<ProductLocationEditDto> GetProductLocationForEdit(NullableIdDto input)
        {
            ProductLocationEditDto productLocationEditDto;
            if (input.Id.HasValue)
            {
                productLocationEditDto = await (await _productLocationRepository.GetQueryAsync())
                    .Where(x => x.Id == input.Id)
                    .Select(x => new ProductLocationEditDto
                    {
                        Id = x.Id,
                        ItemId = x.ItemId,
                        LocationId = x.LocationId,
                        Cost = x.Cost,
                        MaterialUomId = x.UnitOfMeasureId,
                        MaterialUomName = x.UnitOfMeasure.Name,
                        LocationName = x.Location.DisplayName,
                        ProductLocationPrices = x.ProductLocationPrices
                            .Select(price => new ProductLocationPriceDto
                            {
                                Id = price.Id,
                                ProductLocationId = price.ProductLocationId,
                                PricePerUnit = price.PricePerUnit,
                                PricingTierName = price.PricingTier.Name,
                                PricingTierId = price.PricingTierId,
                            }).ToList(),
                    })
                    .FirstAsync();
            }
            else
            {
                productLocationEditDto = new ProductLocationEditDto();
            }

            return productLocationEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<List<PricingTierDto>> GetPricingTiers()
        {
            var productLocationPrices = await (await _pricingTierRepository.GetQueryAsync())
                .Select(p => new PricingTierDto
                {
                    Id = p.Id,
                    Name = p.Name,
                })
                .OrderBy(x => x.Id)
                .ToListAsync();

            return productLocationPrices;
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<List<ProductLocationPriceDto>> GetEmptyProductLocationPrices()
        {
            var tiers = await GetPricingTiers();

            return tiers.Select(p => new ProductLocationPriceDto
            {
                PricingTierId = p.Id,
                PricingTierName = p.Name,
            }).ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<PagedResultDto<ProductLocationDto>> GetRates(GetRatesInput input)
        {
            var query = (await _productLocationRepository.GetQueryAsync()).Where(x => x.ItemId == input.ItemId);

            var items = await query
                .Select(x => new ProductLocationDto
                {
                    Id = x.Id,
                    LocationName = x.Location.DisplayName,
                    UOM = x.UnitOfMeasure.Name,
                    Cost = x.Cost,
                    ProductLocationPrices = x.ProductLocationPrices
                        .Select(price => new ProductLocationPriceDto
                        {
                            Id = price.Id,
                            PricingTierId = price.PricingTierId,
                            PricePerUnit = price.PricePerUnit,
                            PricingTierName = price.PricingTier.Name,
                        }).ToList(),
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<ProductLocationDto>(items.Count, items);
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task EditProductLocation(ProductLocationEditDto input)
        {
            await CheckForProductLocationDuplicates(input.Id, input.LocationId, input.MaterialUomId, input.ItemId);
            var tenantId = await AbpSession.GetTenantIdAsync();

            var productLocationId = await _productLocationRepository.InsertOrUpdateAndGetIdAsync(new ProductLocation
            {
                Id = input.Id ?? 0,
                TenantId = tenantId,
                ItemId = input.ItemId,
                LocationId = input.LocationId,
                Cost = input.Cost,
                UnitOfMeasureId = input.MaterialUomId,
            });

            var productLocationPrices = input.ProductLocationPrices.Select(x => new ProductLocationPrice
            {
                Id = x.Id,
                ProductLocationId = productLocationId,
                PricingTierId = x.PricingTierId,
                PricePerUnit = x.PricePerUnit,
                TenantId = tenantId,
            }).ToList();

            await EditProductLocationPrices(productLocationPrices);
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task DeleteProductLocation(EntityDto input)
        {
            await _productLocationPriceRepository.DeleteAsync(x => x.ProductLocationId == input.Id);
            await _productLocationRepository.DeleteAsync(input.Id);
        }

        private async Task EditProductLocationPrices(List<ProductLocationPrice> input)
        {
            foreach (var item in input)
            {
                await _productLocationPriceRepository.InsertOrUpdateAndGetIdAsync(item);
            }
        }

        private async Task CheckForProductLocationDuplicates(int? id, int? locationId, int? unitOfMeasureId, int itemId)
        {
            var isProductLocationAvailable = await (await _productLocationRepository.GetQueryAsync())
                .AnyAsync(x =>
                    x.Id != id
                    && x.LocationId == locationId
                    && x.ItemId == itemId
                    && x.UnitOfMeasureId == unitOfMeasureId);

            if (isProductLocationAvailable)
            {
                throw new UserFriendlyException(L("ProductLocationAlreadyExists"));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_ReadItemPricing)]
        public async Task<MaterialPricingDto> GetMaterialRateForJob(GetRateForJobInput input)
        {
            if (input.ItemId == 0 || !input.UomId.HasValue)
            {
                return new MaterialPricingDto
                {
                    PricePerUnit = 0,
                };
            }
            var priceObjects = await (await _productLocationPriceRepository.GetQueryAsync())
                .Where(x =>
                    x.PricingTierId == input.PricingTierId
                    && x.ProductLocation.ItemId == input.ItemId
                    && x.ProductLocation.UnitOfMeasureId == input.UomId
                    && (x.ProductLocation.LocationId == input.LoadAtId
                        || x.ProductLocation.LocationId == input.DeliverToId
                        || x.ProductLocation.LocationId == null)
                )
                .Select(x => new
                {
                    x.ProductLocation.LocationId,
                    x.PricePerUnit,
                    x.ProductLocation.Cost,
                }).ToListAsync();

            var pricing = priceObjects.FirstOrDefault(x => x.LocationId == input.LoadAtId)
                          ?? priceObjects.FirstOrDefault(x => x.LocationId == input.DeliverToId)
                          ?? priceObjects.FirstOrDefault();

            return new MaterialPricingDto
            {
                PricePerUnit = pricing?.PricePerUnit,
                MaterialCostRate = pricing?.Cost,
            };
        }
    }
}
