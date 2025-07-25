using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Drivers;
using DispatcherWeb.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.SyncRequests;
using DispatcherWeb.TimeClassifications.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.TimeClassifications
{
    [AbpAuthorize]
    public class TimeClassificationAppService : DispatcherWebAppServiceBase, ITimeClassificationAppService
    {
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IRepository<EmployeeTimeClassification> _employeeTimeClassificationRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly ISyncRequestSender _syncRequestSender;

        public TimeClassificationAppService(
            IRepository<TimeClassification> timeClassificationRepository,
            IRepository<EmployeeTimeClassification> employeeTimeClassificationRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<Driver> driverRepository,
            ISyncRequestSender syncRequestSender
            )
        {
            _timeClassificationRepository = timeClassificationRepository;
            _employeeTimeClassificationRepository = employeeTimeClassificationRepository;
            _employeeTimeRepository = employeeTimeRepository;
            _driverRepository = driverRepository;
            _syncRequestSender = syncRequestSender;
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_TimeClassifications)]
        public async Task<ListResultDto<SelectListDto>> GetTimeClassificationsSelectList(GetTimeClassificationsSelectListInput input)
        {
            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay) && await FeatureChecker.IsEnabledAsync(AppFeatures.DriverProductionPayFeature);

            var driverId = input.EmployeeId == null ? (int?)null : await _driverRepository.GetDriverIdByUserIdOrDefault(input.EmployeeId.Value);
            if (driverId == 0)
            {
                driverId = null;
            }

            var items = await (await _timeClassificationRepository.GetQueryAsync())
                .WhereIf(!allowProductionPay || input.ExcludeProductionPay, x => !x.IsProductionBased)
                .WhereIf(driverId.HasValue,
                    x => x.EmployeeTimeClassifications.Any(e => e.DriverId == driverId
                        && (!input.AllowForManualTime.HasValue || e.AllowForManualTime == input.AllowForManualTime)))
                .Select(x => new SelectListDto<TimeClassificationSelectListInfo>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new TimeClassificationSelectListInfo
                    {
                        IsProductionPay = x.IsProductionBased,
                        DefaultRate = x.DefaultRate,
                    },
                })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return new ListResultDto<SelectListDto>(items);
        }

        [AbpAuthorize(AppPermissions.Pages_TimeEntry_EditTimeClassifications)]
        public async Task<PagedResultDto<TimeClassificationDto>> GetTimeClassifications(GetTimeClassificationsInput input)
        {
            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay) && await FeatureChecker.IsEnabledAsync(AppFeatures.DriverProductionPayFeature);

            var query = (await _timeClassificationRepository.GetQueryAsync())
                .WhereIf(!allowProductionPay, x => !x.IsProductionBased);

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new TimeClassificationDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsProductionBased = x.IsProductionBased,
                    DefaultRate = x.DefaultRate,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<TimeClassificationDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_TimeEntry_EditTimeClassifications)]
        public async Task<TimeClassificationEditDto> GetTimeClassificationForEdit(NullableIdDto input)
        {
            TimeClassificationEditDto timeClassificationEditDto;

            if (input.Id.HasValue)
            {
                timeClassificationEditDto = await (await _timeClassificationRepository.GetQueryAsync())
                    .Where(x => x.Id == input.Id.Value)
                    .Select(x => new TimeClassificationEditDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        IsProductionBased = x.IsProductionBased,
                        DefaultRate = x.DefaultRate,
                        HasRecordsAssociated = x.EmployeeTimeClassifications.Any()
                            || x.QuoteLines.Any()
                            || x.OrderLines.Any(),
                    }).FirstAsync();
            }
            else
            {
                timeClassificationEditDto = new TimeClassificationEditDto();
            }

            return timeClassificationEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_TimeEntry_EditTimeClassifications)]
        public async Task EditTimeClassification(TimeClassificationEditDto model)
        {
            var entity = model.Id.HasValue ? await _timeClassificationRepository.GetAsync(model.Id.Value) : new TimeClassification();

            var hasRecordsAssociated = model.Id.HasValue
                && await (await _timeClassificationRepository.GetQueryAsync())
                    .Where(x => x.Id == model.Id)
                    .Select(x =>
                        x.EmployeeTimeClassifications.Any()
                        || x.QuoteLines.Any()
                        || x.OrderLines.Any()
                    )
                    .FirstAsync();

            var nameHasChanged = model.Id.HasValue && entity.Name != model.Name;

            entity.Name = model.Name;
            entity.DefaultRate = model.DefaultRate;
            entity.TenantId = await Session.GetTenantIdAsync();

            if (!hasRecordsAssociated)
            {
                entity.IsProductionBased = model.IsProductionBased;
            }

            if (await (await _timeClassificationRepository.GetQueryAsync())
                .WhereIf(model.Id != 0, x => x.Id != model.Id)
                .AnyAsync(x => x.Name == model.Name)
            )
            {
                throw new UserFriendlyException($"TimeClassification with name '{model.Name}' already exists!");
            }

            await _timeClassificationRepository.InsertOrUpdateAndGetIdAsync(entity);

            if (nameHasChanged)
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                var affectedDriverIds = await (await _employeeTimeClassificationRepository.GetQueryAsync())
                    .Where(x => x.TimeClassificationId == model.Id)
                    .Select(x => x.DriverId)
                    .Distinct()
                    .ToListAsync();

                await _syncRequestSender.SendSyncRequest(
                    new SyncRequest()
                        .AddChange(EntityEnum.TimeClassification,
                            entity.ToChangedEntity().AddDriverIds(affectedDriverIds)));
            }
        }

        [AbpAuthorize(AppPermissions.Pages_TimeEntry_EditTimeClassifications)]
        public async Task DeleteTimeClassification(EntityDto input)
        {
            var hasEmployeeTimeRecords = await (await _employeeTimeRepository.GetQueryAsync())
                .Where(x => x.TimeClassificationId == input.Id)
                .AnyAsync();

            if (hasEmployeeTimeRecords)
            {
                throw new UserFriendlyException(L("UnableToDeleteSelectedRowWithAssociatedData"));
            }

            var defaultTimeClassificationId = await SettingManager.GetSettingValueAsync<int>(AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId);
            if (defaultTimeClassificationId == input.Id)
            {
                throw new UserFriendlyException("You can't delete selected row because it is a default time tracking classification.");
            }

            var timeClassification = await (await _timeClassificationRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    HasDataAssociated = x.EmployeeTimeClassifications.Any()
                        || x.QuoteLines.Any()
                        || x.OrderLines.Any(),
                }).FirstAsync();

            if (timeClassification.HasDataAssociated)
            {
                throw new UserFriendlyException(L("UnableToDeleteSelectedRowWithAssociatedData"));
            }

            await _timeClassificationRepository.DeleteAsync(input.Id);
        }
    }
}
