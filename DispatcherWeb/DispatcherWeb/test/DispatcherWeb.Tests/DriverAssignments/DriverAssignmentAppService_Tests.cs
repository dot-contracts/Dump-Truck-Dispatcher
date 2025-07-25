using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.EntityFrameworkCore.Uow;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Customers;
using DispatcherWeb.DriverAssignments;
using DispatcherWeb.DriverAssignments.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Items;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Trucks;
using DispatcherWeb.VehicleCategories;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.DriverAssignments
{
    public class DriverAssignmentAppService_Tests : AppTestBase, IAsyncLifetime
    {
        private IDriverAssignmentAppService _driverAssignmentAppService;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _driverAssignmentAppService = Resolve<IDriverAssignmentAppService>();
        }

        [Fact]
        public async Task Test_SetDriverForTruck_Should_set_driver_to_DriverAssignment()
        {
            // Arrange
            var truckEntity = await CreateTruckEntity();
            var driverEntity = await CreateDriver();
            var date = Clock.Now.Date;
            await _driverAssignmentAppService.SetNoDriverForTruck(new SetNoDriverForTruckInput
            {
                TruckId = truckEntity.Id,
                StartDate = date,
                EndDate = date,
            });

            // Act
            await _driverAssignmentAppService.SetDriverForTruck(new SetDriverForTruckInput
            {
                TruckId = truckEntity.Id,
                DriverId = driverEntity.Id,
                Date = date,
            });

            // Assert
            var driverAssignments = await UsingDbContextAsync(async context =>
                await context.DriverAssignments.Where(da => da.IsDeleted == false && da.TruckId == truckEntity.Id).ToListAsync()
            );
            driverAssignments.Count().ShouldBe(1);
            var driverAssignment = driverAssignments.First();
            driverAssignment.DriverId.ShouldBe(driverEntity.Id);
        }

        [Fact]
        public async Task Test_SetDriverForTruck_Should_assign_another_driver_to_DriverAssignment()
        {
            // Arrange
            var truckEntity = await CreateTruckEntity();
            var driver1 = await CreateDriver();
            var driver2 = await CreateDriver();
            var date = Clock.Now.Date;
            await UsingDbContextAsync(async context =>
            {
                var da = new DriverAssignment
                {
                    TenantId = 1,
                    Date = date,
                    OfficeId = truckEntity.OfficeId,
                    TruckId = truckEntity.Id,
                    DriverId = driver1.Id,
                };
                await context.DriverAssignments.AddAsync(da);
            });

            // Act
            await _driverAssignmentAppService.SetDriverForTruck(new SetDriverForTruckInput
            {
                TruckId = truckEntity.Id,
                DriverId = driver2.Id,
                Date = date,
            });

            // Assert
            var driverAssignments = await UsingDbContextAsync(async context =>
                await context.DriverAssignments.Where(da => da.IsDeleted == false && da.TruckId == truckEntity.Id).ToListAsync()
            );
            driverAssignments.Count().ShouldBe(1);
            var driverAssignment = driverAssignments.First();
            driverAssignment.DriverId.ShouldBe(driver2.Id);
        }

        [Fact]
        public async Task Test_SetDriverForTruck_should_throw_ArgumentException_when_Truck_is_Trailer()
        {
            // Arrange
            await CreateTruckEntity();
            var truckEntity = await CreateTruck("301", new VehicleCategory { Name = "Trailer", SortOrder = 2, AssetType = AssetType.Trailer, IsPowered = false });
            var driverEntity = await CreateDriver();
            var date = Clock.Now.Date;

            // Act, Assert
            await _driverAssignmentAppService.SetDriverForTruck(new SetDriverForTruckInput
            {
                TruckId = truckEntity.Id,
                DriverId = driverEntity.Id,
                Date = date,
            }).ShouldThrowAsync(typeof(ArgumentException));
        }

        [Fact]
        public async Task Test_SetNoDriverForTruck_should_throw_UserFriendlyException_past_dates()
        {
            // Arrange
            await CreateTruckEntity();
            var truckEntity = await CreateTruck("301");
            var driverEntity = await CreateDriver();
            var date = Clock.Now.Date.AddDays(-1);

            // Act, Assert
            await _driverAssignmentAppService.SetNoDriverForTruck(new SetNoDriverForTruckInput
            {
                TruckId = truckEntity.Id,
                StartDate = date,
                EndDate = date,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_SetNoDriverForTruck_should_create_DriverAssignment_with_DriverId_null()
        {
            // Arrange
            var truckEntity = await CreateTruckEntity();
            var date = Clock.Now.Date;

            // Act
            await _driverAssignmentAppService.SetNoDriverForTruck(new SetNoDriverForTruckInput
            {
                TruckId = truckEntity.Id,
                StartDate = date,
                EndDate = date,
            });

            // Assert
            var driverAssignments = await UsingDbContextAsync(async context =>
                await context.DriverAssignments.Where(da => da.IsDeleted == false && da.TruckId == truckEntity.Id).ToListAsync()
            );
            driverAssignments.Count().ShouldBe(1);
            var driverAssignment = driverAssignments.First();
            driverAssignment.DriverId.ShouldBeNull();
        }

        [Fact]
        public async Task Test_SetNoDriverForTruck_should_create_DriverAssignment_for_DateShift_with_DriverId_null()
        {
            // Arrange
            var truckEntity = await CreateTruckEntity();
            var date = Clock.Now.Date;

            // Act
            await _driverAssignmentAppService.SetNoDriverForTruck(new SetNoDriverForTruckInput
            {
                TruckId = truckEntity.Id,
                StartDate = date,
                EndDate = date,
                Shift = Shift.Shift1,
            });

            // Assert
            var driverAssignments = await UsingDbContextAsync(async context =>
                await context.DriverAssignments.Where(da => da.IsDeleted == false && da.TruckId == truckEntity.Id).ToListAsync()
            );
            driverAssignments.Count().ShouldBe(1);
            var driverAssignment = driverAssignments.First();
            driverAssignment.DriverId.ShouldBeNull();
            driverAssignment.Date.ShouldBe(date);
            driverAssignment.Shift.ShouldBe(Shift.Shift1);
        }

        [Fact]
        public async Task Test_SetNoDriverForTruck_should_create_DriverAssignment_with_DriverId_null_and_cancel_Unacknowledged_Dispatches()
        {
            // Arrange
            var truckEntity = await CreateTruckEntity();
            var date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine = order.OrderLines.First();
            var driver = await CreateDriver();
            var dispatch = await CreateDispatch(truckEntity.Id, driver.Id, orderLine.Id, DispatchStatus.Sent);
            var dispatch2 = await CreateDispatch(truckEntity.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);

            // Act
            await _driverAssignmentAppService.SetNoDriverForTruck(new SetNoDriverForTruckInput
            {
                TruckId = truckEntity.Id,
                StartDate = date,
                EndDate = date,
            });

            // Assert
            var driverAssignments = await UsingDbContextAsync(async context =>
                await context.DriverAssignments.Where(da => da.IsDeleted == false && da.TruckId == truckEntity.Id).ToListAsync()
            );
            driverAssignments.Count().ShouldBe(1);
            var driverAssignment = driverAssignments.First();
            driverAssignment.DriverId.ShouldBeNull();
            var updatedDispatch = await UsingDbContextAsync(async context => await context.Dispatches.Where(d => d.Id == dispatch.Id).FirstAsync());
            updatedDispatch.Status.ShouldBe(DispatchStatus.Canceled);
            var updatedDispatch2 = await UsingDbContextAsync(async context => await context.Dispatches.Where(d => d.Id == dispatch2.Id).FirstAsync());
            updatedDispatch2.Status.ShouldBe(DispatchStatus.Acknowledged);
        }

        [Fact]
        public async Task Test_SetNoDriverForTruck_should_remove_truck_from_orders()
        {
            // Arrange
            var date = Clock.Now.Date;
            var date2 = date.AddDays(1);
            var truckEntity = await CreateTruckEntity();
            var orderEntity = await CreateOrderAndAssignTruck(date, truckEntity.Id);
            var orderEntity2 = await CreateOrderAndAssignTruck(date2, truckEntity.Id);
            var orderLineTrucks = await GetOrderLineTrucks(truckEntity.Id);
            orderLineTrucks.Count.ShouldBe(2);

            // Act
            await _driverAssignmentAppService.SetNoDriverForTruck(new SetNoDriverForTruckInput
            {
                TruckId = truckEntity.Id,
                StartDate = date,
                EndDate = date,
            });

            // Assert
            var driverAssignments = await UsingDbContextAsync(async context =>
                await context.DriverAssignments.Where(da => da.IsDeleted == false && da.TruckId == truckEntity.Id).ToListAsync()
            );
            driverAssignments.Count().ShouldBe(1);
            var driverAssignment = driverAssignments.First();
            driverAssignment.DriverId.ShouldBeNull();
            orderLineTrucks = await GetOrderLineTrucks(truckEntity.Id);
            orderLineTrucks.Count.ShouldBe(1);
            orderLineTrucks.First().OrderLine.OrderId.ShouldBe(orderEntity2.Id);
        }

        [Theory]
        [InlineData(DispatchStatus.Created, true, false)]
        [InlineData(DispatchStatus.Sent, true, false)]
        [InlineData(DispatchStatus.Acknowledged, false, true)]
        [InlineData(DispatchStatus.Loaded, false, true)]
        [InlineData(DispatchStatus.Completed, false, false)]
        [InlineData(DispatchStatus.Canceled, false, false)]
        public async Task Test_ThereAreOpenDispatchesForTruckOnDate_should_return_result_when_there_is_dispatch(DispatchStatus status, bool unacknowledged, bool acknowledged)
        {
            // Arrange
            var truckEntity = await CreateTruckEntity();
            var date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine = order.OrderLines.First();
            var driver = await CreateDriver();
            var dispatch = await CreateDispatch(truckEntity.Id, driver.Id, orderLine.Id, status);

            // Act
            var result = await _driverAssignmentAppService.ThereAreOpenDispatchesForTruckOnDate(new ThereAreOpenDispatchesForTruckOnDateInput
            {
                TruckId = truckEntity.Id,
                StartDate = date,
                EndDate = date,
            });

            // Assert
            result.ThereAreUnacknowledgedDispatches.ShouldBe(unacknowledged);
            result.ThereAreAcknowledgedDispatches.ShouldBe(acknowledged);
        }

        [Fact]
        public async Task Test_ThereAreOpenDispatchesForTruckOnDate_should_return_result_when_there_is_no_dispatch()
        {
            // Arrange
            var truckEntity = await CreateTruckEntity();
            var date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine = order.OrderLines.First();
            var driver = await CreateDriver();

            // Act
            var result = await _driverAssignmentAppService.ThereAreOpenDispatchesForTruckOnDate(new ThereAreOpenDispatchesForTruckOnDateInput
            {
                TruckId = truckEntity.Id,
                StartDate = date,
                EndDate = date,
            });

            // Assert
            result.ThereAreUnacknowledgedDispatches.ShouldBe(false);
            result.ThereAreAcknowledgedDispatches.ShouldBe(false);
        }

        [Fact]
        public async Task Test_ThereAreOpenDispatchesForTruckOnDate_should_return_result_when_there_are_dispatches()
        {
            // Arrange
            var truckEntity = await CreateTruckEntity();
            var date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var orderLine = order.OrderLines.First();
            var driver = await CreateDriver();
            var dispatch1 = await CreateDispatch(truckEntity.Id, driver.Id, orderLine.Id, DispatchStatus.Created);
            var dispatch2 = await CreateDispatch(truckEntity.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);

            // Act
            var result = await _driverAssignmentAppService.ThereAreOpenDispatchesForTruckOnDate(new ThereAreOpenDispatchesForTruckOnDateInput
            {
                TruckId = truckEntity.Id,
                StartDate = date,
                EndDate = date,
            });

            // Assert
            result.ThereAreUnacknowledgedDispatches.ShouldBe(true);
            result.ThereAreAcknowledgedDispatches.ShouldBe(true);
        }

        [Fact]
        public async Task Test_SetDefaultDriverForTruck_should_update_DriverAssignments_with_DefaultDriver()
        {
            // Arrange
            var date = Clock.Now.Date;
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            truck = await UpdateEntity(truck, t => t.DefaultDriverId = driver.Id);
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, Shift.Shift1);

            // Act
            await _driverAssignmentAppService.SetDefaultDriverForTruck(new SetDefaultDriverForTruckInput
            {
                StartDate = date,
                EndDate = date,
                Shift = Shift.Shift1,
                TruckId = truck.Id,
            });

            // Assert
            var driverAssignments = await UsingDbContextAsync(async context => await context.DriverAssignments.Where(da => !da.IsDeleted).ToListAsync());
            driverAssignments.Count.ShouldBe(1);
            driverAssignments[0].DriverId.ShouldBe(driver.Id);
        }


        private async Task<List<OrderLineTruck>> GetOrderLineTrucks(int truckId)
        {
            return await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Include(olt => olt.OrderLine).Where(olt => olt.IsDeleted == false && olt.TruckId == truckId).ToListAsync()
            );
        }

        private async Task<Truck> CreateTruckEntity()
        {
            var truckEntity = await UsingDbContextAsync(async context =>
            {
                var vehicleCategory = await CreateVehicleCategory();
                // Create several trucks so the Id wouldn't be always equal to 1
                var truck = new Truck
                {
                    TenantId = 1,
                    TruckCode = "101",
                    OfficeId = _officeId,
                    IsActive = true,
                    AlwaysShowOnSchedule = true,
                    VehicleCategoryId = vehicleCategory.Id,
                };
                await context.Trucks.AddAsync(truck);
                truck = new Truck
                {
                    TenantId = 1,
                    TruckCode = "102",
                    OfficeId = _officeId,
                    IsActive = true,
                    AlwaysShowOnSchedule = true,
                    VehicleCategoryId = vehicleCategory.Id,
                };
                await context.Trucks.AddAsync(truck);

                truck = new Truck
                {
                    TenantId = 1,
                    TruckCode = "001",
                    OfficeId = _officeId,
                    IsActive = true,
                    AlwaysShowOnSchedule = true,
                    VehicleCategoryId = vehicleCategory.Id,
                };
                await context.Trucks.AddAsync(truck);

                return truck;
            });
            return truckEntity;
        }

        private async Task<Order> CreateOrderAndAssignTruck(DateTime orderDate, int truckId)
        {
            var orderEntity = await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var context = await UnitOfWorkManager.Current.GetDbContextAsync<DispatcherWebDbContext>();
                var order = new Order
                {
                    TenantId = 1,
                    DeliveryDate = orderDate,
                    Customer = new Customer() { TenantId = 1, Name = "Cust" },
                    Office = new Office() { TenantId = 1, Name = "Office1", TruckColor = "fff" },
                    SalesTaxRate = 2,
                };
                order.OrderLines.Add(new OrderLine
                {
                    TenantId = 1,
                    Designation = DesignationEnum.FreightAndMaterial,
                    FreightPricePerUnit = 2,
                    MaterialPricePerUnit = 3,
                    MaterialQuantity = 5,
                    FreightQuantity = 5,
                    FreightPrice = 2 * 5,
                    MaterialPrice = 3 * 5,
                    FreightItem = new Item
                    {
                        TenantId = 1,
                        Name = "sss",
                    },
                    MaterialUomId = 1,
                    FreightUomId = 1,
                });
                var orderLine = new OrderLine
                {
                    TenantId = 1,
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
                    },
                    MaterialUomId = 1,
                    FreightUomId = 1,
                };
                orderLine.OrderLineTrucks.Add(new OrderLineTruck
                {
                    TenantId = 1,
                    TruckId = truckId,
                    DriverId = null,
                    Utilization = 1,
                });
                order.OrderLines.Add(orderLine);
                await context.Orders.AddAsync(order);

                var user = await context.Users.FirstAsync(u => u.TenantId == 1);
                user.Office = order.Office;
                await context.SaveChangesAsync();

                return order;
            });
            return orderEntity;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
