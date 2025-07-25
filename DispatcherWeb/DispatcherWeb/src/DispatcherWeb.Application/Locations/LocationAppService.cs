using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Locations.Dto;
using DispatcherWeb.Locations.Exporting;
using DispatcherWeb.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Locations
{
    [AbpAuthorize]
    public class LocationAppService : DispatcherWebAppServiceBase, ILocationAppService
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILocationRepository _locationRepository;
        private readonly IRepository<LocationContact> _locationContactRepository;
        private readonly IRepository<LocationCategory> _locationCategoryRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly ILocationListCsvExporting _locationListCsvExporting;

        public LocationAppService(
            IUnitOfWorkManager unitOfWorkManager,
            ILocationRepository locationRepository,
            IRepository<LocationContact> locationContactRepository,
            IRepository<LocationCategory> locationCategoryRepository,
            IRepository<OrderLine> orderLineRepository,
            ILocationListCsvExporting locationListCsvExporting
            )
        {
            _unitOfWorkManager = unitOfWorkManager;
            _locationRepository = locationRepository;
            _locationContactRepository = locationContactRepository;
            _locationCategoryRepository = locationCategoryRepository;
            _orderLineRepository = orderLineRepository;
            _locationListCsvExporting = locationListCsvExporting;
        }

        /// <summary>
        /// This is a temporary method needed to populate Location.DisplayName of preexisting locations.
        /// </summary>
        [AbpAuthorize(AppPermissions.Pages_Administration_Host_Dashboard)]
        [UnitOfWork(IsDisabled = true)]
        public async Task<string> MigrateLocations(int takeCount = 1000)
        {
            var skipCount = 0;
            var totalCount = await WithTempUnitOfWorkAsync(async () =>
            {
                return await _locationRepository.CountAsync();
            });
            var updatedCount = 0;

            while (skipCount < totalCount)
            {
                await WithTempUnitOfWorkAsync(async () =>
                {
                    var locations = await (await _locationRepository.GetQueryAsync())
                        .OrderBy(x => x.Id)
                        .Skip(skipCount)
                        .Take(takeCount)
                        .ToListAsync();

                    foreach (var location in locations)
                    {
                        location.DisplayName = Utilities.ConcatenateAddress(location.Name, location.StreetAddress, location.City, location.State);
                        updatedCount++;
                    }
                    return true;
                });
                skipCount += takeCount;
            }

            return $"Updated {updatedCount} locations";
        }

        private async Task<T> WithTempUnitOfWorkAsync<T>(Func<Task<T>> action)
        {
            return await _unitOfWorkManager.WithUnitOfWorkAsync(new UnitOfWorkOptions
            {
                IsTransactional = false,
                Timeout = TimeSpan.FromMinutes(30),
            }, async () =>
            {
                return await action();
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task<PagedResultDto<LocationDto>> GetLocations(GetLocationsInput input)
        {
            var query = await GetFilteredLocationQueryAsync(input);

            var totalCount = await query.CountAsync();

            var items = await GetLocationDtoQuery(query)
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<LocationDto>(
                totalCount,
                items);
        }

        private async Task<IQueryable<Location>> GetFilteredLocationQueryAsync(IGetLocationFilteredList input)
        {
            return (await _locationRepository.GetQueryAsync())
                .WhereIf(!input.Name.IsNullOrEmpty(), x => x.Name.Contains(input.Name) || x.Abbreviation.Contains(input.Name))
                .WhereIf(input.CategoryId.HasValue, x => x.CategoryId == input.CategoryId)
                .WhereIf(!input.City.IsNullOrEmpty(), x => x.City.StartsWith(input.City))
                .WhereIf(!input.State.IsNullOrEmpty(), x => x.State.StartsWith(input.State))
                .WhereIf(input.Status == FilterActiveStatus.Active, x => x.IsActive)
                .WhereIf(input.Status == FilterActiveStatus.Inactive, x => !x.IsActive)
                .WhereIf(input.WithCoordinates, x => x.Latitude != null && x.Longitude != null);
        }

        private IQueryable<LocationDto> GetLocationDtoQuery(IQueryable<Location> query)
        {
            return query.Select(x => new LocationDto
            {
                Id = x.Id,
                Name = x.Name,
                CategoryName = x.Category.Name,
                StreetAddress = x.StreetAddress,
                City = x.City,
                State = x.State,
                ZipCode = x.ZipCode,
                CountryCode = x.CountryCode,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                IsActive = x.IsActive,
                PredefinedLocationKind = x.PredefinedLocationKind,
                DisallowDataMerge = x.PredefinedLocationKind != null,
                Abbreviation = x.Abbreviation,
                Notes = x.Notes,
                SendToFulcrum = x.SendToFulcrum,
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        [HttpPost]
        public async Task<FileDto> GetLocationsToCsv(GetLocationsInput input)
        {
            var query = await GetFilteredLocationQueryAsync(input);
            var items = await GetLocationDtoQuery(query)
                .OrderBy(input.Sorting)
                .ToListAsync();

            if (!items.Any())
            {
                throw new UserFriendlyException(L("ThereIsNoDataToExport"));
            }

            return await _locationListCsvExporting.ExportToFileAsync(items);
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Locations)]
        public async Task<PagedResultDto<SelectListDto>> GetLocationsSelectList(GetLocationsSelectListInput input)
        {
            var query = (await _locationRepository.GetQueryAsync())
                .WhereIf(!input.IncludeInactive, x => x.IsActive)
                .WhereIf(input.LoadAtQuoteId.HasValue, x => x.LoadAtQuoteLines.Any(s => s.QuoteId == input.LoadAtQuoteId))
                .WhereIf(input.DeliverToQuoteId.HasValue, x => x.DeliverToQuoteLines.Any(s => s.QuoteId == input.DeliverToQuoteId))
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.DisplayName,
                });

            return await query.GetSelectListResult(input);
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task<ListResultDto<SelectListDto>> GetLocationsByIdsSelectList(GetItemsByIdsInput input)
        {
            var items = await (await _locationRepository.GetQueryAsync())
                .Where(x => input.Ids.Contains(x.Id))
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.DisplayName,
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return new ListResultDto<SelectListDto>(items);
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task<LocationEditDto> GetLocationForEdit(GetLocationForEditInput input)
        {
            LocationEditDto locationEditDto;

            if (input.Id.HasValue)
            {
                locationEditDto = await (await _locationRepository.GetQueryAsync())
                    .Select(l => new LocationEditDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        CategoryId = l.CategoryId,
                        CategoryName = l.Category.Name,
                        IsTemporary = l.Category.Name == "Temporary",
                        StreetAddress = l.StreetAddress,
                        City = l.City,
                        State = l.State,
                        ZipCode = l.ZipCode,
                        CountryCode = l.CountryCode,
                        Latitude = l.Latitude,
                        Longitude = l.Longitude,
                        PlaceId = l.PlaceId,
                        IsActive = l.IsActive,
                        Abbreviation = l.Abbreviation,
                        Notes = l.Notes,
                        SendToFulcrum = l.SendToFulcrum,

                    })
                    .SingleAsync(s => s.Id == input.Id.Value);
            }
            else
            {
                locationEditDto = new LocationEditDto
                {
                    IsActive = true,
                    Name = input.Name,
                };

                if (input.Temporary)
                {
                    var temporaryCategory = await (await _locationCategoryRepository.GetQueryAsync())
                        .Where(x => x.Name == "Temporary")
                        .FirstOrDefaultAsync();

                    locationEditDto.CategoryId = temporaryCategory?.Id;
                    locationEditDto.CategoryName = temporaryCategory?.Name;
                    locationEditDto.IsTemporary = true;
                }

                locationEditDto.MergeWithDuplicateSilently = input.MergeWithDuplicateSilently;
            }

            return locationEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task<LocationEditDto> EditLocation(LocationEditDto model)
        {
            var location = model.Id.HasValue ? await _locationRepository.GetAsync(model.Id.Value) : new Location();

            if (location.PredefinedLocationKind != null)
            {
                return model;
            }

            location.Name = model.Name;
            location.CategoryId = model.CategoryId;
            location.StreetAddress = model.StreetAddress;
            location.City = model.City;
            location.State = model.State;
            location.ZipCode = model.ZipCode;
            location.CountryCode = model.CountryCode;
            location.Latitude = model.Latitude;
            location.Longitude = model.Longitude;
            location.PlaceId = model.PlaceId;
            location.IsActive = model.IsActive;
            location.Abbreviation = model.Abbreviation;
            location.Notes = model.Notes;
            location.SendToFulcrum = model.SendToFulcrum;

            if (model.Id.HasValue)
            {
                return model;
            }
            else
            {
                model.Id = await _locationRepository.InsertAndGetIdAsync(location);
                return model;
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Locations_InlineCreation)]
        public async Task<LocationEditDto> CreateOrGetExistingLocation(CreateOrGetExistingLocationInput model)
        {
            var category = await (await _locationCategoryRepository.GetQueryAsync())
                .Where(x => x.PredefinedLocationCategoryKind == model.PredefinedLocationCategoryKind)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                }).FirstOrDefaultAsync();

            if (category == null)
            {
                Logger.Error("PredefinedLocationCategory wasn't found: " + model.PredefinedLocationCategoryKind?.ToIntString());
                throw new UserFriendlyException("Predefined Location Category wasn't found");
            }

            var duplicate = await FindExistingLocationDuplicate(new LocationEditDto
            {
                CategoryId = category.Id,
                Name = model.Name,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                PlaceId = model.PlaceId,
                StreetAddress = model.StreetAddress,
                City = model.City,
                State = model.State,
            });

            if (duplicate != null)
            {
                return duplicate;
            }

            var result = await EditLocation(new LocationEditDto
            {
                Name = model.Name?.Truncate(100),
                CategoryId = category.Id,
                CategoryName = category.Name,
                StreetAddress = model.StreetAddress?.Truncate(EntityStringFieldLengths.GeneralAddress.MaxStreetAddressLength),
                City = model.City?.Truncate(EntityStringFieldLengths.GeneralAddress.MaxCityLength),
                State = model.State?.Truncate(EntityStringFieldLengths.GeneralAddress.MaxStateLength),
                ZipCode = model.ZipCode?.Truncate(EntityStringFieldLengths.GeneralAddress.MaxZipCodeLength),
                CountryCode = model.CountryCode?.Truncate(EntityStringFieldLengths.GeneralAddress.MaxCountryCodeLength),
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                PlaceId = model.PlaceId,
                IsActive = model.IsActive,
                Abbreviation = model.Abbreviation?.Truncate(10),
                Notes = model.Notes?.Truncate(1000),
                SendToFulcrum = model.SendToFulcrum,
            });

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_Locations_InlineCreation, AppPermissions.Pages_Locations)]
        public async Task<LocationEditDto> FindExistingLocationDuplicate(LocationEditDto model)
        {
            var addressIsSpecified = !string.IsNullOrEmpty(model.StreetAddress) || !string.IsNullOrEmpty(model.City) || !string.IsNullOrEmpty(model.State);

            var result = await (await _locationRepository.GetQueryAsync())
                    .Where(x => !string.IsNullOrEmpty(model.Name) && x.Name == model.Name
                        || addressIsSpecified && x.StreetAddress == model.StreetAddress && x.City == model.City && x.State == model.State)
                    .Select(l => new LocationEditDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        CategoryId = l.CategoryId,
                        CategoryName = l.Category.Name,
                        IsTemporary = l.Category.Name == "Temporary",
                        StreetAddress = l.StreetAddress,
                        City = l.City,
                        State = l.State,
                        ZipCode = l.ZipCode,
                        CountryCode = l.CountryCode,
                        Latitude = l.Latitude,
                        Longitude = l.Longitude,
                        PlaceId = l.PlaceId,
                        IsActive = l.IsActive,
                        Abbreviation = l.Abbreviation,
                        Notes = l.Notes,
                        SendToFulcrum = l.SendToFulcrum,

                    }).FirstOrDefaultAsync();

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task<bool> CanDeleteLocation(EntityDto input)
        {
            var isPredefined = await (await _locationRepository.GetQueryAsync()).AnyAsync(x => x.Id == input.Id && x.PredefinedLocationKind != null);
            if (isPredefined)
            {
                return false;
            }

            var hasOrderLines = await (await _orderLineRepository.GetQueryAsync()).Where(x => x.LoadAtId == input.Id || x.DeliverToId == input.Id).AnyAsync();
            if (hasOrderLines)
            {
                return false;
            }

            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task DeleteLocation(EntityDto input)
        {
            var canDelete = await CanDeleteLocation(input);
            if (!canDelete)
            {
                throw new UserFriendlyException("You can't delete selected row because it has data associated with it.");
            }
            await _locationRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task<PagedResultDto<LocationContactDto>> GetLocationContacts(GetLocationContactsInput input)
        {
            var query = (await _locationContactRepository.GetQueryAsync())
                .Where(x => x.LocationId == input.LocationId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new LocationContactDto
                {
                    Id = x.Id,
                    LocationId = x.LocationId,
                    Name = x.Name,
                    Phone = x.Phone,
                    Email = x.Email,
                    Title = x.Title,
                })
                .OrderBy(input.Sorting)
                .ToListAsync();

            return new PagedResultDto<LocationContactDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task<LocationContactEditDto> GetLocationContactForEdit(NullableIdDto input)
        {
            LocationContactEditDto locationContactEditDto;

            if (input.Id.HasValue)
            {
                var locationContact = await _locationContactRepository.GetAsync(input.Id.Value);
                locationContactEditDto = new LocationContactEditDto
                {
                    Id = locationContact.Id,
                    LocationId = locationContact.LocationId,
                    Name = locationContact.Name,
                    Phone = locationContact.Phone,
                    Email = locationContact.Email,
                    Title = locationContact.Title,
                };
            }
            else
            {
                locationContactEditDto = new LocationContactEditDto();
            }

            return locationContactEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task EditLocationContact(LocationContactEditDto model)
        {
            await _locationContactRepository.InsertOrUpdateAndGetIdAsync(new LocationContact
            {
                Id = model.Id ?? 0,
                LocationId = model.LocationId,
                Name = model.Name,
                Phone = model.Phone,
                Email = model.Email,
                Title = model.Title,
                TenantId = await Session.GetTenantIdAsync(),
            });
        }

        [AbpAuthorize(AppPermissions.Pages_Locations)]
        public async Task DeleteLocationContact(EntityDto input)
        {
            await _locationContactRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_Locations_Merge)]
        public async Task MergeLocations(DataMergeInput input)
        {
            await _locationRepository.MergeLocationsAsync(input.IdsToMerge, input.MainRecordId, await AbpSession.GetTenantIdOrNullAsync());
        }

        [AbpAuthorize(AppPermissions.Pages_Locations_Merge)]
        public async Task MergeLocationContacts(DataMergeInput input)
        {
            await _locationRepository.MergeLocationContactsAsync(input.IdsToMerge, input.MainRecordId, await AbpSession.GetTenantIdOrNullAsync());
        }

        public async Task<PagedResultDto<SelectListDto>> GetLocationCategorySelectList(GetSelectListInput input)
        {
            var query = (await _locationCategoryRepository.GetQueryAsync())
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                });

            return await query.GetSelectListResult(input);
        }
    }

}
