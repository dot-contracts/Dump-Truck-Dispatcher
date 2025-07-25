using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Features;
using Abp.Application.Services;
using Abp.Auditing;
using Abp.Configuration;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Extensions;
using Abp.EntityFrameworkCore.Uow;
using Abp.Events.Bus;
using Abp.Events.Bus.Entities;
using Abp.Runtime.Session;
using Abp.TestBase;
using Abp.TestBase.Runtime.Session;
using Abp.Timing;
using Abp.Zero.Configuration;
using AutoFixture;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Customers;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Features;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Items;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Locations;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Payments;
using DispatcherWeb.Test.Base.Sessions;
using DispatcherWeb.Tests.TestDatas;
using DispatcherWeb.Trucks;
using DispatcherWeb.VehicleCategories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace DispatcherWeb.Tests
{
    /// <summary>
    /// This is base class for all our test classes.
    /// It prepares ABP system, modules and a fake, in-memory database.
    /// Seeds database with initial data.
    /// Provides methods to easily work with <see cref="DispatcherWebDbContext"/>.
    /// </summary>
    public abstract class AppTestBase : AbpIntegratedTestBase<DispatcherWebTestModule>
    {
        [Obsolete("Do not use TestAbpSession, use ExtendedTestAbpSession instead", error: true)]
        protected new TestAbpSession AbpSession { get; set; }

        protected ExtendedTestAbpSession Session { get; set; }

        protected ICurrentUnitOfWorkProvider CurrentUnitOfWorkProvider { get; set; }

        protected IUnitOfWorkManager UnitOfWorkManager { get; set; }

        protected AppTestBase()
        {
            Clock.Provider = ClockProviders.Utc;
            Session = Resolve<ExtendedTestAbpSession>();
            CurrentUnitOfWorkProvider = LocalIocManager.Resolve<ICurrentUnitOfWorkProvider>();
            UnitOfWorkManager = LocalIocManager.Resolve<IUnitOfWorkManager>();

            SeedTestData();
            LoginAsDefaultTenantAdmin();
        }

        private void SeedTestData()
        {
            void NormalizeDbContext(DispatcherWebDbContext context)
            {
                context.EntityChangeEventHelper = NullEntityChangeEventHelper.Instance;
                context.EventBus = NullEventBus.Instance;
                context.SuppressAutoSetTenantId = true;
            }

            //Seed initial data for default tenant
            Session.TenantId = 1;
            Session.OfficeId = 1;
            UsingDbContext(context =>
            {
                NormalizeDbContext(context);
                new TestDataBuilder(context, 1).Create();
            });
        }

        protected IDisposable UsingTenantId(int? tenantId)
        {
            var previousTenantId = Session.TenantId;
            Session.TenantId = tenantId;
            return new DisposeAction(() => Session.TenantId = previousTenantId);
        }

        protected void UsingDbContext(Action<DispatcherWebDbContext> action)
        {
            UsingDbContext(Session.TenantId, action);
        }

        protected Task UsingDbContextAsync(Func<DispatcherWebDbContext, Task> action)
        {
            return UsingDbContextAsync(Session.TenantId, action);
        }

        protected T UsingDbContext<T>(Func<DispatcherWebDbContext, T> func)
        {
            return UsingDbContext(Session.TenantId, func);
        }

        protected Task<T> UsingDbContextAsync<T>(Func<DispatcherWebDbContext, Task<T>> func)
        {
            return UsingDbContextAsync(Session.TenantId, func);
        }

        protected void UsingDbContext(int? tenantId, Action<DispatcherWebDbContext> action)
        {
            using (UsingTenantId(tenantId))
            {
                using (var context = LocalIocManager.Resolve<DispatcherWebDbContext>())
                {
                    action(context);
                    context.SaveChanges();
                }
            }
        }

        protected async Task UsingDbContextAsync(int? tenantId, Func<DispatcherWebDbContext, Task> action)
        {
            using (UsingTenantId(tenantId))
            {
                await using (var context = LocalIocManager.Resolve<DispatcherWebDbContext>())
                {
                    await action(context);
                    await context.SaveChangesAsync();
                }
            }
        }

        protected T UsingDbContext<T>(int? tenantId, Func<DispatcherWebDbContext, T> func)
        {
            T result;

            using (UsingTenantId(tenantId))
            {
                using (var context = LocalIocManager.Resolve<DispatcherWebDbContext>())
                {
                    result = func(context);
                    context.SaveChanges();
                }
            }

            return result;
        }

        protected async Task<T> UsingDbContextAsync<T>(int? tenantId, Func<DispatcherWebDbContext, Task<T>> func)
        {
            T result;

            using (UsingTenantId(tenantId))
            {
                await using (var context = LocalIocManager.Resolve<DispatcherWebDbContext>())
                {
                    result = await func(context);
                    await context.SaveChangesAsync();
                }
            }

            return result;
        }

        protected void SubstituteServiceDependencies(object service)
        {
            if (service is AbpServiceBase abpService)
            {
                abpService.SettingManager = SettingManager;
            }
            if (service is ApplicationService applicationService)
            {
                applicationService.FeatureChecker = FeatureChecker;
            }
            if (service is DomainService domainService)
            {
                domainService.SettingManager = SettingManager;
            }
            if (service is DispatcherWebDomainServiceBase domainServiceBase)
            {
                domainServiceBase.FeatureChecker = FeatureChecker;
            }
        }

        #region Settings

        private ISettingManager _settingManager;
        protected ISettingManager SettingManager
        {
            get
            {
                if (_settingManager != null)
                {
                    return _settingManager;
                }
                _settingManager = Substitute.For<ISettingManager>();

                var appSettingsProvider = Resolve<AppSettingProvider>();
                var settingDefinitionManager = Resolve<ISettingDefinitionManager>();
                var settingsDefinitions = appSettingsProvider.GetSettingDefinitions(new SettingDefinitionProviderContext(settingDefinitionManager));
                foreach (var settingDefinition in settingsDefinitions)
                {
                    _settingManager.GetSettingValueAsync(settingDefinition.Name).Returns(settingDefinition.DefaultValue);
                }
                _settingManager.GetSettingValueAsync(TimingSettingNames.TimeZone).Returns("UTC");
                _settingManager.GetSettingValueAsync(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEnabled).Returns("false");
                _settingManager.GetSettingValueAsync(AbpZeroSettingNames.UserManagement.UserLockOut.IsEnabled).Returns("false");

                return _settingManager;
            }
        }

        protected void SubstituteSetting(string settingName, string settingValue)
        {
            SettingManager.GetSettingValueAsync(settingName).Returns(settingValue);
        }

        protected void SubstituteSettingForUser(string settingName, string settingValue, UserIdentifier userIdentifier)
        {
            SettingManager.GetSettingValueForUserAsync(settingName, userIdentifier).Returns(settingValue);
            SettingManager.GetSettingValueForUserAsync(settingName, userIdentifier.TenantId, userIdentifier.UserId).Returns(settingValue);
        }

        protected void SubstituteSettingForTenant(string settingName, string settingValue, int tenantId)
        {
            SettingManager.GetSettingValueForTenantAsync(settingName, tenantId).Returns(settingValue);
        }

        protected void SubstituteDispatchViaSetting(DispatchVia dispatchVia)
        {
            SettingManager.GetSettingValueAsync(AppSettings.DispatchingAndMessaging.DispatchVia).Returns(((int)dispatchVia).ToString());
            SettingManager.GetSettingValueForTenantAsync(AppSettings.DispatchingAndMessaging.DispatchVia, Arg.Any<int>()).Returns(((int)dispatchVia).ToString());
            //not sure if we should use the above or below implementation, more tests needed
            //var defaultTenantId = GetTenant(Tenant.DefaultTenantName).Id;
            //SettingManager.GetSettingValueForTenantAsync(AppSettings.DispatchingAndMessaging.DispatchVia, defaultTenantId).Returns(((int)dispatchVia).ToString());
        }

        #endregion

        #region FeatureChecker

        private IFeatureChecker _featureChecker;
        protected IFeatureChecker FeatureChecker
        {
            get
            {
                if (_featureChecker != null)
                {
                    return _featureChecker;
                }
                _featureChecker = Substitute.For<IFeatureChecker>();
                //todo should we populate default values for anything?
                return _featureChecker;
            }
        }

        protected void SubstituteAllowLeaseHaulersFeature(bool value)
        {
            FeatureChecker.GetValueAsync(AppFeatures.AllowLeaseHaulersFeature).Returns(value.ToLowerCaseString());
        }

        protected void SubstituteAllowMultiOfficeFeature(bool value)
        {
            FeatureChecker.GetValueAsync(AppFeatures.AllowMultiOfficeFeature).Returns(value.ToLowerCaseString());
        }

        #endregion

        #region Login

        protected void LoginAsHostAdmin()
        {
            LoginAsHost(User.AdminUserName);
        }

        protected void LoginAsDefaultTenantAdmin()
        {
            LoginAsTenant(Tenant.DefaultTenantName, User.AdminUserName);
        }

        protected void LoginAsHost(string userName)
        {
            Session.TenantId = null;

            var user = UsingDbContext(context => context.Users.FirstOrDefault(u => u.TenantId == Session.TenantId && u.UserName == userName));
            if (user == null)
            {
                throw new Exception("There is no user: " + userName + " for host.");
            }

            Session.UserId = user.Id;
        }

        protected void LoginAsTenant(string tenancyName, string userName)
        {
            Session.TenantId = null;

            var tenant = UsingDbContext(context => context.Tenants.FirstOrDefault(t => t.TenancyName == tenancyName));
            if (tenant == null)
            {
                throw new Exception("There is no tenant: " + tenancyName);
            }

            Session.TenantId = tenant.Id;

            var user = UsingDbContext(context => context.Users.FirstOrDefault(u => u.TenantId == Session.TenantId && u.UserName == userName));
            if (user == null)
            {
                throw new Exception("There is no user: " + userName + " for tenant: " + tenancyName);
            }

            Session.UserId = user.Id;
        }

        #endregion

        #region GetCurrentUser

        /// <summary>
        /// Gets current user if <see cref="IAbpSession.UserId"/> is not null.
        /// Throws exception if it's null.
        /// </summary>
        protected User GetCurrentUser()
        {
            var userId = Session.GetUserId();
            return UsingDbContext(context => context.Users.Single(u => u.Id == userId));
        }

        /// <summary>
        /// Gets current user if <see cref="IAbpSession.UserId"/> is not null.
        /// Throws exception if it's null.
        /// </summary>
        protected async Task<User> GetCurrentUserAsync()
        {
            var userId = Session.GetUserId();
            return await UsingDbContext(context => context.Users.SingleAsync(u => u.Id == userId));
        }

        #endregion

        #region GetCurrentTenant

        /// <summary>
        /// Gets current tenant if <see cref="IAbpSession.GetTenantIdOrNullAsync"/> is not null.
        /// Throws exception if there is no current tenant.
        /// </summary>
        protected async Task<Tenant> GetCurrentTenantAsync()
        {
            var tenantId = await Session.GetTenantIdAsync();
            return await UsingDbContextAsync(null, context => context.Tenants.SingleAsync(t => t.Id == tenantId));
        }

        #endregion

        #region GetTenant / GetTenantOrNull

        protected Tenant GetTenant(string tenancyName)
        {
            return UsingDbContext(null, context => context.Tenants.Single(t => t.TenancyName == tenancyName));
        }

        protected async Task<Tenant> GetTenantAsync(string tenancyName)
        {
            return await UsingDbContext(null, async context => await context.Tenants.SingleAsync(t => t.TenancyName == tenancyName));
        }

        protected Tenant GetTenantOrNull(string tenancyName)
        {
            return UsingDbContext(null, context => context.Tenants.FirstOrDefault(t => t.TenancyName == tenancyName));
        }

        protected async Task<Tenant> GetTenantOrNullAsync(string tenancyName)
        {
            return await UsingDbContext(null, async context => await context.Tenants.FirstOrDefaultAsync(t => t.TenancyName == tenancyName));
        }

        #endregion

        #region GetRole

        protected Role GetRole(string roleName)
        {
            return UsingDbContext(context => context.Roles.Single(r => r.Name == roleName && r.TenantId == Session.TenantId));
        }

        protected async Task<Role> GetRoleAsync(string roleName)
        {
            return await UsingDbContext(async context => await context.Roles.SingleAsync(r => r.Name == roleName && r.TenantId == Session.TenantId));
        }

        #endregion

        #region GetUserByUserName

        protected User GetUserByUserName(string userName)
        {
            var user = GetUserByUserNameOrNull(userName);
            if (user == null)
            {
                throw new Exception("Can not find a user with username: " + userName);
            }

            return user;
        }

        protected async Task<User> GetUserByUserNameAsync(string userName)
        {
            var user = await GetUserByUserNameOrNullAsync(userName);
            if (user == null)
            {
                throw new Exception("Can not find a user with username: " + userName);
            }

            return user;
        }

        protected User GetUserByUserNameOrNull(string userName)
        {
            return UsingDbContext(context =>
                context.Users.FirstOrDefault(u =>
                    u.UserName == userName
                    && u.TenantId == Session.TenantId
                    ));
        }

        protected async Task<User> GetUserByUserNameOrNullAsync(string userName, bool includeRoles = false)
        {
            return await UsingDbContextAsync(async context =>
                await context.Users
                    .IncludeIf(includeRoles, u => u.Roles)
                    .FirstOrDefaultAsync(u =>
                            u.UserName == userName
                            && u.TenantId == Session.TenantId
                    ));
        }

        #endregion

        protected async Task SetOrderLineNumberOfTrucks(int orderLineId, double numberOfTrucks)
        {
            await UsingDbContextAsync(async context =>
            {
                var orderLine = await context.OrderLines.FindAsync(orderLineId);
                orderLine.NumberOfTrucks = numberOfTrucks;
                orderLine.ScheduledTrucks = numberOfTrucks;
                context.Update(orderLine);
            });

        }

        protected async Task<VehicleCategory> CreateTrailerVehicleCategory()
        {
            return await CreateVehicleCategory(new VehicleCategory { Name = "Trailer", SortOrder = 2, AssetType = AssetType.Trailer, IsPowered = false });
        }

        protected async Task<VehicleCategory> CreateTractorVehicleCategory()
        {
            return await CreateVehicleCategory(new VehicleCategory { Name = "Tractor", SortOrder = 1, AssetType = AssetType.Tractor, IsPowered = true });
        }

        protected async Task<VehicleCategory> CreateVehicleCategory(VehicleCategory vehicleCategory = null)
        {
            var vehicleCategoryEntity = await UsingDbContextAsync(async context =>
            {
                vehicleCategory ??= new VehicleCategory
                {
                    Name = "DumpTruck",
                    AssetType = AssetType.DumpTruck,
                    IsPowered = true,
                    SortOrder = 1,
                };
                var existingVehicleCategory = await context.VehicleCategories
                    .FirstOrDefaultAsync(x => x.Id == vehicleCategory.Id
                        || x.Name == vehicleCategory.Name
                        && x.AssetType == vehicleCategory.AssetType
                        && x.IsPowered == vehicleCategory.IsPowered
                        && x.SortOrder == vehicleCategory.SortOrder);
                if (existingVehicleCategory != null)
                {
                    vehicleCategory = existingVehicleCategory;
                }
                else
                {
                    await context.VehicleCategories.AddAsync(vehicleCategory);
                }
                return vehicleCategory;
            });
            return vehicleCategoryEntity;
        }

        protected async Task<Truck> CreateTruck(string truckCode = "101", VehicleCategory vehicleCategory = null, int officeId = 1, int tenantId = 1, bool canPullTrailer = false, bool alwaysShowOnSchedule = false)
        {
            var truckEntity = await UsingDbContextAsync(async context =>
            {
                var office = await context.Offices.Where(o => o.Id == officeId).FirstAsync();
                vehicleCategory = await CreateVehicleCategory(vehicleCategory);

                var truck = new Truck
                {
                    TenantId = tenantId,
                    TruckCode = truckCode,
                    VehicleCategoryId = vehicleCategory.Id,
                    Office = office,
                    IsActive = true,
                    AlwaysShowOnSchedule = true,
                    IsOutOfService = false,
                    CanPullTrailer = canPullTrailer,
                };
                await context.Trucks.AddAsync(truck);

                if (alwaysShowOnSchedule)
                {
                    var leaseHauler = new LeaseHauler
                    {
                        Name = "Unknown",
                    };
                    await context.LeaseHaulers.AddAsync(leaseHauler);
                    var leaseHaulerTruck = new LeaseHaulerTruck
                    {
                        Truck = truck,
                        LeaseHauler = leaseHauler,
                        AlwaysShowOnSchedule = alwaysShowOnSchedule,
                    };
                    await context.LeaseHaulerTrucks.AddAsync(leaseHaulerTruck);
                }

                return truck;
            });
            return truckEntity;
        }

        protected async Task<OrderLineTruck> CreateOrderLineTruck(int truckId, int? driverId, int orderLineId, decimal utilization)
        {
            return await UsingDbContextAsync(async context =>
            {
                var orderLine = await context.OrderLines.FirstAsync(x => x.Id == orderLineId);

                var orderLineTruck = new OrderLineTruck
                {
                    TenantId = 1,
                    OrderLineId = orderLineId,
                    TruckId = truckId,
                    DriverId = driverId,
                    Utilization = utilization,
                    IsDone = utilization == 0,
                    TimeOnJob = orderLine.TimeOnJob,
                };
                await context.OrderLineTrucks.AddAsync(orderLineTruck);
                return orderLineTruck;
            });
        }

        protected async Task<Office> CreateOfficeAndAssignUserToIt()
        {
            var existingOffice = await UsingDbContextAsync(async context =>
            {
                return await context.Offices.OrderBy(x => x.Id).FirstAsync(x => x.TenantId == Session.TenantId);
            });

            if (existingOffice != null)
            {
                return existingOffice;
            }

            var office = await CreateOffice("Office1", "111");
            await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstAsync(u => u.TenantId == 1);
                user.OfficeId = office.Id;
                Session.OfficeId = office.Id;
            });
            return office;
        }

        protected async Task<Office> CreateOffice(string officeName = "Office2", string truckColor = "222")
        {
            var officeEntity = await UsingDbContextAsync(async context =>
            {
                var office = new Office() { TenantId = 1, Name = officeName, TruckColor = truckColor };
                await context.Offices.AddAsync(office);
                return office;
            });
            return officeEntity;
        }

        protected async Task<Order> CreateOrderWithOrderLines(DateTime? date = null, Shift? shift = null)
        {
            date ??= DateTime.Today;

            var orderEntity = await UsingDbContextAsync(async context =>
            {
                var order = new Order
                {
                    TenantId = 1,
                    DeliveryDate = date.Value,
                    Shift = shift,
                    Customer = context.Customers.FirstOrDefault() ?? new Customer() { TenantId = 1, Name = "Cust" },
                    Office = context.Offices.FirstOrDefault() ?? new Office() { TenantId = 1, Name = "Office1", TruckColor = "fff" },
                    SalesTaxRate = 2,
                };
                order.OrderLines.Add(new OrderLine
                {
                    TenantId = 1,
                    LineNumber = 1,
                    Designation = DesignationEnum.FreightAndMaterial,
                    MaterialQuantity = 1,
                    FreightQuantity = 1,
                    FreightPricePerUnit = 2,
                    MaterialPricePerUnit = 3,
                    FreightItem = new Item
                    {
                        TenantId = 1,
                        Name = "sss",
                        IsTaxable = true,
                    },
                    LoadAt = new Location
                    {
                        TenantId = 1,
                        Name = "Location1",
                    },
                    DeliverTo = new Location
                    {
                        TenantId = 1,
                        Name = "Location2",
                    },
                    MaterialUomId = 1,
                    FreightUomId = 1,
                    NumberOfTrucks = 2,
                    ScheduledTrucks = 2,
                    TimeOnJob = Clock.Now.Date.AddHours(1),
                });
                order.OrderLines.Add(new OrderLine
                {
                    TenantId = 1,
                    LineNumber = 2,
                    Designation = DesignationEnum.FreightAndMaterial,
                    FreightPricePerUnit = 20,
                    MaterialPricePerUnit = 30,
                    MaterialQuantity = 2,
                    FreightQuantity = 2,
                    FreightPrice = 20 * 2,
                    MaterialPrice = 30 * 2,
                    FreightItem = new Item
                    {
                        TenantId = 1,
                        Name = "sss",
                        IsTaxable = true,
                    },
                    LoadAt = new Location
                    {
                        TenantId = 1,
                        Name = "Location2",
                    },
                    MaterialUomId = 1,
                    FreightUomId = 1,
                    NumberOfTrucks = 2,
                    ScheduledTrucks = 2,
                    TimeOnJob = Clock.Now.Date.AddHours(2),
                });
                await context.Orders.AddAsync(order);

                return order;
            });
            return orderEntity;
        }

        protected async Task<Payment> CreateOrderPayment(Order order)
        {
            return await UsingDbContextAsync(async context =>
            {
                var payment = new Payment
                {
                    TenantId = order.TenantId,
                };
                await context.Payments.AddAsync(payment);

                await context.OrderPayments.AddAsync(new OrderPayment
                {
                    TenantId = order.TenantId,
                    OrderId = order.Id,
                    Payment = payment,
                });

                return payment;
            });
        }

        protected async Task<Driver> CreateDriver(string firstName = null, string lastName = null, int? officeId = null, string phoneNumber = null, string email = null)
        {
            return await UsingDbContextAsync(async context =>
            {
                var driver = new Driver
                {
                    TenantId = 1,
                    FirstName = firstName ?? "fn",
                    LastName = lastName ?? "ln",
                    OfficeId = officeId,
                    CellPhoneNumber = phoneNumber,
                    EmailAddress = email,
                };
                await context.Drivers.AddAsync(driver);
                return driver;
            });
        }

        protected async Task<DriverAssignment> CreateDriverAssignmentForTruck(int? officeId, int truckId, DateTime date, int? driverId = null) =>
            await CreateDriverAssignmentForTruck(officeId, truckId, date, null, driverId);

        protected async Task<DriverAssignment> CreateDriverAssignmentForTruck(int? officeId, int truckId, DateTime date, Shift? shift, int? driverId = null)
        {
            return await UsingDbContextAsync(async context =>
            {
                var truck = await context.Trucks.FindAsync(truckId);
                var driverAssignment = new DriverAssignment
                {
                    TenantId = 1,
                    Date = date,
                    Shift = shift,
                    OfficeId = officeId,
                    DriverId = driverId,
                };
                truck.DriverAssignments.Add(driverAssignment);
                return driverAssignment;
            });
        }

        protected async Task<Driver> SetDefaultDriverForTruck(int truckId)
        {
            return await UsingDbContextAsync(async context =>
            {
                var truck = await context.Trucks.FindAsync(truckId);
                var driver = new Driver
                {
                    TenantId = 1,
                    FirstName = "fn",
                    LastName = "ln",
                };
                truck.DefaultDriver = driver;
                await context.SaveChangesAsync();
                return driver;
            });
        }

        protected async Task<Dispatch> CreateDispatch(int truckId, int driverId, int orderLineId, DispatchStatus status, string message = null)
        {
            return await UsingDbContextAsync(async context =>
            {
                var user = await context.Users.FirstAsync(u => u.TenantId == 1);
                var dispatch = new Dispatch
                {
                    TenantId = 1,
                    Guid = Guid.NewGuid(),
                    UserId = user.Id,
                    TruckId = truckId,
                    DriverId = driverId,
                    OrderLineId = orderLineId,
                    Status = status,
                    Message = message,
                    Note = "Note",
                };

                if (status == DispatchStatus.Loaded || status == DispatchStatus.Completed)
                {
                    var load = new Load
                    {
                        SourceDateTime = Clock.Now,
                        DestinationDateTime = status == DispatchStatus.Completed ? Clock.Now : (DateTime?)null,
                    };
                    dispatch.Loads = new List<Load>() { load };
                }

                await context.Dispatches.AddAsync(dispatch);

                dispatch.SortOrder = dispatch.Id;
                return dispatch;
            });
        }

        protected async Task<Load> CreateLoad(int dispatchId, DateTime? sourceDateTime = null, DateTime? destinationDateTime = null)
        {
            return await UsingDbContextAsync(async context =>
            {
                var load = new Load
                {
                    DispatchId = dispatchId,
                    SourceDateTime = sourceDateTime,
                    DestinationDateTime = destinationDateTime,
                };
                await context.Loads.AddAsync(load);
                return load;
            });
        }

        protected async Task<Ticket> CreateTicket(OrderLine orderLine, Truck truck, string ticketNumber = "12345", decimal quantity = 12.34m, int? uomId = 1)
        {
            var ticketEntity = await UsingDbContextAsync(async context =>
            {
                var t = new Ticket
                {
                    TenantId = 1,
                    OrderLineId = orderLine.Id,
                    OfficeId = orderLine.Order.OfficeId,
                    TruckId = truck.Id,
                    TruckCode = truck.TruckCode,
                    CustomerId = orderLine.Order.CustomerId,
                    FreightItemId = orderLine.FreightItemId,
                    TicketDateTime = orderLine.Order.DeliveryDate,
                    TicketNumber = ticketNumber,
                    MaterialQuantity = quantity,
                    FreightUomId = uomId,
                };
                await context.Tickets.AddAsync(t);
                return t;
            });
            return ticketEntity;
        }



        protected async Task<T> UpdateEntity<T>(T entity, Action<T> action) where T : class
        {
            return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var context = await UnitOfWorkManager.Current.GetDbContextAsync<DispatcherWebDbContext>();
                action(entity);
                context.Update(entity);
                return entity;
            });
        }

        protected async Task ChangeOrderOffice(int orderId, int officeId)
        {
            await UsingDbContextAsync(async context =>
                {
                    var order = await context.Orders.FindAsync(orderId);
                    order.OfficeId = officeId;
                }
            );
        }

        protected async Task<FuelPurchase> CreateFuelPurchase(
            int truckId,
            DateTime fuelDateTime,
            decimal? amount,
            decimal? rate,
            decimal? odometer,
            string ticketNumber)
        {
            return await UsingDbContextAsync(async context =>
            {
                var fuelPurchase = new FuelPurchase
                {
                    TenantId = 1,
                    TruckId = truckId,
                    FuelDateTime = fuelDateTime,
                    Amount = amount,
                    Rate = rate,
                    Odometer = odometer,
                    TicketNumber = ticketNumber,
                };
                await context.FuelPurchases.AddAsync(fuelPurchase);
                return fuelPurchase;
            });
        }

        protected async Task<User> CreateUser(int tenantId, bool isActive = true, DateTime? creationTime = null)
        {
            var fixture = new Fixture();
            var userEntity = await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var context = await UnitOfWorkManager.Current.GetDbContextAsync<DispatcherWebDbContext>();
                var user = fixture.Build<User>()
                    .OmitAutoProperties()
                    .With(x => x.TenantId, tenantId)
                    .With(x => x.IsActive, isActive)
                    .With(x => x.CreationTime, creationTime ?? new DateTime(2019, 1, 1))
                    .With(x => x.Name, fixture.Create<string>())
                    .With(x => x.UserName, fixture.Create<string>())
                    .With(x => x.EmailAddress, $"{fixture.Create<string>()}@example.com")
                    .With(x => x.Surname, fixture.Create<string>())
                    .With(x => x.Password, fixture.Create<string>())
                    .With(x => x.NormalizedUserName, fixture.Create<string>())
                    .With(x => x.NormalizedEmailAddress, fixture.Create<string>())
                    .Create();
                await context.Users.AddAsync(user);
                return user;
            });
            return userEntity;
        }

        protected async Task CreateAuditLog(int? tenantId, long? userId, DateTime executionTime, int executionDuration = 0, string serviceName = null, string methodName = null)
        {
            var fixture = new Fixture();
            await UsingDbContextAsync(async context =>
            {
                var auditLog = fixture.Build<AuditLog>()
                    .OmitAutoProperties()
                    .With(x => x.TenantId, tenantId)
                    .With(x => x.UserId, userId)
                    .With(x => x.ExecutionTime, executionTime)
                    .With(x => x.ExecutionDuration, executionDuration)
                    .With(x => x.ServiceName, serviceName ?? fixture.Create<string>())
                    .With(x => x.MethodName, methodName ?? fixture.Create<string>())
                    .Create();
                await context.AuditLogs.AddAsync(auditLog);
            });
        }

        protected async Task<DispatcherWeb.Drivers.EmployeeTime> CreateEmployeeTime(long userId, DateTime startDateTime, DateTime? endDateTime, int truckId)
        {
            return await UsingDbContextAsync(async context =>
            {
                var timeClassification = await context.TimeClassifications.FirstOrDefaultAsync();
                if (timeClassification == null)
                {
                    timeClassification = new TimeClassifications.TimeClassification { Name = "Driver Truck" };
                    await context.TimeClassifications.AddAsync(timeClassification);
                    await context.SaveChangesAsync();
                }

                var entity = new DispatcherWeb.Drivers.EmployeeTime
                {
                    TenantId = 1,
                    UserId = userId,
                    StartDateTime = startDateTime,
                    EndDateTime = endDateTime,
                    TimeClassificationId = timeClassification.Id,
                    EquipmentId = truckId,
                };
                await context.EmployeeTime.AddAsync(entity);
                return entity;
            });
        }

        protected async Task<Tenant> CreateTenant(string tenantName)
        {
            return await UsingDbContextAsync(async context =>
            {
                var entity = new Tenant(tenantName, tenantName)
                {
                };
                await context.Tenants.AddAsync(entity);
                return entity;
            });
        }

        protected async Task<Truck> CreateLeaseHaulerTruck(int leaseHaulerId, int? defaultDriverId = null, string truckCode = "LH001", VehicleCategory vehicleCategory = null, bool canPullTrailer = false)
        {
            var truckEntity = await UsingDbContextAsync(async context =>
            {
                var leaseHaulerTruck = new LeaseHaulerTruck
                {
                    TenantId = 1,
                    LeaseHaulerId = leaseHaulerId,
                };
                vehicleCategory = await CreateVehicleCategory();
                var truck = new Truck
                {
                    TenantId = 1,
                    TruckCode = truckCode,
                    VehicleCategoryId = vehicleCategory.Id,
                    Office = null, //external lease haulers
                    IsActive = true,
                    IsOutOfService = false,
                    DefaultDriverId = defaultDriverId,
                    CanPullTrailer = canPullTrailer,
                    LeaseHaulerTruck = leaseHaulerTruck,
                };
                await context.Trucks.AddAsync(truck);
                return truck;
            });
            return truckEntity;
        }

        protected async Task<Driver> CreateLeaseHaulerDriver(int leaseHaulerId, string firstName = "fn1", string lastName = "ln1", int? officeId = 1, string phoneNumber = null, string email = null)
        {
            var driverEntity = await UsingDbContextAsync(async context =>
            {
                var leaseHaulerDriver = new LeaseHaulerDriver
                {
                    TenantId = 1,
                    LeaseHaulerId = leaseHaulerId,
                };
                var driver = new Driver
                {
                    TenantId = 1,
                    FirstName = firstName,
                    LastName = lastName,
                    OfficeId = officeId,
                    CellPhoneNumber = phoneNumber,
                    EmailAddress = email,
                    LeaseHaulerDriver = leaseHaulerDriver,
                };
                await context.Drivers.AddAsync(driver);
                return driver;
            });
            return driverEntity;
        }

        protected async Task<AvailableLeaseHaulerTruck> CreateAvailableLeaseHaulerTruck(int leaseHaulerId, int truckId, int driverId, DateTime date, Shift? shift, int officeId)
        {
            return await UsingDbContextAsync(async context =>
            {
                var availableLeaseHaulerTruck = new AvailableLeaseHaulerTruck
                {
                    TenantId = 1,
                    LeaseHaulerId = leaseHaulerId,
                    TruckId = truckId,
                    DriverId = driverId,
                    Date = date,
                    Shift = shift,
                    OfficeId = officeId,
                };
                await context.AvailableLeaseHaulerTruck.AddAsync(availableLeaseHaulerTruck);
                return availableLeaseHaulerTruck;
            });
        }

        protected async Task<LeaseHauler> CreateLeaseHauler(string name = "LeaseHauler1")
        {
            return await UsingDbContextAsync(async context =>
            {
                var leaseHauler = new LeaseHauler
                {
                    TenantId = 1,
                    Name = name,
                };
                await context.LeaseHaulers.AddAsync(leaseHauler);
                return leaseHauler;
            });
        }

        protected async Task<LeaseHaulerRequest> CreateLeaseHaulerRequest(int leaseHaulerId, DateTime date, Shift? shift, int? available, int? approved, int officeId = 1)
        {
            return await UsingDbContextAsync(async context =>
            {
                var request = new LeaseHaulerRequest
                {
                    TenantId = 1,
                    Guid = Guid.NewGuid(),
                    LeaseHaulerId = leaseHaulerId,
                    Date = date,
                    Shift = shift,
                    Available = available,
                    Approved = approved,
                    OfficeId = officeId,
                };
                await context.LeaseHaulerRequests.AddAsync(request);
                return request;
            });
        }
    }
}
