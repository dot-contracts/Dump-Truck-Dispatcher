using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Caching;
using DispatcherWeb.Distance;
using DispatcherWeb.Distance.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.HaulingCategories;
using DispatcherWeb.HaulZones;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Items.Dto;
using DispatcherWeb.Items.Exporting;
using DispatcherWeb.Locations;
using DispatcherWeb.Quotes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Items
{
    [AbpAuthorize]
    public class ItemAppService : DispatcherWebAppServiceBase, IItemAppService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IRepository<OfficeItemPrice> _itemPriceRepository;
        private readonly IRepository<ProductLocation> _productLocationRepository;
        private readonly IRepository<ProductLocationPrice> _productLocationPriceRepository;
        private readonly IRepository<QuoteLine> _quoteLineRepository;
        private readonly IRepository<HaulZone> _haulZoneRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly ListCacheCollection _listCaches;
        private readonly IDistanceCalculator _distanceCalculator;
        private readonly IItemListCsvExporter _itemListCsvExporter;
        private readonly IProductLocationAppService _productLocationAppService;
        private readonly IHaulingCategoryAppService _haulingCategoryAppService;

        public ItemAppService(
            IItemRepository itemRepository,
            IRepository<OfficeItemPrice> itemPriceRepository,
            IRepository<ProductLocation> productLocationRepository,
            IRepository<ProductLocationPrice> productLocationPriceRepository,
            IRepository<QuoteLine> quoteLineRepository,
            IRepository<HaulZone> haulZoneRepository,
            IRepository<Location> locationRepository,
            ListCacheCollection listCaches,
            IDistanceCalculator distanceCalculator,
            IItemListCsvExporter itemListCsvExporter,
            IProductLocationAppService productLocationAppService,
            IHaulingCategoryAppService haulingCategoryAppService
        )
        {
            _itemRepository = itemRepository;
            _itemPriceRepository = itemPriceRepository;
            _productLocationRepository = productLocationRepository;
            _productLocationPriceRepository = productLocationPriceRepository;
            _quoteLineRepository = quoteLineRepository;
            _haulZoneRepository = haulZoneRepository;
            _locationRepository = locationRepository;
            _listCaches = listCaches;
            _distanceCalculator = distanceCalculator;
            _itemListCsvExporter = itemListCsvExporter;
            _productLocationAppService = productLocationAppService;
            _haulingCategoryAppService = haulingCategoryAppService;
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<PagedResultDto<ItemDto>> GetItems(GetItemsInput input)
        {
            var query = await GetFilteredItemQuery(input);

            var totalCount = await query.CountAsync();

            var items = await GetItemDtoQuery(query)
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<ItemDto>(
                totalCount,
                items);
        }

        private async Task<IQueryable<Item>> GetFilteredItemQuery(IGetItemListFilter input)
        {
            return (await _itemRepository.GetQueryAsync())
                .WhereIf(!input.Name.IsNullOrEmpty(), x => x.Name.Contains(input.Name))
                .WhereIf(input.Status == FilterActiveStatus.Active, x => x.IsActive)
                .WhereIf(input.Status == FilterActiveStatus.Inactive, x => !x.IsActive)
                .WhereIf(input.Type.HasValue, x => x.Type == input.Type);
        }

        private IQueryable<ItemDto> GetItemDtoQuery(IQueryable<Item> query)
        {
            return query.Select(x => new ItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Type = x.Type,
                IsActive = x.IsActive,
                DisallowDataMerge = false,
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        [HttpPost]
        public async Task<FileDto> GetItemsToCsv(GetItemsInput input)
        {
            var query = await GetFilteredItemQuery(input);
            var items = await GetItemDtoQuery(query)
                .OrderBy(input.Sorting)
                .ToListAsync();

            if (!items.Any())
            {
                throw new UserFriendlyException(L("ThereIsNoDataToExport"));
            }

            return await _itemListCsvExporter.ExportToFileAsync(items);
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Items)]
        public async Task<PagedResultDto<SelectListDto>> GetItemsSelectList(GetItemsSelectListInput input)
        {
            var query = (await _itemRepository.GetQueryAsync())
                .WhereIf(!input.IncludeInactive, x => x.IsActive)
                .WhereIf(input.Types?.Any() == true, x => input.Types.Contains(x.Type))
                .WhereIf(input.QuoteId.HasValue,
                    x => x.QuoteFreightItems.Any(s => s.QuoteId == input.QuoteId)
                         || x.QuoteMaterialItems.Any(s => s.QuoteId == input.QuoteId))
                .Select(x => new SelectListDto<ItemSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new ItemSelectListInfoDto
                    {
                        IsTaxable = x.IsTaxable,
                        UseZoneBasedRates = x.UseZoneBasedRates,
                    },
                });

            return await query.GetSelectListResult(input);
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<ListResultDto<SelectListDto>> GetItemsByIdsSelectList(GetItemsByIdsInput input)
        {
            var items = await (await _itemRepository.GetQueryAsync())
                .Where(x => input.Ids.Contains(x.Id))
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return new ListResultDto<SelectListDto>(items);
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Misc_ReadItemPricing)]
        public async Task<ItemPricingDto> GetItemPricing(GetItemPricingInput input)
        {
            ItemPricingDto itemPricing;

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.PricingTiers)
                && await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems))
            {
                if (await FeatureChecker.IsEnabledAsync(AppFeatures.HaulZone)
                    && input.UseZoneBasedRates
                )
                {
                    itemPricing = await GetItemPricingFromHaulZone(input);
                }
                else
                {
                    itemPricing = await GetItemPricingFromHaulingCategory(input);
                }
            }
            else if (await FeatureChecker.IsEnabledAsync(AppFeatures.PricingTiers))
            {
                itemPricing = await GetItemPricingEnabledPricingTiers(input);
            }
            else
            {
                itemPricing = await GetItemPricingDisabledPricingTiers(input);
            }

            if (input.QuoteLineId.HasValue)
            {
                var quotePricing = await (await _quoteLineRepository.GetQueryAsync())
                    .Where(x => x.Id == input.QuoteLineId)
                    .Select(x => new
                    {
                        x.PricePerUnit,
                        x.FreightRate,
                        x.FreightRateToPayDrivers,
                        x.LeaseHaulerRate,
                        x.MaterialCostRate,
                    })
                    .FirstOrDefaultAsync();

                if (quotePricing != null)
                {
                    itemPricing.QuoteBasedPricing = new QuoteLinePricingDto
                    {
                        PricePerUnit = quotePricing.PricePerUnit,
                        MaterialCostRate = quotePricing.MaterialCostRate,
                        FreightRate = quotePricing.FreightRate,
                        FreightRateToPayDrivers = quotePricing.FreightRateToPayDrivers,
                        LeaseHaulerRate = quotePricing.LeaseHaulerRate,
                    };
                }
            }

            return itemPricing;
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<ItemEditDto> GetItemForEdit(NullableIdNameDto input)
        {
            ItemEditDto itemEditDto;

            if (input.Id.HasValue)
            {
                itemEditDto = await (await _itemRepository.GetQueryAsync())
                    .Where(x => x.Id == input.Id)
                    .Select(x => new ItemEditDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        IsActive = x.IsActive,
                        Type = x.Type,
                        IsTaxable = x.IsTaxable,
                        IncomeAccount = x.IncomeAccount,
                        ExpenseAccount = x.ExpenseAccount,
                        UseZoneBasedRates = x.UseZoneBasedRates,
                    })
                    .FirstAsync();
            }
            else
            {
                itemEditDto = new ItemEditDto
                {
                    IsActive = true,
                    Name = input.Name,
                };
            }

            if (await FeatureChecker.IsEnabledAsync(AppFeatures.PricingTiers))
            {
                itemEditDto.PricingTiers = await _productLocationAppService.GetPricingTiers();
            }

            return itemEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<ItemEditDto> EditItem(ItemEditDto model)
        {
            var item = model.Id.HasValue ? await _itemRepository.GetAsync(model.Id.Value) : new Item();

            item.Name = model.Name;
            item.Description = model.Description;
            item.IsActive = model.IsActive;
            item.Type = model.Type;
            item.IsTaxable = model.IsTaxable;
            item.IncomeAccount = model.IncomeAccount;
            item.ExpenseAccount = model.ExpenseAccount;
            item.UseZoneBasedRates = model.UseZoneBasedRates;

            if (!model.Id.HasValue)
            {
                model.Id = await _itemRepository.InsertAndGetIdAsync(item);
            }

            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<bool> CanDeleteItem(EntityDto input)
        {
            var temp = await (await _itemRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    HasData = x.QuoteFreightItems.Any()
                              || x.QuoteMaterialItems.Any()
                              || x.OrderLineFreightItems.Any()
                              || x.OrderLineMaterialItems.Any()
                              || x.ReceiptLineFreightItems.Any()
                              || x.ReceiptLineMaterialItems.Any()
                              || x.FreightTickets.Any()
                              || x.MaterialTickets.Any(),
                }).FirstAsync();

            return !temp.HasData;
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task DeleteItem(EntityDto input)
        {
            var canDelete = await CanDeleteItem(input);
            if (!canDelete)
            {
                throw new UserFriendlyException("You can't delete selected row because it has data associated with it.");
            }

            await _productLocationPriceRepository.DeleteAsync(x => x.ProductLocation.ItemId == input.Id);
            await _productLocationRepository.DeleteAsync(x => x.ItemId == input.Id);
            await _itemPriceRepository.DeleteAsync(x => x.ItemId == input.Id);
            await _itemRepository.DeleteAsync(input.Id);
        }

        //*************************************************//

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<PagedResultDto<ItemPriceDto>> GetItemPrices(GetItemPricesInput input)
        {
            var query = (await _itemPriceRepository.GetQueryAsync())
                .Where(x => x.ItemId == input.ItemId && x.OfficeId == OfficeId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new ItemPriceDto
                {
                    Id = x.Id,
                    ItemId = x.ItemId,
                    OfficeId = x.OfficeId,
                    PricePerUnit = x.PricePerUnit,
                    FreightRate = x.FreightRate,
                    MaterialUomName = x.MaterialUom.Name,
                    FreightUomName = x.FreightUom.Name,
                    Designation = x.Designation,
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<ItemPriceDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task<ItemPriceEditDto> GetItemPriceForEdit(NullableIdDto input)
        {
            ItemPriceEditDto itemPriceEditDto;

            if (input.Id.HasValue)
            {
                itemPriceEditDto = await (await _itemPriceRepository.GetQueryAsync())
                    .Where(x => x.Id == input.Id)
                    .Select(x => new ItemPriceEditDto
                    {
                        Id = x.Id,
                        ItemId = x.ItemId,
                        OfficeId = x.OfficeId,
                        PricePerUnit = x.PricePerUnit,
                        FreightRate = x.FreightRate,
                        MaterialUomId = x.MaterialUomId,
                        MaterialUomName = x.MaterialUom.Name,
                        FreightUomId = x.FreightUomId,
                        FreightUomName = x.FreightUom.Name,
                        Designation = x.Designation,
                    })
                    .FirstAsync();
            }
            else
            {
                itemPriceEditDto = new ItemPriceEditDto
                {
                    OfficeId = OfficeId,
                };
            }

            return itemPriceEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task EditItemPrice(ItemPriceEditDto model)
        {
            await _itemPriceRepository.InsertOrUpdateAndGetIdAsync(new OfficeItemPrice
            {
                Id = model.Id ?? 0,
                ItemId = model.ItemId,
                OfficeId = OfficeId,
                PricePerUnit = model.PricePerUnit,
                FreightRate = model.FreightRate,
                MaterialUomId = model.MaterialUomId,
                FreightUomId = model.FreightUomId,
                Designation = model.Designation,
                TenantId = await Session.GetTenantIdAsync(),
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Items)]
        public async Task DeleteItemPrice(EntityDto input)
        {
            await _itemPriceRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_Items_Merge)]
        public async Task MergeItems(DataMergeInput input)
        {
            await _itemRepository.MergeItemsAsync(input.IdsToMerge, input.MainRecordId, await AbpSession.GetTenantIdOrNullAsync());
        }

        private async Task<ItemPricingDto> GetItemPricingFromHaulZone(GetItemPricingInput input)
        {
            var locationPricingInput = GetLocationsWithRatesInput.CreateFromPricingInput(input);
            locationPricingInput.UomBaseId = await SettingManager.GetHaulRateCalculationUomBaseId(input.CustomerIsCod);
            if (!locationPricingInput.AreRequiredFieldsFilled())
            {
                return await GetFallbackItemPricingFromHaulZone(input);
            }

            var locationPricing = await GetLocationsWithRates(locationPricingInput);
            if (!locationPricing.Any())
            {
                return await GetFallbackItemPricingFromHaulZone(input);
            }

            return new ItemPricingDto
            {
                HasPricing = true,
                FreightRate = locationPricing[0].Item.FreightRate,
                FreightRateToPayDrivers = locationPricing[0].Item.FreightRateToPayDrivers,
                PricePerUnit = locationPricing[0].Item.MaterialPricePerUnit,
                MaterialCostRate = locationPricing[0].Item.MaterialCostRate,
                LeaseHaulerRate = locationPricing[0].Item.LeaseHaulerRate,
            };
        }

        private async Task<ItemPricingDto> GetFallbackItemPricingFromHaulZone(GetItemPricingInput input)
        {
            var emptyResult = new ItemPricingDto
            {
                HasPricing = false,
                FreightRate = 0,
                PricePerUnit = 0,
                FreightRateToPayDrivers = 0,
                LeaseHaulerRate = 0,
            };

            if (input.MaterialItemId != null && input.PricingTierId != null)
            {
                var materialRate = await _productLocationAppService.GetMaterialRateForJob(new GetRateForJobInput
                {
                    ItemId = input.MaterialItemId.Value,
                    UomId = input.MaterialUomId,
                    LoadAtId = input.LoadAtId,
                    DeliverToId = input.DeliverToId,
                    PricingTierId = input.PricingTierId.Value,
                });

                if (materialRate != null)
                {
                    emptyResult.HasPricing = true;
                    emptyResult.PricePerUnit = materialRate.PricePerUnit;
                    emptyResult.MaterialCostRate = materialRate.MaterialCostRate;
                    emptyResult.FreightRate = null;
                    emptyResult.FreightRateToPayDrivers = null;
                    emptyResult.LeaseHaulerRate = null;
                }
            }

            return emptyResult;
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_ReadItemPricing)]
        private async Task<ItemPricingDto> GetItemPricingFromHaulingCategory(GetItemPricingInput input)
        {
            ItemPricingDto itemPricing;
            MaterialPricingDto materialRate = null;
            HaulZonePricingDto freightRate = null;

            if (input.QuoteLineId == null && input.PricingTierId != null)
            {
                if (input.FreightItemId != null)
                {
                    freightRate = await _haulingCategoryAppService.GetFreightRateForJob(new GetRateForJobInput
                    {
                        ItemId = input.FreightItemId.Value,
                        UomId = input.FreightUomId,
                        TruckCategoryIds = input.TruckCategoryIds,
                        PricingTierId = input.PricingTierId.Value,
                    });
                }

                if (input.MaterialItemId != null)
                {
                    materialRate = await _productLocationAppService.GetMaterialRateForJob(new GetRateForJobInput
                    {
                        ItemId = (int)input.MaterialItemId,
                        UomId = input.MaterialUomId,
                        LoadAtId = input.LoadAtId,
                        DeliverToId = input.DeliverToId,
                        PricingTierId = input.PricingTierId.Value,
                    });
                }

                itemPricing = new ItemPricingDto
                {
                    HasPricing = true,
                    PricePerUnit = materialRate?.PricePerUnit,
                    MaterialCostRate = materialRate?.MaterialCostRate,
                    FreightRate = freightRate?.PricePerUnit,
                    FreightRateToPayDrivers = freightRate?.PricePerUnit,
                    LeaseHaulerRate = freightRate?.LeaseHaulerRate,
                    IsMultiplePriceObject = freightRate?.IsMultiplePriceObject,
                };
            }
            else
            {
                itemPricing = new ItemPricingDto();
            }

            return itemPricing;
        }

        private async Task<ItemPricingDto> GetItemPricingEnabledPricingTiers(GetItemPricingInput input)
        {
            ItemPricingDto itemPricing;

            if (input.QuoteLineId == null && input.PricingTierId != null)
            {
                var materialPricing = await _productLocationAppService.GetMaterialRateForJob(new GetRateForJobInput
                {
                    UomId = input.MaterialUomId,
                    ItemId = input.FreightItemId.Value,
                    LoadAtId = input.LoadAtId,
                    DeliverToId = input.DeliverToId,
                    PricingTierId = input.PricingTierId.Value,
                });

                itemPricing = new ItemPricingDto
                {
                    HasPricing = true,
                    PricePerUnit = materialPricing.PricePerUnit,
                    MaterialCostRate = materialPricing.MaterialCostRate,
                    FreightRate = null,
                };
            }
            else
            {
                itemPricing = new ItemPricingDto();
            }

            return itemPricing;
        }

        private async Task<ItemPricingDto> GetItemPricingDisabledPricingTiers(GetItemPricingInput input)
        {
            var itemPrices = await (await _itemPriceRepository.GetQueryAsync())
                .Where(x => x.ItemId == input.FreightItemId
                            && (x.MaterialUomId == input.MaterialUomId || x.FreightUomId == input.FreightUomId)
                            && x.OfficeId == OfficeId)
                .Select(x => new
                {
                    x.PricePerUnit,
                    x.FreightRate,
                    x.MaterialUomId,
                    x.FreightUomId,
                })
                .ToListAsync();

            var priceMatchingBoth = itemPrices.FirstOrDefault(x => x.MaterialUomId == input.MaterialUomId && x.FreightUomId == input.FreightUomId);
            var priceMatchingMaterial = itemPrices.FirstOrDefault(x => x.MaterialUomId == input.MaterialUomId);
            var priceMatchingFreight = itemPrices.FirstOrDefault(x => x.FreightUomId == input.FreightUomId);

            var itemPricing = itemPrices.Any()
                ? new ItemPricingDto
                {
                    HasPricing = true,
                    PricePerUnit = priceMatchingBoth?.PricePerUnit ?? priceMatchingMaterial?.PricePerUnit,
                    FreightRate = priceMatchingBoth?.FreightRate ?? priceMatchingFreight?.FreightRate,
                    //FreightRateToPayDrivers will match FreightRate (set below),
                } : new ItemPricingDto();

            itemPricing.FreightRateToPayDrivers = itemPricing.FreightRate;

            return itemPricing;
        }

        [RequiresFeature(AppFeatures.HaulZone, AppFeatures.SeparateMaterialAndFreightItems, RequiresAll = true)]
        [AbpAuthorize(AppPermissions.Pages_Misc_ReadItemPricing, AppPermissions.Pages_Misc_SelectLists_Locations, RequireAllPermissions = true)]
        public async Task<List<SelectListDto<LocationRateSelectListInfoDto>>> GetLocationsWithRates(GetLocationsWithRatesInput input)
        {
            input.Validate();

            var currencyCulture = await SettingManager.GetCurrencyCultureAsync();

            //Temporarily, limit to only "Tons" for Freight UOM
            var uoms = await _listCaches.UnitOfMeasure.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));
            var freightUom = uoms.Find(input.FreightUomId);
            if (freightUom?.UomBaseId != UnitOfMeasureBaseEnum.Tons)
            {
                //we won't throw an exception since this is a temporary limitation
                //this is also a reason why this is not included in the front end validation
                return new List<SelectListDto<LocationRateSelectListInfoDto>>();
            }

            var materialUomId = input.Designation == DesignationEnum.FreightOnly ? input.FreightUomId : input.MaterialUomId;

            var locationsWithMaterialRaw = await (await _productLocationRepository.GetQueryAsync())
                .WhereIf(input.LoadAtId.HasValue, x => x.LocationId == input.LoadAtId)
                .WhereIf(input.MaterialItemId.HasValue, x => x.ItemId == input.MaterialItemId
                    && x.Location.IsActive
                    && (materialUomId == null || x.UnitOfMeasureId == null || x.UnitOfMeasureId == materialUomId)
                )
                .Select(x => new
                {
                    x.LocationId,
                    x.UnitOfMeasureId,
                    Result = new LocationDistanceRateDto
                    {
                        Id = x.Location.Id,
                        Name = x.Location.Name,
                        PlaceId = x.Location.PlaceId,
                        Latitude = x.Location.Latitude,
                        Longitude = x.Location.Longitude,
                        StreetAddress = x.Location.StreetAddress,
                        City = x.Location.City,
                        ZipCode = x.Location.ZipCode,
                        State = x.Location.State,
                        CountryCode = x.Location.CountryCode,
                        MaterialPricing = x.ProductLocationPrices
                            .Where(p => p.PricingTierId == input.PricingTierId)
                            .Select(p => new LocationDistanceRateDto.MaterialPricingDto
                            {
                                PricePerUnit = p.PricePerUnit,
                                Cost = p.ProductLocation.Cost,
                            })
                            .FirstOrDefault(),
                    },
                }).ToListAsync();

            var locationsWithMaterial = locationsWithMaterialRaw
                .GroupBy(g => new { g.LocationId, g.UnitOfMeasureId })
                .Select(x => x
                    .OrderByDescending(l => l.UnitOfMeasureId != null && l.UnitOfMeasureId == materialUomId)
                    .First()
                )
                .Select(x => x.Result)
                .ToList();

            if (!locationsWithMaterial.Any())
            {
                if (input.LoadAtId.HasValue)
                {
                    locationsWithMaterial = await (await _locationRepository.GetQueryAsync())
                        .Where(x => x.Id == input.LoadAtId)
                        .Select(x => new LocationDistanceRateDto
                        {
                            Id = x.Id,
                            Name = x.Name,
                            PlaceId = x.PlaceId,
                            Latitude = x.Latitude,
                            Longitude = x.Longitude,
                            StreetAddress = x.StreetAddress,
                            City = x.City,
                            ZipCode = x.ZipCode,
                            State = x.State,
                            CountryCode = x.CountryCode,
                            MaterialPricing = null,
                        }).ToListAsync();
                }

                if (!locationsWithMaterial.Any())
                {
                    return new List<SelectListDto<LocationRateSelectListInfoDto>>();
                }
            }

            var deliverTo = await (await _locationRepository.GetQueryAsync())
                .Where(x => x.Id == input.DeliverToId)
                .Select(x => new LocationDto
                {
                    Id = x.Id,
                    PlaceId = x.PlaceId,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    StreetAddress = x.StreetAddress,
                    City = x.City,
                    ZipCode = x.ZipCode,
                    State = x.State,
                    CountryCode = x.CountryCode,
                }).FirstAsync();

            await _distanceCalculator.PopulateDistancesAsync(new PopulateDistancesInput
            {
                UomBaseId = input.UomBaseId,
                Sources = locationsWithMaterial,
                Destination = deliverTo,
            });

            var haulZones = await (await _haulZoneRepository.GetQueryAsync())
                .Where(x => x.IsActive
                    && x.UnitOfMeasure.UnitOfMeasureBaseId == (int)input.UomBaseId
                )
                .Select(x => new
                {
                    x.Quantity,
                    x.BillRatePerTon,
                    x.PayRatePerTon,
                })
                .OrderBy(x => x.Quantity)
                .ToListAsync();

            var result = locationsWithMaterial.Select(x =>
            {
                var haulZone = x.Distance.HasValue ? haulZones.FirstOrDefault(h => h.Quantity >= (double)x.Distance) : null;
                return new SelectListDto<LocationRateSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new LocationRateSelectListInfoDto
                    {
                        Distance = x.Distance,
                        MaterialPricePerUnit = x.MaterialPricing?.PricePerUnit,
                        MaterialCostRate = x.MaterialPricing?.Cost,
                        FreightRate = haulZone?.BillRatePerTon,
                        FreightRateToPayDrivers = haulZone?.PayRatePerTon,
                        LeaseHaulerRate = haulZone?.PayRatePerTon,
                        CombinedRate = null,
                    },
                };
            }).ToList();

            foreach (var resultItem in result)
            {
                var item = resultItem.Item;
                if (item.Distance.HasValue)
                {
                    item.CombinedRate = item.FreightRate;
                    var asteriskOnRate = false;

                    if (input.Designation == DesignationEnum.FreightAndMaterial)
                    {
                        if (input.FreightUomId != input.MaterialUomId
                            || item.MaterialPricePerUnit == null
                            || item.MaterialPricePerUnit == 0
                        )
                        {
                            asteriskOnRate = true;
                        }
                        else
                        {
                            item.CombinedRate += item.MaterialPricePerUnit;
                        }
                    }

                    var distanceString = DistanceCalculator.FormatDistanceWithUnits(item.Distance.Value, input.UomBaseId);
                    var rateString = item.CombinedRate.HasValue
                        ? (item.CombinedRate.Value.ToString("C2", currencyCulture) + "/" + freightUom.Name + (asteriskOnRate ? "*" : ""))
                        : "";
                    resultItem.Name += $" @{distanceString} {rateString}";
                }
            }

            result = result
                .Where(x => string.IsNullOrEmpty(input.Term)
                    || x.Name?.ToLower().Contains(input.Term.ToLower()) == true)
                .OrderByDescending(x => x.Item.CombinedRate.HasValue)
                .ThenBy(x => x.Item.CombinedRate)
                .ToList();

            return result;
        }
    }
}
