using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trucks.Dto;
using DispatcherWeb.VehicleCategories;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Trucks
{
    public class TruckAppService_EditTruck_Tests : AppTestBase, IAsyncLifetime
    {
        private ITruckAppService _truckAppService;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _truckAppService = Resolve<ITruckAppService>();
        }


        [Fact]
        public async Task Test_EditTruck_should_set_DriverId_null_for_DriverAssignment_and_not_remove_OrderLineTruck_for_current_and_future_dates_when_DefaultDriver_is_removed()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driverId);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driverId, orderLineId, 1);

            // Act
            await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = null,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = truck.IsOutOfService,
                IsActive = truck.IsActive,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            var driverAssignments = await UsingDbContextAsync(async context =>
                await context.DriverAssignments.Where(da => !da.IsDeleted).ToListAsync());
            driverAssignments.Count.ShouldBe(1);
            driverAssignments[0].DriverId.ShouldBeNull();
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(da => !da.IsDeleted).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Test_EditTruck_should_not_change_DriverAssignment_and_OrderLineTruck_for_past_dates_when_DefaultDriver_is_removed()
        {
            // Assign
            var date = Clock.Now.Date.AddDays(-1);
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, driverId);
            var order = await CreateOrderWithOrderLines(date);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driverId, orderLineId, 1);

            // Act
            await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = null,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = truck.IsOutOfService,
                IsActive = truck.IsActive,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            var driverAssignments = await UsingDbContextAsync(async context =>
                await context.DriverAssignments.Where(da => !da.IsDeleted).ToListAsync());
            driverAssignments.Count.ShouldBe(1);
            driverAssignments[0].DriverId.ShouldBe(driverId);
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(da => !da.IsDeleted).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Test_EditTruck_should_not_remove_OrderLineTruck_for_current_and_future_dates_when_DefaultDriver_is_removed_and_there_is_another_DriverAssignment()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            var driver2 = await CreateDriver();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driver2.Id);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver2.Id, orderLineId, 1);

            // Act
            await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = null,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = truck.IsOutOfService,
                IsActive = truck.IsActive,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            var driverAssignments = await UsingDbContextAsync(async context =>
                await context.DriverAssignments.Where(da => !da.IsDeleted).ToListAsync());
            driverAssignments.Count.ShouldBe(1);
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(da => !da.IsDeleted).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Test_EditTruck_should_not_remove_DriverAssignment_and_OrderLineTruck_for_past_dates_when_DefaultDriver_is_removed()
        {
            // Assign
            var pastDate = Clock.Now.Date.AddDays(-1);
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, pastDate, driverId);
            var order = await CreateOrderWithOrderLines(pastDate);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driverId, orderLineId, 1);

            // Act
            await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = null,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = truck.IsOutOfService,
                IsActive = truck.IsActive,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            var driverAssignments = await UsingDbContextAsync(async context =>
                await context.DriverAssignments.Where(da => !da.IsDeleted).ToListAsync());
            driverAssignments.Count.ShouldBe(1);
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(da => !da.IsDeleted).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditTruck_should_remove_OrderLineTruck_when_set_IsOutOfService()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driverId);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driverId, orderLineId, 1);

            // Act
            await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = driverId,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = true,
                Reason = "unit test",
                IsActive = truck.IsActive,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            truck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            truck.IsOutOfService.ShouldBeTrue();
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(da => !da.IsDeleted).ToListAsync());
            orderLineTrucks.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_EditTruck_should_remove_DriverAssignments_and_OrderLineTruck_when_set_IsActive_false()
        {
            // Assign
            var today = Clock.Now.Date;
            var tomorrow = today.AddDays(1);
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driverId);
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, tomorrow, driverId);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driverId, orderLineId, 1);
            var order2 = await CreateOrderWithOrderLines(tomorrow);
            var orderLineId2 = order2.OrderLines.First().Id;
            var orderLineTruck2 = await CreateOrderLineTruck(truck.Id, driverId, orderLineId2, 1);

            // Act
            await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = driverId,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = truck.IsOutOfService,
                IsActive = false,
                InactivationDate = tomorrow,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            truck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            truck.IsActive.ShouldBeFalse();
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => !olt.IsDeleted).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
            orderLineTrucks[0].Id.ShouldBe(orderLineTruck.Id);
            var driverAssignments = await UsingDbContextAsync(async context => await context.DriverAssignments.Where(da => !da.IsDeleted).ToListAsync());
            driverAssignments.Count.ShouldBe(1);
            driverAssignments[0].Id.ShouldBe(driverAssignment.Id);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditTruck_should_remove_OrderLineTruck_when_Office_is_changed()
        {
            // Assign
            var today = Clock.Now.Date;
            var office2 = await CreateOffice();
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driverId);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driverId, orderLineId, 1);

            // Act
            await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = driverId,
                TruckCode = truck.TruckCode,
                OfficeId = office2.Id,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = truck.IsOutOfService,
                IsActive = truck.IsActive,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            truck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            truck.OfficeId.ShouldBe(office2.Id);
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(da => !da.IsDeleted).ToListAsync());
            orderLineTrucks.Count.ShouldBe(0);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditTruck_should_remove_OrderLineTruck_when_set_IsActive_false()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driverId);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driverId, orderLineId, 1);

            // Act
            await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = driverId,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = truck.IsOutOfService,
                IsActive = false,
                InactivationDate = today,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            truck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            truck.IsActive.ShouldBeFalse();
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(da => !da.IsDeleted).ToListAsync());
            orderLineTrucks.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_EditTruck_should_throw_Exception_when_IsActive_false_and_InactivationDate_is_null()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();

            // Act, Assert
            await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = truck.IsOutOfService,
                IsActive = false,
                InactivationDate = null,
                MobileDevices = new List<MobileDeviceEditDto>(),
            }).ShouldThrowAsync(typeof(ArgumentException));
        }

        [Fact]
        public async Task Test_EditTruck_should_set_InactivationDate_null_when_IsActive_true()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();

            // Act, Assert
            await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = truck.IsOutOfService,
                IsActive = true,
                InactivationDate = today,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            var savedTruck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            savedTruck.InactivationDate.ShouldBeNull();
        }

        [Fact]
        public async Task Test_EditTruck_should_cancel_Unacknowledged_dispatches_when_set_IsActive_false()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driverId);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driverId, orderLineId, 1);
            var dispatch = await CreateDispatch(truck.Id, driverId, orderLineId, DispatchStatus.Created);
            var dispatch2 = await CreateDispatch(truck.Id, driverId, orderLineId, DispatchStatus.Sent);

            // Act
            var result = await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = driverId,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = truck.IsOutOfService,
                IsActive = false,
                InactivationDate = today,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            result.Id.ShouldBe(truck.Id);
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
        public async Task Test_EditTruck_should_not_cancel_Acknowledged_or_Loaded_dispatches_when_set_IsActive_false()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driverId);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driverId, orderLineId, 1);
            var dispatch = await CreateDispatch(truck.Id, driverId, orderLineId, DispatchStatus.Acknowledged);
            var dispatch2 = await CreateDispatch(truck.Id, driverId, orderLineId, DispatchStatus.Loaded);

            // Act
            var result = await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = driverId,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = truck.IsOutOfService,
                IsActive = false,
                InactivationDate = today,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            result.Id.ShouldBe(truck.Id);
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
        public async Task Test_EditTruck_should_not_check_Closed_dispatches_when_set_IsActive_false()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driverId);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driverId, orderLineId, 1);
            var dispatch = await CreateDispatch(truck.Id, driverId, orderLineId, DispatchStatus.Completed);
            var dispatch2 = await CreateDispatch(truck.Id, driverId, orderLineId, DispatchStatus.Canceled);

            // Act
            var result = await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = driverId,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = truck.IsOutOfService,
                IsActive = false,
                InactivationDate = today,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            result.Id.ShouldBe(truck.Id);
            result.ThereWereCanceledDispatches.ShouldBeFalse();
            result.ThereWereNotCanceledDispatches.ShouldBeFalse();
            var updatedDispatch = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch.Id));
            updatedDispatch.IsDeleted.ShouldBeFalse();
            updatedDispatch.Status.ShouldBe(DispatchStatus.Completed);
            var updatedDispatch2 = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch2.Id));
            updatedDispatch2.IsDeleted.ShouldBeFalse();
            updatedDispatch2.Status.ShouldBe(DispatchStatus.Canceled);
        }

        [Fact]
        public async Task Test_EditTruck_should_cancel_Unacknowledged_dispatches_when_set_IsOutOfService_true()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driverId);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driverId, orderLineId, 1);
            var dispatch = await CreateDispatch(truck.Id, driverId, orderLineId, DispatchStatus.Created);
            var dispatch2 = await CreateDispatch(truck.Id, driverId, orderLineId, DispatchStatus.Sent);

            // Act
            var result = await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = driverId,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = true,
                IsActive = true,
                InactivationDate = today,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            result.Id.ShouldBe(truck.Id);
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
        public async Task Test_EditTruck_should_not_cancel_Acknowledged_or_Loaded_dispatches_when_set_IsOutOfService_true()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driverId);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driverId, orderLineId, 1);
            var dispatch = await CreateDispatch(truck.Id, driverId, orderLineId, DispatchStatus.Acknowledged);
            var dispatch2 = await CreateDispatch(truck.Id, driverId, orderLineId, DispatchStatus.Loaded);

            // Act
            var result = await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = driverId,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = true,
                IsActive = true,
                InactivationDate = today,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            result.Id.ShouldBe(truck.Id);
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
        public async Task Test_EditTruck_should_not_check_Closed_dispatches_when_set_IsOutOfService_true()
        {
            // Assign
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driverId = (await SetDefaultDriverForTruck(truck.Id)).Id;
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driverId);
            var order = await CreateOrderWithOrderLines(today);
            var orderLineId = order.OrderLines.First().Id;
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driverId, orderLineId, 1);
            var dispatch = await CreateDispatch(truck.Id, driverId, orderLineId, DispatchStatus.Completed);
            var dispatch2 = await CreateDispatch(truck.Id, driverId, orderLineId, DispatchStatus.Canceled);

            // Act
            var result = await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = driverId,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsOutOfService = true,
                IsActive = true,
                InactivationDate = today,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            result.Id.ShouldBe(truck.Id);
            result.ThereWereCanceledDispatches.ShouldBeFalse();
            result.ThereWereNotCanceledDispatches.ShouldBeFalse();
            var updatedDispatch = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch.Id));
            updatedDispatch.IsDeleted.ShouldBeFalse();
            updatedDispatch.Status.ShouldBe(DispatchStatus.Completed);
            var updatedDispatch2 = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatch2.Id));
            updatedDispatch2.IsDeleted.ShouldBeFalse();
            updatedDispatch2.Status.ShouldBe(DispatchStatus.Canceled);
        }

        [Fact]
        public async Task Test_EditTruck_should_throw_Exception_when_Truck_is_Trailer_and_DefaultDriverId_is_not_null()
        {
            // Arrange
            var truck = await CreateTruck(vehicleCategory: new VehicleCategory { Name = "Trailer", SortOrder = 2, AssetType = AssetType.Trailer, IsPowered = false });
            var driver = await CreateDriver();

            // Act, Assert
            await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = driver.Id,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsActive = true,
                MobileDevices = new List<MobileDeviceEditDto>(),
            }).ShouldThrowAsync(typeof(ArgumentException));
        }

        [Theory]
        [InlineData(AssetType.DumpTruck)]
        [InlineData(AssetType.Tractor)]
        public async Task Test_EditTruck_should_not_throw_Exception_when_Truck_is_DumpTruck_or_Tractor_and_DefaultDriverId_is_not_null(AssetType assetType)
        {
            // Arrange
            var truck = await CreateTruck(vehicleCategory: new VehicleCategory { Name = assetType.ToString(), SortOrder = 1, AssetType = assetType, IsPowered = true });
            var driver = await CreateDriver();

            // Act
            await _truckAppService.EditTruck(new TruckEditDto
            {
                Id = truck.Id,
                DefaultDriverId = driver.Id,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsActive = true,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            var updatedTruck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            updatedTruck.DefaultDriverId.ShouldBe(driver.Id);
        }

        [Fact]
        public async Task Test_EditTruck_should_throw_UserFriendlyException_when_creating_truck_with_existing_TruckCode()
        {
            // Arrange
            var truck = await CreateTruck("101", officeId: _officeId);
            var dumpTrucks = await CreateVehicleCategory();

            // Act, Assert
            await _truckAppService.EditTruck(new TruckEditDto
            {
                TruckCode = "101",
                OfficeId = _officeId,
                VehicleCategoryId = dumpTrucks.Id,
                IsActive = true,
                MobileDevices = new List<MobileDeviceEditDto>(),
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_EditTruck_should_create_truck_with_existing_TruckCode_in_another_office()
        {
            // Arrange
            var truck = await CreateTruck("101");
            var office = await CreateOffice();
            var dumpTrucks = await CreateVehicleCategory();

            // Act
            var result = await _truckAppService.EditTruck(new TruckEditDto
            {
                TruckCode = "101",
                OfficeId = office.Id,
                VehicleCategoryId = dumpTrucks.Id,
                IsActive = true,
                MobileDevices = new List<MobileDeviceEditDto>(),
            });

            // Assert
            var createdTruck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(result.Id));
            createdTruck.Id.ShouldNotBe(truck.Id);
            createdTruck.TruckCode.ShouldBe("101");
            createdTruck.OfficeId.ShouldBe(office.Id);
        }

        [Fact]
        public async Task Test_EditTruck_should_set_DefaultTrailer_for_tractor()
        {
            // Arrange
            var truck = await CreateTruck("401", await CreateTractorVehicleCategory(), canPullTrailer: true);
            var trailer = await CreateTruck("501", await CreateTrailerVehicleCategory());

            // Act
            var result = await _truckAppService.EditTruck(CreateTruckEditDtoFromTruck(truck, t => t.CurrentTrailerId = trailer.Id));

            // Assert
            var createdTruck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(result.Id));
            createdTruck.CurrentTrailerId.ShouldBe(trailer.Id);
        }

        [Fact]
        public async Task Test_EditTruck_should_set_DefaultTrailer_null()
        {
            // Arrange
            var truck = await CreateTruck("401", await CreateTractorVehicleCategory(), canPullTrailer: true);
            var trailer = await CreateTruck("501", await CreateTrailerVehicleCategory());
            await UpdateEntity(truck, t => t.CurrentTrailerId = trailer.Id);

            // Act
            var result = await _truckAppService.EditTruck(CreateTruckEditDtoFromTruck(truck, t => t.CurrentTrailerId = null));

            // Assert
            var updatedTruck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(result.Id));
            updatedTruck.CurrentTrailerId.ShouldBeNull();
        }

        [Fact]
        public async Task Test_EditTruck_should_throw_ArgumentException_when_Truck_is_not_Tractor_and_DefaultTrailer_is_not_null()
        {
            // Arrange
            var truck = await CreateTruck("101");
            var trailer = await CreateTruck("501", await CreateTrailerVehicleCategory());

            // Act, Assert
            var result = await _truckAppService.EditTruck(CreateTruckEditDtoFromTruck(truck, t => t.CurrentTrailerId = trailer.Id)).ShouldThrowAsync(typeof(ArgumentException));
        }

        [Fact]
        public async Task Test_EditTruck_should_throw_ArgumentException_when_Truck_is_Tractor_but_DefaultTrailer_is_not_Trailer()
        {
            // Arrange
            var truck = await CreateTruck("401", await CreateTractorVehicleCategory(), canPullTrailer: true);
            var trailer = await CreateTruck("101");

            // Act, Assert
            var result = await _truckAppService.EditTruck(CreateTruckEditDtoFromTruck(truck, t => t.CurrentTrailerId = trailer.Id)).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_EditTruck_should_throw_ArgumentException_when_Truck_is_Tractor_and_DefaultTrailer_is_not_active_Trailer()
        {
            // Arrange
            var truck = await CreateTruck("401", await CreateTractorVehicleCategory(), canPullTrailer: true);
            var trailer = await CreateTruck("501", await CreateTrailerVehicleCategory());
            await UpdateEntity(trailer, t => t.IsActive = false);

            // Act, Assert
            var result = await _truckAppService.EditTruck(CreateTruckEditDtoFromTruck(truck, t => t.CurrentTrailerId = trailer.Id)).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_EditTruck_should_remove_Trailer_from_another_Tractor_when_Truck_is_Tractor_and_DefaultTrailer_is_Trailer_is_associated_with_another_Tractor()
        {
            // Arrange
            var tractors = await CreateTractorVehicleCategory();
            var trailers = await CreateTrailerVehicleCategory();
            var truck = await CreateTruck("401", tractors, canPullTrailer: true);
            var truck2 = await CreateTruck("402", tractors, canPullTrailer: true);
            var trailer = await CreateTruck("501", trailers);
            await UpdateEntity(truck2, t => t.CurrentTrailerId = trailer.Id);

            // Act
            var result = await _truckAppService.EditTruck(CreateTruckEditDtoFromTruck(truck, t => t.CurrentTrailerId = trailer.Id));

            // Assert
            var updatedTruck = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck.Id));
            updatedTruck.CurrentTrailerId.ShouldBe(trailer.Id);
            var updatedTruck2 = await UsingDbContextAsync(async context => await context.Trucks.FindAsync(truck2.Id));
            updatedTruck2.CurrentTrailerId.ShouldBeNull();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditTruck_should_throw_UserFriendlyException_when_Trailer_is_associated_with_Tractor_and_IsActive_set_false()
        {
            // Arrange
            var truck = await CreateTruck("401", await CreateTractorVehicleCategory(), canPullTrailer: true);
            var trailer = await CreateTruck("501", await CreateTrailerVehicleCategory());
            await UpdateEntity(truck, t => t.CurrentTrailerId = trailer.Id);

            // Act, Assert
            var result = await _truckAppService.EditTruck(CreateTruckEditDtoFromTruck(trailer, t =>
            {
                t.IsActive = false;
                t.InactivationDate = DateTime.Today;
            })).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_EditTruck_should_throw_UserFriendlyException_when_Trailer_is_associated_with_Tractor_and_Category_is_changed()
        {
            // Arrange
            var truck = await CreateTruck("401", await CreateTractorVehicleCategory(), canPullTrailer: true);
            var trailer = await CreateTruck("501", await CreateTrailerVehicleCategory());
            var dumpTrucks = await CreateVehicleCategory();
            await UpdateEntity(truck, t => t.CurrentTrailerId = trailer.Id);

            // Act, Assert
            var result = await _truckAppService.EditTruck(CreateTruckEditDtoFromTruck(trailer, t => t.VehicleCategoryId = dumpTrucks.Id))
                .ShouldThrowAsync(typeof(UserFriendlyException));
        }


        private TruckEditDto CreateTruckEditDtoFromTruck(Truck truck, Action<TruckEditDto> action = null)
        {
            var truckEditDto = new TruckEditDto
            {
                Id = truck.Id,
                TruckCode = truck.TruckCode,
                OfficeId = truck.OfficeId ?? 0,
                VehicleCategoryId = truck.VehicleCategoryId,
                IsActive = truck.IsActive,
                CanPullTrailer = truck.CanPullTrailer,
                MobileDevices = truck.MobileDevices
                    .Select(md => new MobileDeviceEditDto
                    {
                        Id = md.Id,
                        DeviceType = md.DeviceType,
                        Imei = md.Imei,
                        Make = md.Make,
                        Model = md.Model,
                        SimId = md.SimId,
                    }).ToList(),
            };
            action?.Invoke(truckEditDto);

            return truckEditDto;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

    }
}
