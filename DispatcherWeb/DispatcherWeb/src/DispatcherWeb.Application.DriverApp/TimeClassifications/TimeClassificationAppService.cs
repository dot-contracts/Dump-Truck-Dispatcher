using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.DriverApp.TimeClassifications.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.Features;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DriverApp.TimeClassifications
{
    [AbpAuthorize(AppPermissions.Pages_DriverApplication_ReactNativeDriverApp)]
    public class TimeClassificationAppService : DispatcherWebDriverAppAppServiceBase, ITimeClassificationAppService
    {
        private readonly IRepository<EmployeeTimeClassification> _employeeTimeClassificationRepository;

        public TimeClassificationAppService(
            IRepository<EmployeeTimeClassification> employeeTimeClassificationRepository
            )
        {
            _employeeTimeClassificationRepository = employeeTimeClassificationRepository;
        }

        public async Task<IListResult<TimeClassificationDto>> Get(GetInput input)
        {
            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay) && await FeatureChecker.IsEnabledAsync(AppFeatures.DriverProductionPayFeature);

            var result = await (await _employeeTimeClassificationRepository.GetQueryAsync())
                .Where(x => x.Driver.UserId == Session.UserId)
                .WhereIf(!allowProductionPay, x => !x.TimeClassification.IsProductionBased)
                .Select(x => new TimeClassificationDto
                {
                    Id = x.TimeClassificationId,
                    Name = x.TimeClassification.Name,
                    IsProductionBased = x.TimeClassification.IsProductionBased,
                    IsDefault = x.IsDefault,
                })
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.Name)
                .ToListAsync(CancellationTokenProvider.Token);

            return new ListResultDto<TimeClassificationDto>(result);
        }
    }
}
