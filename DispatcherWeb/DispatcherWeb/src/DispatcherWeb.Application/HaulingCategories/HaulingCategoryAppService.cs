using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Items;
using DispatcherWeb.Items.Dto;
using DispatcherWeb.PricingTiers.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.HaulingCategories
{
    [AbpAuthorize]
    public class HaulingCategoryAppService : DispatcherWebAppServiceBase, IHaulingCategoryAppService
    {
        private readonly IRepository<HaulingCategory> _haulingCategoryRepository;
        private readonly IRepository<HaulingCategoryPrice> _haulingCategoryPriceRepository;
        private readonly IRepository<PricingTier> _pricingTierRepository;
        public HaulingCategoryAppService(
            IRepository<HaulingCategory> haulingCategoryRepository,
            IRepository<HaulingCategoryPrice> haulingCategoryPriceRepository,
            IRepository<PricingTier> pricingTierRepository
            )
        {
            _haulingCategoryRepository = haulingCategoryRepository;
            _haulingCategoryPriceRepository = haulingCategoryPriceRepository;
            _pricingTierRepository = pricingTierRepository;
        }


        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<HaulingCategoryEditDto> GetHaulingCategoryForEdit(NullableIdDto input)
        {
            HaulingCategoryEditDto haulingCategoryEditDto;
            if (input.Id.HasValue)
            {
                haulingCategoryEditDto = await (await _haulingCategoryRepository.GetQueryAsync())
                    .Where(x => x.Id == input.Id)
                    .Select(x => new HaulingCategoryEditDto
                    {
                        Id = x.Id,
                        ItemId = x.ItemId,
                        TruckCategoryId = x.TruckCategoryId,
                        TruckCategoryName = x.TruckCategory.Name,
                        UnitOfMeasureId = x.UnitOfMeasureId,
                        UnitOfMeasureName = x.UnitOfMeasure.Name,
                        MinimumBillableUnits = x.MinimumBillableUnits,
                        LeaseHaulerRate = x.LeaseHaulerRate,
                        HaulingCategoryPrices = x.HaulingCategoryPrices
                            .Select(price => new HaulingCategoryPriceDto
                            {
                                Id = price.Id,
                                HaulingCategoryId = price.HaulingCategoryId,
                                PricePerUnit = price.PricePerUnit,
                                PricingTierName = price.PricingTier.Name,
                                PricingTierId = price.PricingTierId,
                            }).ToList(),
                    })
                    .FirstAsync();
            }
            else
            {
                haulingCategoryEditDto = new HaulingCategoryEditDto();
            }

            return haulingCategoryEditDto;
        }


        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<PagedResultDto<HaulingCategoryDto>> GetRates(GetRatesInput input)
        {
            var query = (await _haulingCategoryRepository.GetQueryAsync()).Where(x => x.ItemId == input.ItemId);

            var items = await query
                .Select(x => new HaulingCategoryDto
                {
                    Id = x.Id,
                    TruckCategoryName = x.TruckCategory.Name,
                    UOM = x.UnitOfMeasure.Name,
                    MinimumBillableUnits = x.MinimumBillableUnits,
                    LeaseHaulerRate = x.LeaseHaulerRate,
                    HaulingCategoryPrices = x.HaulingCategoryPrices
                        .Select(price => new HaulingCategoryPriceDto
                        {
                            Id = price.Id,
                            PricingTierId = price.PricingTierId,
                            PricePerUnit = price.PricePerUnit,
                            PricingTierName = price.PricingTier.Name,
                        }).ToList(),
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<HaulingCategoryDto>(items.Count, items);
        }


        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task EditHaulingCategory(HaulingCategoryEditDto input)
        {
            await CheckForHaulingCategoryDuplicates(input.Id, input.TruckCategoryId, input.UnitOfMeasureId, input.ItemId);

            var tenantId = await Session.GetTenantIdAsync();

            var haulingCategoryId = await _haulingCategoryRepository.InsertOrUpdateAndGetIdAsync(new HaulingCategory
            {
                Id = input.Id ?? 0,
                TenantId = tenantId,
                ItemId = input.ItemId,
                TruckCategoryId = input.TruckCategoryId,
                UnitOfMeasureId = input.UnitOfMeasureId,
                MinimumBillableUnits = input.MinimumBillableUnits,
                LeaseHaulerRate = input.LeaseHaulerRate,
            });

            var haulingCategoryPrices = input.HaulingCategoryPrices.Select(x => new HaulingCategoryPrice
            {
                Id = x.Id,
                HaulingCategoryId = haulingCategoryId,
                PricingTierId = x.PricingTierId,
                PricePerUnit = x.PricePerUnit,
                TenantId = tenantId,
            }).ToList();

            await EditHaulingCategoryPrices(haulingCategoryPrices);
        }

        private async Task EditHaulingCategoryPrices(List<HaulingCategoryPrice> input)
        {
            foreach (var item in input)
            {
                await _haulingCategoryPriceRepository.InsertOrUpdateAndGetIdAsync(item);
            }
        }


        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<List<HaulingCategoryPriceDto>> GetEmptyHaulingCategoryPrices()
        {
            var tiers = await GetPricingTiers();

            return tiers.Select(p => new HaulingCategoryPriceDto
            {
                PricingTierId = p.Id,
                PricingTierName = p.Name,
            }).ToList();
        }


        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<List<PricingTierDto>> GetPricingTiers()
        {
            var haulingCategoryPrices = await (await _pricingTierRepository.GetQueryAsync())
                .Select(p => new PricingTierDto
                {
                    Id = p.Id,
                    Name = p.Name,
                })
                .OrderBy(x => x.Id)
                .ToListAsync();

            return haulingCategoryPrices;
        }


        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task DeleteHaulingCategory(EntityDto input)
        {
            await _haulingCategoryPriceRepository.DeleteAsync(x => x.HaulingCategoryId == input.Id);
            await _haulingCategoryRepository.DeleteAsync(input.Id);
        }

        private async Task CheckForHaulingCategoryDuplicates(int? id, int? truckCategoryId, int? unitOfMeasureId, int itemId)
        {
            var isHaulingCategoryAvailable = await (await _haulingCategoryRepository.GetQueryAsync())
                .AnyAsync(x =>
                    x.Id != id
                    && x.TruckCategoryId == truckCategoryId
                    && x.ItemId == itemId
                    && x.UnitOfMeasureId == unitOfMeasureId);

            if (isHaulingCategoryAvailable)
            {
                throw new UserFriendlyException(L("HaulingCategoryAlreadyExists"));
            }
        }


        [AbpAuthorize(AppPermissions.Pages_Misc_ReadItemPricing)]
        public async Task<HaulZonePricingDto> GetFreightRateForJob(GetRateForJobInput input)
        {
            if (input.ItemId == 0 || !input.UomId.HasValue)
            {
                return new HaulZonePricingDto
                {
                    PricePerUnit = 0,
                };
            }

            var priceObjects = await (await _haulingCategoryPriceRepository.GetQueryAsync())
                .Where(x =>
                    x.PricingTierId == input.PricingTierId
                    && x.HaulingCategory.ItemId == input.ItemId
                    && x.HaulingCategory.UnitOfMeasureId == input.UomId
                )
                .WhereIf(input.TruckCategoryIds?.Any() == true, x => input.TruckCategoryIds.Contains(x.HaulingCategory.TruckCategoryId))
                .Select(x => new HaulZonePricingDto
                {
                    LeaseHaulerRate = x.HaulingCategory.LeaseHaulerRate,
                    PricePerUnit = x.PricePerUnit,
                }).ToListAsync();

            var pricing = priceObjects.Count == 1
                ? priceObjects.FirstOrDefault()
                : new HaulZonePricingDto
                {
                    PricePerUnit = 0,
                    IsMultiplePriceObject = priceObjects.Count > 1,
                };

            return pricing;
        }
    }
}
