using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Encryption;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Offices.Dto;
using DispatcherWeb.Storage;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Offices
{
    [AbpAuthorize]
    public class OfficeAppService : DispatcherWebAppServiceBase, IOfficeAppService
    {
        private readonly IRepository<Office> _officeRepository;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IEncryptionService _encryptionService;
        private readonly IOfficeOrganizationUnitSynchronizer _officeOrganizationUnitSynchronizer;

        public OfficeAppService(
            IRepository<Office> officeRepository,
            ISingleOfficeAppService singleOfficeService,
            IBinaryObjectManager binaryObjectManager,
            IEncryptionService encryptionService,
            IOfficeOrganizationUnitSynchronizer officeOrganizationUnitSynchronizer
            )
        {
            _officeRepository = officeRepository;
            _singleOfficeService = singleOfficeService;
            _binaryObjectManager = binaryObjectManager;
            _encryptionService = encryptionService;
            _officeOrganizationUnitSynchronizer = officeOrganizationUnitSynchronizer;
        }

        public async Task<ListResultDto<OfficeDto>> GetAllOffices()
        {
            var items = await (await _officeRepository.GetQueryAsync())
                .Select(x => new OfficeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    TruckColor = x.TruckColor,
                })
                .ToListAsync();

            return new ListResultDto<OfficeDto>(items);
        }

        [AbpAuthorize(AppPermissions.Pages_Offices)]
        public async Task<PagedResultDto<OfficeDto>> GetOffices(GetOfficesInput input)
        {
            var query = await _officeRepository.GetQueryAsync();

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new OfficeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    TruckColor = x.TruckColor,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<OfficeDto>(
                totalCount,
                items);
        }

        public async Task<PagedResultDto<SelectListDto>> GetOfficesSelectList(GetSelectListInput input)
        {
            var organizationUnitIds = await GetOrganizationUnitIds();

            var query = (await _officeRepository.GetQueryAsync())
                .Where(x => organizationUnitIds.Contains(x.OrganizationUnitId))
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                });

            return await query.GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Offices)]
        public async Task<OfficeEditDto> GetOfficeForEdit(NullableIdDto input)
        {
            OfficeEditDto officeEditDto;

            if (input.Id.HasValue)
            {
                var office = await _officeRepository.GetAsync(input.Id.Value);
                officeEditDto = new OfficeEditDto
                {
                    Id = office.Id,
                    Name = office.Name,
                    TruckColor = office.TruckColor,
                    CopyChargeTo = office.CopyDeliverToLoadAtChargeTo,
                    HeartlandPublicKey = office.HeartlandPublicKey,
                    HeartlandSecretKey = office.HeartlandSecretKey,
                    FuelIds = office.FuelIds,
                    DefaultStartTime = office.DefaultStartTime,
                    LogoId = office.LogoId,
                    ReportsLogoId = office.ReportsLogoId,
                };

                if (string.IsNullOrEmpty(officeEditDto.HeartlandPublicKey))
                {
                    officeEditDto.HeartlandPublicKey = await SettingManager.GetSettingValueAsync(AppSettings.Heartland.PublicKey);
                }

                if (string.IsNullOrEmpty(officeEditDto.HeartlandSecretKey))
                {
                    officeEditDto.HeartlandSecretKey = await SettingManager.GetSettingValueAsync(AppSettings.Heartland.SecretKey);
                }

                officeEditDto.HeartlandSecretKey = officeEditDto.HeartlandSecretKey.IsNullOrEmpty()
                    ? string.Empty
                    : DispatcherWebConsts.PasswordHasntBeenChanged;
            }
            else
            {
                officeEditDto = new OfficeEditDto
                {
                    DefaultStartTime = await SettingManager.GetSettingValueAsync<DateTime>(AppSettings.DispatchingAndMessaging.DefaultStartTime),
                };
            }

            officeEditDto.DefaultStartTime = officeEditDto.DefaultStartTime?.ConvertTimeZoneTo(await GetTimezone());

            return officeEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Offices)]
        public async Task<OfficeEditDto> EditOffice(OfficeEditDto model)
        {
            if (!model.Id.HasValue)
            {
                await CheckAllowMultiOffice();
                await _singleOfficeService.Reset();
            }

            var entity = model.Id.HasValue ? await _officeRepository.GetAsync(model.Id.Value) : new Office();

            var shouldUpdateOrganizationUnit = entity.Id == 0
                || entity.Name != model.Name;

            entity.Name = model.Name;
            entity.TruckColor = model.TruckColor;
            entity.CopyDeliverToLoadAtChargeTo = model.CopyChargeTo;
            entity.TenantId = await Session.GetTenantIdAsync();
            entity.FuelIds = model.FuelIds;
            entity.DefaultStartTime = model.DefaultStartTime?.ConvertTimeZoneFrom(await GetTimezone());

            if (model.HeartlandSecretKey != DispatcherWebConsts.PasswordHasntBeenChanged)
            {
                var tenantHeartlandSecretKey = await SettingManager.GetSettingValueAsync(AppSettings.Heartland.SecretKey);

                if (model.HeartlandSecretKey == tenantHeartlandSecretKey)
                {
                    model.HeartlandSecretKey = null;
                }

                entity.HeartlandSecretKey = _encryptionService.EncryptIfNotEmpty(model.HeartlandSecretKey);
            }

            var tenantHeartlandPublicKey = await SettingManager.GetSettingValueAsync(AppSettings.Heartland.PublicKey);

            if (model.HeartlandPublicKey == tenantHeartlandPublicKey)
            {
                model.HeartlandPublicKey = null;
            }

            entity.HeartlandPublicKey = model.HeartlandPublicKey;

            if (await (await _officeRepository.GetQueryAsync())
                .WhereIf(model.Id != 0, x => x.Id != model.Id)
                .AnyAsync(x => x.Name == model.Name)
            )
            {
                throw new UserFriendlyException($"Office with name '{model.Name}' already exists!");
            }

            model.Id = await _officeRepository.InsertOrUpdateAndGetIdAsync(entity);

            if (shouldUpdateOrganizationUnit)
            {
                await _officeOrganizationUnitSynchronizer.UpdateOrganizationUnit(entity);
            }

            return model;
        }

        private async Task CheckAllowMultiOffice()
        {
            if (await FeatureChecker.IsEnabledAsync(AppFeatures.AllowMultiOfficeFeature))
            {
                return;
            }
            int currentOfficesNumber = await _officeRepository.CountAsync();
            if (currentOfficesNumber > 0)
            {
                throw new AbpAuthorizationException("You cannot have more than one office.");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Offices)]
        public async Task<int> GetOfficesNumber()
        {
            return await _officeRepository.CountAsync();
        }

        [AbpAuthorize(AppPermissions.Pages_Offices)]
        public async Task<bool> CanDeleteOffice(EntityDto input)
        {
            return await _officeOrganizationUnitSynchronizer.CanDeleteOffice(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Offices)]
        public async Task DeleteOffice(EntityDto input)
        {
            await _officeOrganizationUnitSynchronizer.DeleteOffice(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Offices)]
        public async Task ClearLogo(int id)
        {
            var office = await _officeRepository.GetAsync(id);

            if (office.LogoId == null)
            {
                return;
            }

            var logoObject = await _binaryObjectManager.GetOrNullAsync(office.LogoId.Value);
            if (logoObject != null)
            {
                await _binaryObjectManager.DeleteAsync(office.LogoId.Value);
            }

            office.LogoId = null;
            office.LogoFileType = null;
        }

        [AbpAuthorize(AppPermissions.Pages_Offices)]
        public async Task ClearReportsLogo(int id)
        {
            var office = await _officeRepository.GetAsync(id);

            if (office.ReportsLogoId == null)
            {
                return;
            }

            var logoObject = await _binaryObjectManager.GetOrNullAsync(office.ReportsLogoId.Value);
            if (logoObject != null)
            {
                await _binaryObjectManager.DeleteAsync(office.ReportsLogoId.Value);
            }

            office.ReportsLogoId = null;
            office.ReportsLogoFileType = null;
        }
    }
}
