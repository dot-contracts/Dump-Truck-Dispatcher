using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using DispatcherWeb.Customers;
using DispatcherWeb.Drivers;
using DispatcherWeb.Items;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trucks.Dto;
using DispatcherWeb.VehicleMaintenance;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Trucks
{
    public class TruckAppService_Tests : AppTestBase
    {
        private readonly ITruckAppService _truckAppService;

        public TruckAppService_Tests()
        {
            _truckAppService = Resolve<ITruckAppService>();
            SubstituteServiceDependencies(_truckAppService);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SetTruckIsOutOfService_should_delete_truck_from_OrderLines_for_the_date()
        {
            var truckEntity = await CreateTruckEntity();
            var today = DateTime.Today;
            await CreateOrder(truckEntity, today);

            await _truckAppService.SetTruckIsOutOfService(new SetTruckIsOutOfServiceInput
            {
                Date = today,
                IsOutOfService = true,
                Reason = "Unit Test",
                TruckId = truckEntity.Id,
            });

            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(ol => ol.IsDeleted == false).ToListAsync()
            );
            orderLineTrucks.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_SetTruckIsOutOfService_should_not_delete_truck_from_OrderLines_for_past_date()
        {
            var truckEntity = await CreateTruckEntity();
            var today = DateTime.Today;
            await CreateOrder(truckEntity, today.AddDays(-1));

            await _truckAppService.SetTruckIsOutOfService(new SetTruckIsOutOfServiceInput
            {
                Date = today,
                IsOutOfService = true,
                Reason = "Unit Test",
                TruckId = truckEntity.Id,
            });

            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(ol => ol.IsDeleted == false).ToListAsync()
            );
            orderLineTrucks.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Test_SetTruckIsOutOfService_should_cancel_Unacknowledged_Dispatches()
        {
            var truck = await CreateTruckEntity();
            var today = DateTime.Today;
            var order = await CreateOrder(truck, today);
            var orderLine = order.OrderLines.First();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Created);
            var dispatch2 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Sent);

            // Act
            var result = await _truckAppService.SetTruckIsOutOfService(new SetTruckIsOutOfServiceInput
            {
                Date = today,
                IsOutOfService = true,
                Reason = "Unit Test",
                TruckId = truck.Id,
            });

            // Assert
            result.ThereWereCanceledDispatches.ShouldBeTrue();
            result.ThereWereNotCanceledDispatches.ShouldBeFalse();
            var updatedDispatch = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch.Id));
            updatedDispatch.IsDeleted.ShouldBeFalse();
            updatedDispatch.Status.ShouldBe(DispatchStatus.Canceled);
            var updatedDispatch2 = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch2.Id));
            updatedDispatch2.IsDeleted.ShouldBeFalse();
            updatedDispatch2.Status.ShouldBe(DispatchStatus.Canceled);
        }

        [Fact]
        public async Task Test_SetTruckIsOutOfService_should_not_cancel_Acknowledged_or_Loaded_Dispatches()
        {
            var truck = await CreateTruckEntity();
            var today = DateTime.Today;
            var order = await CreateOrder(truck, today);
            var orderLine = order.OrderLines.First();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);
            var dispatch2 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Loaded);

            // Act
            var result = await _truckAppService.SetTruckIsOutOfService(new SetTruckIsOutOfServiceInput
            {
                Date = today,
                IsOutOfService = true,
                Reason = "Unit Test",
                TruckId = truck.Id,
            });

            // Assert
            result.ThereWereCanceledDispatches.ShouldBeFalse();
            result.ThereWereNotCanceledDispatches.ShouldBeTrue();
            var updatedDispatch = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch.Id));
            updatedDispatch.IsDeleted.ShouldBeFalse();
            updatedDispatch.Status.ShouldBe(DispatchStatus.Acknowledged);
            var updatedDispatch2 = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch2.Id));
            updatedDispatch2.IsDeleted.ShouldBeFalse();
            updatedDispatch2.Status.ShouldBe(DispatchStatus.Loaded);
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_true_when_there_are_no_dependencies()
        {
            var truckEntity = await CreateTruckEntity();

            var canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeTrue();
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_false_when_Order_exists()
        {
            var truckEntity = await CreateTruckEntity();

            await CreateOrder(truckEntity);

            var canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeFalse();
        }

        private async Task<Order> CreateOrder(Truck truckEntity, DateTime? orderDate = null)
        {
            orderDate ??= DateTime.Today;

            return await UsingDbContextAsync(async context =>
            {
                var order = new Order
                {
                    TenantId = 1,
                    Customer = new Customer() { TenantId = 1, Name = "Cust" },
                    OfficeId = truckEntity.Office.Id,
                    DeliveryDate = orderDate.Value,
                };
                var orderLine = new OrderLine
                {
                    TenantId = 1,
                    FreightItem = new Item
                    {
                        TenantId = 1,
                        Name = "sss",
                    },
                    MaterialUomId = 1,
                };
                order.OrderLines.Add(orderLine);
                orderLine = new OrderLine
                {
                    TenantId = 1,
                    FreightItem = new Item
                    {
                        TenantId = 1,
                        Name = "sss",
                    },
                    MaterialUomId = 1,
                };
                order.OrderLines.Add(orderLine);
                orderLine.OrderLineTrucks.Add(new OrderLineTruck
                {
                    TenantId = 1,
                    TruckId = truckEntity.Id,
                    DriverId = truckEntity.DefaultDriverId,
                });

                await context.Orders.AddAsync(order);
                return order;
            });
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_false_when_Ticket_exists()
        {
            var truckEntity = await CreateTruckEntity();

            await CreateTicket(truckEntity);

            var canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeFalse();
        }

        private async Task CreateTicket(Truck truckEntity)
        {
            await UsingDbContextAsync(async context =>
            {
                var ticket = new Ticket
                {
                    TenantId = 1,
                    TruckId = truckEntity.Id,
                };
                await context.Tickets.AddAsync(ticket);
            });
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_true_when_Ticket_exists_and_Carrier_is_not_null()
        {
            var truckEntity = await CreateTruckEntity();

            await CreateTicketWithCarrier(truckEntity);

            var canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeTrue();
        }

        private async Task CreateTicketWithCarrier(Truck truckEntity)
        {
            await UsingDbContextAsync(async context =>
            {
                var ticket = new Ticket
                {
                    TenantId = 1,
                    TruckId = truckEntity.Id,
                    Carrier = new LeaseHauler() { TenantId = 1, Name = "Carrier" },
                };
                await context.Tickets.AddAsync(ticket);
            });
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_false_when_PreventiveMaintenance_exists()
        {
            var truckEntity = await CreateTruckEntity();

            await CreatePreventiveMaintenance(truckEntity);

            var canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeFalse();
        }

        private async Task CreatePreventiveMaintenance(Truck truckEntity)
        {
            await UsingDbContextAsync(async context =>
            {
                var preventiveMaintenance = new PreventiveMaintenance
                {
                    TenantId = 1,
                    TruckId = truckEntity.Id,
                    VehicleService = new VehicleService
                    {
                        TenantId = 1,
                        Name = "VS1",
                    },
                };
                await context.PreventiveMaintenance.AddAsync(preventiveMaintenance);
            });
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_false_when_DriverAssignment_exists()
        {
            var truckEntity = await CreateTruckEntity();

            await CreateDriverAssignment(truckEntity);

            var canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeFalse();
        }

        private async Task CreateDriverAssignment(Truck truckEntity)
        {
            await UsingDbContextAsync(async context =>
            {
                var driverAssignment = new DriverAssignment
                {
                    TenantId = 1,
                    TruckId = truckEntity.Id,
                    OfficeId = truckEntity.OfficeId,
                };
                await context.DriverAssignments.AddAsync(driverAssignment);
            });
        }

        [Fact]
        public async Task Test_CanDeleteTruck_should_return_false_when_WorkOrder_exists()
        {
            var truckEntity = await CreateTruckEntity();

            await CreateWorkOrder(truckEntity);

            var canDelete = await _truckAppService.CanDeleteTruck(new EntityDto(truckEntity.Id));

            canDelete.ShouldBeFalse();
        }

        private async Task CreateWorkOrder(Truck truckEntity)
        {
            await UsingDbContextAsync(async context =>
            {
                var workOrder = new WorkOrder
                {
                    TenantId = 1,
                    TruckId = truckEntity.Id,
                };
                await context.WorkOrders.AddAsync(workOrder);
            });
        }

        private async Task<Truck> CreateTruckEntity()
        {
            var truckEntity = await UsingDbContextAsync(async context =>
            {
                var office = new Office() { TenantId = 1, Name = "Office1", TruckColor = "fff" };
                // Create several trucks so the Id wouldn't be always equal to 1
                var vehicleCategory = await CreateVehicleCategory();
                var truck = new Truck
                {
                    TenantId = 1,
                    TruckCode = "101",
                    Office = office,
                    IsActive = true,
                    AlwaysShowOnSchedule = true,
                    VehicleCategoryId = vehicleCategory.Id,
                };
                await context.Trucks.AddAsync(truck);
                truck = new Truck
                {
                    TenantId = 1,
                    TruckCode = "102",
                    Office = office,
                    IsActive = true,
                    AlwaysShowOnSchedule = true,
                    VehicleCategoryId = vehicleCategory.Id,
                };
                await context.Trucks.AddAsync(truck);

                truck = new Truck
                {
                    TenantId = 1,
                    TruckCode = "001",
                    Office = office,
                    IsActive = true,
                    AlwaysShowOnSchedule = true,
                    VehicleCategoryId = vehicleCategory.Id,
                };
                await context.Trucks.AddAsync(truck);

                return truck;
            });
            return truckEntity;
        }

    }
}
