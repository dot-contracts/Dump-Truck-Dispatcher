using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Drivers;
using DispatcherWeb.Offices;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Trucks;
using DispatcherWeb.VehicleCategories;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.DemoData
{
    [AbpAuthorize(AppPermissions.Pages_Tenants)]
    public class DemoDataAppService : DispatcherWebAppServiceBase, IDemoDataAppService
    {
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<VehicleCategory> _vehicleCategoryRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<FuelPurchase> _fuelPurchaseRepository;
        private readonly IRepository<VehicleUsage> _vehicleUsageRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IRepository<User, long> _userRepository;

        public DemoDataAppService(
            IRepository<Driver> driverRepository,
            IRepository<Truck> truckRepository,
            IRepository<VehicleCategory> vehicleCategoryRepository,
            IRepository<Office> officeRepository,
            IRepository<FuelPurchase> fuelPurchaseRepository,
            IRepository<VehicleUsage> vehicleUsageRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            IRepository<User, long> userRepository
        )
        {
            _driverRepository = driverRepository;
            _truckRepository = truckRepository;
            _vehicleCategoryRepository = vehicleCategoryRepository;
            _officeRepository = officeRepository;
            _fuelPurchaseRepository = fuelPurchaseRepository;
            _vehicleUsageRepository = vehicleUsageRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _userRepository = userRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_Tenants_AddDemoData)]
        public async Task CreateDemoData(EntityDto input)
        {
            var adminUser = await GetAdminUser(input.Id);
            var drivers = await AddDrivers(input.Id, adminUser.OfficeId);
            var trucks = await AddTrucks(input.Id, drivers);
            await AddFuelPurchases(input.Id, trucks);
            await AddVehicleUsage(input.Id, trucks, ReadingType.Hours);
            await AddVehicleUsage(input.Id, trucks, ReadingType.Miles);
            await AddTimeClassifications(input.Id);
        }

        private async Task<User> GetAdminUser(int tenantId)
        {
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var user = await (await _userRepository.GetQueryAsync())
                    .Where(x => x.TenantId == tenantId && x.UserName == "admin")
                    .FirstOrDefaultAsync();
                return user;
            }
        }

        private async Task<List<Driver>> AddDrivers(int tenantId, int? officeId)
        {
            var names = new List<string>
            {
                "Driver-1",
                "Driver-2",
                "Driver-3",
                "Driver-7",
            };

            var drivers = await (await _driverRepository.GetQueryAsync())
                .Where(x =>
                    x.TenantId == tenantId
                    && x.FirstName != null
                    && names.Contains(x.FirstName)
                )
                .ToListAsync();

            foreach (var name in names)
            {
                if (!drivers.Any(x => x.FirstName == name))
                {
                    //the user should not be assigned directly, instead we should be using DriverUserLinkService.UpdateUser(driver, username, false)
                    //var user = new User() { EmailAddress = item.Replace("-", "").Trim() + "@demo.com", Surname = item.Replace("-", ""), UserName = item.Replace("-", ""), Password = item.Replace("-", ""), IsActive = true, Name = item, OfficeId = officeId, TenantId = tenantId };
                    //await UserManager.CreateAsync(user);

                    var driver = new Driver
                    {
                        FirstName = name,
                        LastName = "Demo",
                        TenantId = tenantId,
                        OfficeId = officeId,
                    };
                    //driver.UserId = user.Id; //the user should not be assigned directly, instead we should be using DriverUserLinkService.UpdateUser(driver, username, false)
                    await _driverRepository.InsertAndGetIdAsync(driver);
                }
            }

            drivers = await (await _driverRepository.GetQueryAsync())
                .Where(x =>
                    x.TenantId == tenantId
                    && x.FirstName != null
                    && names.Contains(x.FirstName)
                )
                .ToListAsync();

            return drivers;
        }

        private async Task<List<Truck>> AddTrucks(int tenantId, List<Driver> drivers)
        {
            var offices = await (await _officeRepository.GetQueryAsync())
                .Where(x => x.TenantId == tenantId)
                .ToListAsync();
            var vehicleCategory = await _vehicleCategoryRepository.GetAsync(1);
            var truckCodes = new List<string>
            {
                "Truck-1",
                "Truck-2",
                "Truck-3",
            };
            var trucks = await (await _truckRepository.GetQueryAsync())
                .Where(x =>
                    x.TenantId == tenantId
                    && x.TruckCode != null
                    && truckCodes.Contains(x.TruckCode)
                )
                .ToListAsync();
            var index = 0;
            foreach (var truckCode in truckCodes)
            {
                foreach (var office in offices)
                {
                    if (
                        !trucks
                            .Any(x =>
                                x.TruckCode == truckCode
                                && x.Office != null
                                && x.Office.Id == office.Id
                            )
                    )
                    {
                        var entity = new Truck
                        {
                            TruckCode = truckCode,
                            VehicleCategory = vehicleCategory,
                            IsActive = true,
                            DefaultDriver = drivers[index],
                            TenantId = tenantId,
                            OfficeId = office.Id,
                        };
                        await _truckRepository.InsertOrUpdateAndGetIdAsync(entity);
                    }
                }
                index++;
            }

            trucks = await (await _truckRepository.GetQueryAsync())
                .Where(x =>
                    x.TenantId == tenantId
                    && x.TruckCode != null
                    && truckCodes.Contains(x.TruckCode)
                )
                .ToListAsync();

            return trucks;
        }

        private async Task AddFuelPurchases(int tenantId, List<Truck> trucks)
        {
            var today = await GetToday();

            for (var i = 0; i < 3; i++)
            {
                if (i > 0)
                {
                    today = today.AddDays(1);
                }

                foreach (var truck in trucks)
                {
                    var hasFuelPurchase = await (await _fuelPurchaseRepository.GetQueryAsync())
                        .Where(x =>
                            x.TenantId == tenantId
                            && x.FuelDateTime.Date == today
                            && x.TruckId == truck.Id
                        )
                        .AnyAsync();
                    if (!hasFuelPurchase)
                    {
                        var entity = new FuelPurchase
                        {
                            TruckId = truck.Id,
                            FuelDateTime = today,
                            Amount = 100,
                            Rate = 5,
                            TenantId = tenantId,
                        };
                        await _fuelPurchaseRepository.InsertOrUpdateAndGetIdAsync(entity);
                    }
                }
            }
        }

        private async Task AddVehicleUsage(
            int tenantId,
            List<Truck> trucks,
            ReadingType readingType
        )
        {
            var today = await GetToday();

            var reading = 50;

            for (var i = 0; i < 3; i++)
            {
                if (i > 0)
                {
                    today = today.AddDays(2);
                }

                foreach (var truck in trucks)
                {
                    var hasVehicleUsage = await (await _vehicleUsageRepository.GetQueryAsync())
                        .Where(x =>
                            x.TenantId == tenantId
                            && x.Truck.Id == truck.Id
                            && x.ReadingDateTime.Date == today
                            && x.ReadingType == readingType
                        )
                        .AnyAsync();

                    if (!hasVehicleUsage)
                    {
                        var entity = new VehicleUsage
                        {
                            TruckId = truck.Id,
                            ReadingDateTime = today,
                            Reading = reading,
                            ReadingType = readingType,
                            TenantId = tenantId,
                        };
                        await _vehicleUsageRepository.InsertOrUpdateAndGetIdAsync(entity);
                        reading += 50;

                        entity = new VehicleUsage
                        {
                            TruckId = truck.Id,
                            ReadingDateTime = today.AddDays(1),
                            Reading = reading,
                            ReadingType = readingType,
                            TenantId = tenantId,
                        };
                        await _vehicleUsageRepository.InsertOrUpdateAndGetIdAsync(entity);
                        reading += 50;
                    }
                }
            }
        }

        private async Task<List<TimeClassification>> AddTimeClassifications(int tenantId)
        {
            var names = new List<string>
            {
                "Drive Truck",
                "Production Pay",
            };
            var timeClassifications = await (await _timeClassificationRepository.GetQueryAsync())
                .Where(x => x.TenantId == tenantId && x.Name != null && names.Contains(x.Name))
                .ToListAsync();

            foreach (var name in names)
            {
                if (!timeClassifications.Any(x => x.Name == name))
                {
                    var entity = new TimeClassification
                    {
                        Name = name,
                        DefaultRate = 5,
                        TenantId = tenantId,
                    };
                    if (name == "Production Pay")
                    {
                        entity.IsProductionBased = true;
                    }
                    await _timeClassificationRepository.InsertOrUpdateAndGetIdAsync(entity);
                }
            }
            timeClassifications = await (await _timeClassificationRepository.GetQueryAsync())
                .Where(x => x.TenantId == tenantId && x.Name != null && names.Contains(x.Name))
                .ToListAsync();
            return timeClassifications;
        }
    }
}
