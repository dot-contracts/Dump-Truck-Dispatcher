using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using DispatcherWeb.Orders.RevenueBreakdownByTruckReport.Dto;
using DispatcherWeb.Orders.RevenueBreakdownReport.Dto;
using DispatcherWeb.Runtime.Session;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Orders.RevenueBreakdownReport
{
    public class RevenueBreakdownTimeCalculator : IRevenueBreakdownTimeCalculator, ITransientDependency
    {
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly ISettingManager _settingManager;
        private readonly IExtendedAbpSession _session;

        public RevenueBreakdownTimeCalculator(
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            ISettingManager settingManager,
            IExtendedAbpSession session
            )
        {
            _employeeTimeRepository = employeeTimeRepository;
            _settingManager = settingManager;
            _session = session;
        }

        public async Task FillDriversTimeForTrucks(List<RevenueBreakdownByTruckItem> items, RevenueBreakdownByTruckReportInput input)
        {
            var truckIds = items.Select(x => x.TruckId).Distinct().ToList();

            await FillDriversTime(new FillDriversTimeInput
            {
                DeliveryDateBegin = input.DeliveryDateBegin,
                DeliveryDateEnd = input.DeliveryDateEnd,
                TruckIds = truckIds,
            },
            (e) =>
            {
                items
                    .Where(x => x.TruckId == e.TruckId && x.TicketDate == e.DeliveryDate)
                    .ToList()
                    .ForEach(x => x.DriverTime += e.HoursToAdd);
            });
        }

        public async Task FillDriversTimeForOrderLines(List<RevenueBreakdownItem> items, RevenueBreakdownReportInput input)
        {
            await FillDriversTime(new FillDriversTimeInput
            {
                DeliveryDateBegin = input.DeliveryDateBegin,
                DeliveryDateEnd = input.DeliveryDateEnd,
            },
            (e) =>
            {
                items
                    .Where(x => x.DeliveryDate == e.DeliveryDate)
                    .ToList()
                    .ForEach(x => x.DriverTime += e.HoursToAdd);
            });
        }

        private async Task<string> GetTimezone()
        {
            return await _settingManager.GetSettingValueForUserAsync(TimingSettingNames.TimeZone, await _session.GetTenantIdAsync(), _session.UserId ?? 0);
        }

        public async Task<bool> HaveTimeWithNoEndDate(FillDriversTimeInput input)
        {
            var timeZone = await GetTimezone();
            var startDate = input.DeliveryDateBegin.ConvertTimeZoneFrom(timeZone);
            var endDate = input.DeliveryDateEnd.ConvertTimeZoneFrom(timeZone).AddDays(1);
            var nowDate = Clock.Now;
            var currentDateStart = nowDate.ConvertTimeZoneTo(timeZone).Date.ConvertTimeZoneFrom(timeZone);
            var currentDateEnd = currentDateStart.AddDays(1);

            return await (await _employeeTimeRepository.GetQueryAsync())
                .WhereIf(input.TruckIds != null, x => input.TruckIds.Contains(x.EquipmentId))
                .WhereIf(input.UserIds != null, x => input.UserIds.Contains(x.UserId))
                .WhereIf(input.LocalEmployeesOnly, x => x.User.Drivers.Any(d => d.OfficeId != null && !d.IsInactive))
                .Where(x => !x.TimeClassification.IsProductionBased)
                .AnyAsync(x => x.StartDateTime >= startDate && x.StartDateTime < endDate && x.EndDateTime == null);
        }

        public async Task FillDriversTime(FillDriversTimeInput input, FillDriverTimeCallback fillTimeCallback)
        {
            var timeZone = await GetTimezone();
            var startDate = input.DeliveryDateBegin.ConvertTimeZoneFrom(timeZone);
            var endDate = input.DeliveryDateEnd.ConvertTimeZoneFrom(timeZone).AddDays(1);
            var nowDate = Clock.Now;
            var currentDateStart = nowDate.ConvertTimeZoneTo(timeZone).Date.ConvertTimeZoneFrom(timeZone);
            var currentDateEnd = currentDateStart.AddDays(1);

            var driversTime = await (await _employeeTimeRepository.GetQueryAsync())
                .WhereIf(input.TruckIds != null, x => input.TruckIds.Contains(x.EquipmentId))
                .WhereIf(input.UserIds != null, x => input.UserIds.Contains(x.UserId))
                .WhereIf(input.ExcludeTimeWithPayStatements, x => x.PayStatementTime == null)
                .WhereIf(input.LocalEmployeesOnly, x => x.User.Drivers.Any(d => d.OfficeId != null && !d.IsInactive))
                .Where(x => x.EndDateTime >= startDate && x.EndDateTime < endDate)
                .Select(x => new
                {
                    x.Id,
                    x.EquipmentId,
                    x.TimeClassificationId,
                    x.TimeClassification.IsProductionBased,
                    x.UserId,
                    x.DriverId,
                    x.OrderLineId,
                    x.StartDateTime,
                    x.EndDateTime,
                    x.ManualHourAmount,
                    x.PayRate,
                })
                .ToListAsync();

            foreach (var driverTime in driversTime)
            {
                if (!driverTime.EndDateTime.HasValue)
                {
                    continue;
                }

                var start = driverTime.StartDateTime;
                var end = driverTime.EndDateTime.Value;
                var deliveryDate = end.ConvertTimeZoneTo(timeZone).Date;
                var hoursToAdd = driverTime.ManualHourAmount ?? Convert.ToDecimal((end - start).TotalHours);

                fillTimeCallback(new FillDriversTimeCallbackArgs
                {
                    EmployeeTimeId = driverTime.Id,
                    TruckId = driverTime.EquipmentId,
                    UserId = driverTime.UserId,
                    DriverId = driverTime.DriverId,
                    OrderLineId = driverTime.OrderLineId,
                    DeliveryDate = deliveryDate,
                    HoursToAdd = hoursToAdd,
                    TimeClassificationId = driverTime.TimeClassificationId,
                    IsProductionBased = driverTime.IsProductionBased,
                    PayRate = driverTime.PayRate,
                });
            }
        }

        public async Task FillDriversTimeAsync(FillDriversTimeInput input, FillDriverTimeAsyncCallback fillTimeCallback)
        {
            var timeZone = await GetTimezone();
            var startDate = input.DeliveryDateBegin.ConvertTimeZoneFrom(timeZone);
            var endDate = input.DeliveryDateEnd.ConvertTimeZoneFrom(timeZone).AddDays(1);
            var nowDate = Clock.Now;
            var currentDateStart = nowDate.ConvertTimeZoneTo(timeZone).Date.ConvertTimeZoneFrom(timeZone);
            var currentDateEnd = currentDateStart.AddDays(1);

            var driversTime = await (await _employeeTimeRepository.GetQueryAsync())
                .WhereIf(input.TruckIds != null, x => input.TruckIds.Contains(x.EquipmentId))
                .WhereIf(input.UserIds != null, x => input.UserIds.Contains(x.UserId))
                .WhereIf(input.ExcludeTimeWithPayStatements, x => x.PayStatementTime == null)
                .WhereIf(input.LocalEmployeesOnly, x => x.User.Drivers.Any(d => d.OfficeId != null && !d.IsInactive))
                .Where(x => x.EndDateTime >= startDate && x.EndDateTime < endDate)
                .Select(x => new
                {
                    x.Id,
                    x.EquipmentId,
                    x.TimeClassificationId,
                    x.TimeClassification.IsProductionBased,
                    x.UserId,
                    x.DriverId,
                    x.OrderLineId,
                    x.StartDateTime,
                    x.EndDateTime,
                    x.ManualHourAmount,
                    x.PayRate,
                })
                .ToListAsync();

            foreach (var driverTime in driversTime)
            {
                if (!driverTime.EndDateTime.HasValue)
                {
                    continue;
                }

                var start = driverTime.StartDateTime;
                var end = driverTime.EndDateTime.Value;
                var deliveryDate = end.ConvertTimeZoneTo(timeZone).Date;
                var hoursToAdd = driverTime.ManualHourAmount ?? Convert.ToDecimal((end - start).TotalHours);

                await fillTimeCallback(new FillDriversTimeCallbackArgs
                {
                    EmployeeTimeId = driverTime.Id,
                    TruckId = driverTime.EquipmentId,
                    UserId = driverTime.UserId,
                    DriverId = driverTime.DriverId,
                    OrderLineId = driverTime.OrderLineId,
                    DeliveryDate = deliveryDate,
                    HoursToAdd = hoursToAdd,
                    TimeClassificationId = driverTime.TimeClassificationId,
                    IsProductionBased = driverTime.IsProductionBased,
                    PayRate = driverTime.PayRate,
                });
            }
        }

        //old time calculation logic
        public async Task FillDriversExactTime(FillDriversTimeInput input, FillDriverTimeCallback fillTimeCallback)
        {
            var timeZone = await GetTimezone();
            var startDate = input.DeliveryDateBegin.ConvertTimeZoneFrom(timeZone);
            var endDate = input.DeliveryDateEnd.ConvertTimeZoneFrom(timeZone).AddDays(1);
            var nowDate = Clock.Now;
            var currentDateStart = nowDate.ConvertTimeZoneTo(timeZone).Date.ConvertTimeZoneFrom(timeZone);
            var currentDateEnd = currentDateStart.AddDays(1);

            var driversTime = await (await _employeeTimeRepository.GetQueryAsync())
                .WhereIf(input.TruckIds != null, x => input.TruckIds.Contains(x.EquipmentId))
                .WhereIf(input.UserIds != null, x => input.UserIds.Contains(x.UserId))
                .Where(x =>
                    //  && !(x.EndDateTime != null && x.EndDateTime <= startDate)
                    //  && !(x.StartDateTime >= endDate))
                    (x.EndDateTime == null && !x.IsImported || x.EndDateTime > startDate)
                    && x.StartDateTime < endDate)
                .Select(x => new
                {
                    x.Id,
                    x.EquipmentId,
                    x.UserId,
                    x.DriverId,
                    x.StartDateTime,
                    x.EndDateTime,
                })
                .ToListAsync();

            foreach (var driverTime in driversTime)
            {
                var start = driverTime.StartDateTime >= startDate ? driverTime.StartDateTime : startDate;
                DateTime end;
                if (driverTime.EndDateTime != null)
                {
                    end = driverTime.EndDateTime >= endDate ? endDate : driverTime.EndDateTime.Value;
                }
                else
                {
                    if (driverTime.StartDateTime >= currentDateStart && driverTime.StartDateTime < currentDateEnd)
                    {
                        end = nowDate;
                    }
                    else
                    {
                        end = driverTime.StartDateTime
                            .ConvertTimeZoneTo(timeZone)
                            .Date.AddDays(1)
                            .ConvertTimeZoneFrom(timeZone);
                    }
                }

                var startDateInUserTimeZone = start.ConvertTimeZoneTo(timeZone).Date;
                var firstMidnightAfterStart = start.ConvertTimeZoneTo(timeZone).Date.AddDays(1).ConvertTimeZoneFrom(timeZone);

                var hoursToAddTotal = Convert.ToDecimal((end - start).TotalHours);
                var hoursToAdd = hoursToAddTotal;
                decimal hoursRemainder = 0;

                if (end > firstMidnightAfterStart)
                {
                    hoursToAdd = Convert.ToDecimal((firstMidnightAfterStart - start).TotalHours);
                    hoursRemainder = hoursToAddTotal - hoursToAdd;
                }
                fillTimeCallback(new FillDriversTimeCallbackArgs
                {
                    EmployeeTimeId = driverTime.Id,
                    TruckId = driverTime.EquipmentId,
                    UserId = driverTime.UserId,
                    DriverId = driverTime.DriverId,
                    DeliveryDate = startDateInUserTimeZone,
                    HoursToAdd = hoursToAdd,
                });

                while (hoursRemainder > 0)
                {
                    startDateInUserTimeZone = startDateInUserTimeZone.AddDays(1);
                    hoursToAdd = hoursRemainder > 24 ? 24 : hoursRemainder;
                    hoursRemainder -= hoursToAdd;
                    fillTimeCallback(new FillDriversTimeCallbackArgs
                    {
                        EmployeeTimeId = driverTime.Id,
                        TruckId = driverTime.EquipmentId,
                        UserId = driverTime.UserId,
                        DriverId = driverTime.DriverId,
                        DeliveryDate = startDateInUserTimeZone,
                        HoursToAdd = hoursToAdd,
                    });
                }

            }
        }
    }
}
