using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.PreventiveMaintenanceSchedule.Dto;
using DispatcherWeb.Trucks;
using DispatcherWeb.VehicleMaintenance;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.PreventiveMaintenanceSchedule
{
    [AbpAuthorize]
    public class PreventiveMaintenanceAppService : DispatcherWebAppServiceBase, IPreventiveMaintenanceAppService
    {
        private readonly IRepository<PreventiveMaintenance> _preventiveMaintenanceRepository;
        private readonly IRepository<VehicleService> _vehicleServiceRepository;
        private readonly IRepository<Truck> _truckRepository;

        public PreventiveMaintenanceAppService(
            IRepository<PreventiveMaintenance> preventiveMaintenanceRepository,
            IRepository<VehicleService> vehicleServiceRepository,
            IRepository<Truck> truckRepository
        )
        {
            _preventiveMaintenanceRepository = preventiveMaintenanceRepository;
            _vehicleServiceRepository = vehicleServiceRepository;
            _truckRepository = truckRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_PreventiveMaintenanceSchedule_View)]
        public async Task<PagedResultDto<PreventiveMaintenanceDto>> GetPreventiveMaintenancePagedList(GetPreventiveMaintenancePagedListInput input)
        {
            var today = await GetToday();
            var query = (await _preventiveMaintenanceRepository.GetQueryAsync())
                .Where(x => x.Truck.IsActive)
                .WhereIf(input.OfficeId.HasValue, x => x.Truck.OfficeId == input.OfficeId)
                .WhereIf(!string.IsNullOrEmpty(input.TruckCode), x => x.Truck.TruckCode.Contains(input.TruckCode))
                .WhereIf(input.DueDateBegin.HasValue, x => x.DueDate >= input.DueDateBegin.Value)
                .WhereIf(input.DueDateEnd.HasValue, x => x.DueDate <= input.DueDateEnd.Value)
                .WhereIf(input.Status == GetPreventiveMaintenancePagedListInput.PreventiveMaintenanceStatus.Overdue,
                        x => x.DueDate < today
                        || x.DueMileage < x.Truck.CurrentMileage)
                .WhereIf(input.VehicleServiceId.HasValue, x => x.VehicleServiceId == input.VehicleServiceId.Value)
                .WhereIf(input.DueForService, pm =>
                        pm.WarningDate < today
                        || pm.WarningMileage < pm.Truck.CurrentMileage)
                ;


            var totalCount = await query.CountAsync();
            var items = await query
                .Select(x => new PreventiveMaintenanceDto
                {
                    Id = x.Id,
                    TruckCode = x.Truck.TruckCode,
                    CurrentMileage = x.Truck.CurrentMileage,
                    VehicleServiceName = x.VehicleService.Name,
                    DueDate = x.DueDate,
                    DueMileage = x.DueMileage,
                    WarningDate = x.WarningDate,
                    WarningMileage = x.WarningMileage,
                    DaysUntilDue = EF.Functions.DateDiffDay(today, x.DueDate),
                    MilesUntilDue = x.DueMileage - x.Truck.CurrentMileage,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();



            return new PagedResultDto<PreventiveMaintenanceDto>(totalCount, items);

        }

        [AbpAuthorize(AppPermissions.Pages_PreventiveMaintenanceSchedule_View)]
        public async Task<PreventiveMaintenanceEditDto> GetForEdit(NullableIdDto input)
        {
            PreventiveMaintenanceEditDto dto;
            if (input.Id.HasValue)
            {
                dto = await (await _preventiveMaintenanceRepository.GetQueryAsync())
                    .Select(x => new PreventiveMaintenanceEditDto
                    {
                        Id = x.Id,
                        TruckId = x.TruckId,
                        TruckCode = x.Truck.TruckCode,
                        VehicleServiceId = x.VehicleServiceId,
                        VehicleServiceName = x.VehicleService.Name,
                        LastDate = x.LastDate,
                        LastMileage = x.LastMileage,
                        LastHour = x.LastHour,
                        DueDate = x.DueDate,
                        DueMileage = x.DueMileage,
                        DueHour = x.DueHour,
                        WarningDate = x.WarningDate,
                        WarningMileage = x.WarningMileage,
                        WarningHour = x.WarningHour,
                    })
                    .FirstOrDefaultAsync(x => x.Id == input.Id.Value);
            }
            else
            {
                dto = new PreventiveMaintenanceEditDto();
            }
            return dto;
        }

        [AbpAuthorize(AppPermissions.Pages_PreventiveMaintenanceSchedule_Edit)]
        public async Task<PreventiveMaintenanceEditDto> Save(PreventiveMaintenanceEditDto model)
        {
            CheckDateIsNotNull(model.LastDate, nameof(model.LastDate));
            CheckEitherMileageOrDateIsNotNull(model.DueDate, nameof(model.DueDate), model.DueMileage, nameof(model.DueMileage));
            CheckEitherMileageOrDateIsNotNull(model.WarningDate, nameof(model.WarningDate), model.WarningMileage, nameof(model.WarningMileage));
            if (
                model.DueDate.HasValue && !model.WarningDate.HasValue
                || !model.DueDate.HasValue && model.WarningDate.HasValue
                || model.DueHour.HasValue && !model.WarningHour.HasValue
                || !model.DueHour.HasValue && model.WarningHour.HasValue
                || model.DueMileage.HasValue && !model.WarningMileage.HasValue
                || !model.DueMileage.HasValue && model.WarningMileage.HasValue
            )
            {
                throw new UserFriendlyException("Combination of the Due Date or Mileage or Hour and Warning Date or Mileage or Hour is wrong!");
            }

            PreventiveMaintenance entity = model.Id != 0 ?
                await _preventiveMaintenanceRepository.GetAsync(model.Id)
                : new PreventiveMaintenance();

            entity.TruckId = model.TruckId;
            entity.VehicleServiceId = model.VehicleServiceId;
            Debug.Assert(model.LastDate != null, "model.LastDate != null");
            entity.LastDate = model.LastDate.Value;
            entity.LastHour = model.LastHour;
            entity.LastMileage = model.LastMileage;
            entity.DueDate = model.DueDate;
            entity.DueHour = model.DueHour;
            entity.DueMileage = model.DueMileage;
            entity.WarningDate = model.WarningDate;
            entity.WarningHour = model.WarningHour;
            entity.WarningMileage = model.WarningMileage;

            model.Id = await _preventiveMaintenanceRepository.InsertOrUpdateAndGetIdAsync(entity);
            return model;
        }

        private void CheckDateIsNotNull(DateTime? date, string name)
        {
            if (!date.HasValue)
            {
                throw new UserFriendlyException($"{name} cannot be null!");
            }
        }
        private void CheckEitherMileageOrDateIsNotNull(DateTime? date, string dateName, decimal? mileage, string mileageName)
        {
            if (!date.HasValue && !mileage.HasValue)
            {
                throw new UserFriendlyException($"Either {dateName} or {mileageName} are required.");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_PreventiveMaintenanceSchedule_Edit)]
        public async Task DeletePreventiveMaintenance(EntityDto input)
        {
            await _preventiveMaintenanceRepository.DeleteAsync(input.Id);
        }

        [AbpAuthorize(AppPermissions.Pages_PreventiveMaintenanceSchedule_Edit)]
        public async Task<PreventiveMaintenanceDefaultValuesDto> GetDefaultValues(int vehicleServiceId, int truckId)
        {
            if (vehicleServiceId == 0)
            {
                throw new ArgumentException(nameof(vehicleServiceId));
            }

            if (truckId == 0)
            {
                throw new ArgumentException(nameof(truckId));
            }

            var vehicleServiceValues = await (await _vehicleServiceRepository.GetQueryAsync())
                .Select(x => new
                {
                    Id = x.Id,
                    RecommendedTimeInterval = x.RecommendedTimeInterval,
                    RecommendedMileageInterval = x.RecommendedMileageInterval,
                    RecommendedHourInterval = x.RecommendedHourInterval,
                    WarningDays = x.WarningDays,
                    WarningMiles = x.WarningMiles,
                    WarningHours = x.WarningHours,
                })
                .FirstOrDefaultAsync(x => x.Id == vehicleServiceId);

            var truck = await (await _truckRepository.GetQueryAsync())
                .Where(t => t.Id == truckId)
                .Select(t => new
                {
                    t.InServiceDate,
                    t.CurrentMileage,
                    t.CurrentHours,
                }).SingleAsync();

            var lastPM = await (await _preventiveMaintenanceRepository.GetQueryAsync())
                .Where(pm => pm.VehicleServiceId == vehicleServiceId && pm.TruckId == truckId)
                .OrderByDescending(pm => pm.LastDate)
                .Select(pm => new
                {
                    pm.LastDate,
                    pm.LastMileage,
                    pm.LastHour,
                }).FirstOrDefaultAsync();

            var lastDate = lastPM?.LastDate ?? truck.InServiceDate;
            decimal? lastMileage = lastPM?.LastMileage ?? truck.CurrentMileage;
            decimal? lastHour = lastPM?.LastHour ?? truck.CurrentHours;

            var defaultValuesDto = new PreventiveMaintenanceDefaultValuesDto
            {
                LastDate = lastDate,
                LastMileage = lastMileage ?? 0,
                LastHour = lastHour ?? 0,
            };

            if (vehicleServiceValues != null && vehicleServiceValues.RecommendedTimeInterval != 0)
            {
                defaultValuesDto.DueDate = defaultValuesDto.LastDate?.AddDays(vehicleServiceValues.RecommendedTimeInterval ?? 0);
            }
            if (vehicleServiceValues != null && vehicleServiceValues.RecommendedMileageInterval != 0)
            {
                defaultValuesDto.DueMileage = defaultValuesDto.LastMileage + vehicleServiceValues.RecommendedMileageInterval;
            }
            if (vehicleServiceValues != null && vehicleServiceValues.RecommendedHourInterval != 0)
            {
                defaultValuesDto.DueHour = defaultValuesDto.LastHour + vehicleServiceValues.RecommendedHourInterval;
            }
            defaultValuesDto.WarningDate = defaultValuesDto.DueDate?.AddDays(-vehicleServiceValues?.WarningDays ?? 0);
            defaultValuesDto.WarningMileage = defaultValuesDto.DueMileage - vehicleServiceValues?.WarningMiles;
            defaultValuesDto.WarningHour = defaultValuesDto.DueHour - vehicleServiceValues?.WarningHours;
            return defaultValuesDto;
        }

    }
}
