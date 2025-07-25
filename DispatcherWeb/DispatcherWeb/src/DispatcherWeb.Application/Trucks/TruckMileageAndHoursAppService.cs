using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using DispatcherWeb.Configuration;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Trucks
{
    public class TruckMileageAndHoursAppService : DispatcherWebAppServiceBase, ITruckMileageAndHoursAppService
    {
        private readonly IRepository<VehicleUsage> _vehicleUsageRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Tenant> _tenantRepository;

        public TruckMileageAndHoursAppService(
            IRepository<VehicleUsage> vehicleUsageRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Tenant> tenantRepository
        )
        {
            _vehicleUsageRepository = vehicleUsageRepository;
            _orderLineRepository = orderLineRepository;
            _tenantRepository = tenantRepository;
        }

        [RemoteService(false)]
        [UnitOfWork]
        public async Task AddTrucksMileageAndHourForDayBeforeTickets()
        {
            var demoTenant = await (await _tenantRepository.GetQueryAsync())
                .Where(t => t.IsActive && t.Name != null && t.TenancyName != null && t.Name.Equals("demo") && t.TenancyName.Equals("demo"))
                .FirstAsync();

            if (demoTenant != null)
            {
                using (CurrentUnitOfWork.SetTenantId(demoTenant.Id))
                using (AbpSession.Use(demoTenant.Id, null))
                {
                    bool isGpsConfigured = await FeatureChecker.IsEnabledAsync(demoTenant.Id, AppFeatures.GpsIntegrationFeature);
                    if (!isGpsConfigured)
                    {
                        return;
                    }
                    var platform = (GpsPlatform)await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.GpsIntegration.Platform, demoTenant.Id);
                    if (platform == GpsPlatform.Geotab)
                    {
                        var geotabSettings = await SettingManager.GetGeotabSettingsAsync();
                        if (geotabSettings.IsEmpty())
                        {
                            return;
                        }
                    }

                    if (platform == GpsPlatform.Samsara)
                    {
                        var samsaraSettings = await SettingManager.GetSamsaraSettingsAsync();
                        if (samsaraSettings.IsEmpty())
                        {
                            return;
                        }
                    }

                    if (platform == GpsPlatform.DtdTracker)
                    {
                        var dtdTrackerSettings = await SettingManager.GetDtdTrackerSettingsAsync();
                        if (dtdTrackerSettings.IsEmpty())
                        {
                            return;
                        }
                    }

                    Logger.Info($"Updating mileage and hours for TenantId={demoTenant.Id}");

                    var timezone = await GetTimezone();
                    var today = await GetToday();
                    var readingDateTime = today.AddDays(-2).ConvertTimeZoneFrom(timezone);

                    var trucksWithTotalTicketsCountList = await (await _orderLineRepository.GetQueryAsync())
                        .SelectMany(ol => ol.Tickets)
                        .Where(t => t.TruckId != null && t.CarrierId == null && t.TicketDateTime != null && t.TicketDateTime.Value.Date == readingDateTime.Date)
                        .GroupBy(x => x.TruckId)
                        .Select(x => new
                        {
                            TruckId = x.Key ?? 0,
                            TotalTickets = x.Count(),
                        })
                        .ToListAsync();

                    int defaultMileage = 20;
                    int defaultHours = 2;

                    foreach (var truck in trucksWithTotalTicketsCountList)
                    {
                        var usageReadingDateTime = new DateTime(readingDateTime.Year, readingDateTime.Month, readingDateTime.Day, 09, 00, 00);
                        usageReadingDateTime = DateTime.SpecifyKind(usageReadingDateTime, DateTimeKind.Utc);

                        await _vehicleUsageRepository.InsertAsync(new VehicleUsage
                        {
                            TruckId = truck.TruckId,
                            ReadingDateTime = usageReadingDateTime,
                            ReadingType = ReadingType.Miles,
                            Reading = (decimal)defaultMileage * truck.TotalTickets,
                        });

                        await _vehicleUsageRepository.InsertAsync(new VehicleUsage
                        {
                            TruckId = truck.TruckId,
                            ReadingDateTime = usageReadingDateTime,
                            ReadingType = ReadingType.Hours,
                            Reading = (decimal)defaultHours * truck.TotalTickets,
                        });
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }
        }
    }
}
