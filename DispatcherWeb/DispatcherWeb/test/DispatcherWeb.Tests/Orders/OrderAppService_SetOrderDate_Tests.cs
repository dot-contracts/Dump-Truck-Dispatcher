using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Customers;
using DispatcherWeb.Items;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Orders
{
    public class OrderAppService_SetOrderDate_Tests : AppTestBase
    {
        private readonly IOrderAppService _orderAppService;

        public OrderAppService_SetOrderDate_Tests()
        {
            _orderAppService = Resolve<IOrderAppService>();
            SubstituteServiceDependencies(_orderAppService);

        }

        [Fact]
        public async Task Test_SetOrderDate_should_update_DateTime_when_change_date_for_entire_order()
        {
            var orderEntity = await CreateOrder();
            var newDate = Clock.Now.Date.AddDays(1);

            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                OrderId = orderEntity.Id,
                Date = newDate,
            });

            result.Completed.ShouldBeTrue();
            var orderEntityResult = await UsingDbContextAsync(async context =>
            {
                return await context.Orders
                    .Include(o => o.OrderLines)
                    .FirstAsync(o => o.Id == orderEntity.Id);
            });
            orderEntityResult.Id.ShouldBe(orderEntity.Id);
            orderEntityResult.DeliveryDate.ShouldNotBe(orderEntity.DeliveryDate);
            orderEntityResult.DeliveryDate.ShouldBe(newDate);
            orderEntityResult.Shift.ShouldBeNull();
        }

        [Fact]
        public async Task Test_SetOrderDate_should_update_DateTime_and_Shift_when_change_date_for_entire_order()
        {
            var orderEntity = await CreateOrder();
            orderEntity = await UpdateEntity(orderEntity, o => o.Shift = Shift.Shift1);
            SubstituteSetting(AppSettings.General.UseShifts, "true");

            var newDate = Clock.Now.Date.AddDays(1);

            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                OrderId = orderEntity.Id,
                Date = newDate,
                Shift = Shift.Shift3,
            });

            result.Completed.ShouldBeTrue();
            var orderEntityResult = await UsingDbContextAsync(async context =>
            {
                return await context.Orders
                    .Include(o => o.OrderLines)
                    .FirstAsync(o => o.Id == orderEntity.Id);
            });
            orderEntityResult.Id.ShouldBe(orderEntity.Id);
            orderEntityResult.DeliveryDate.ShouldNotBe(orderEntity.DeliveryDate);
            orderEntityResult.DeliveryDate.ShouldBe(newDate);
            orderEntityResult.Shift.ShouldBe(Shift.Shift3);
        }

        [Fact]
        public async Task Test_SetOrderDate_should_copy_Order_and_move_OrderLine_when_update_date_for_only_one_OrderLine()
        {
            // Arrange
            var orderEntity = await CreateOrder();
            orderEntity.OrderLines.Count.ShouldBe(2);
            var newDate = Clock.Now.Date.AddDays(1);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                OrderId = orderEntity.Id,
                OrderLineId = orderEntity.OrderLines.First().Id,
                Date = newDate,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            var orderEntityResult = await UsingDbContextAsync(async context =>
            {
                return await context.Orders
                    .Include(o => o.OrderLines)
                    .FirstOrDefaultAsync(o => o.Id != orderEntity.Id);
            });
            orderEntityResult.ShouldNotBeNull();
            orderEntityResult.Id.ShouldNotBe(orderEntity.Id);
            orderEntityResult.DeliveryDate.ShouldNotBe(orderEntity.DeliveryDate);
            orderEntityResult.DeliveryDate.ShouldBe(newDate);
            orderEntityResult.Shift.ShouldBeNull();
            orderEntityResult.OrderLines.Count.ShouldBe(1);
            orderEntityResult.OrderLines.First().Id.ShouldBe(orderEntity.OrderLines.First().Id);

            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.OrderLines.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Test_SetOrderDate_should_copy_Order_and_update_Shift_and_move_OrderLine_when_update_date_for_only_one_OrderLine()
        {
            // Arrange
            var orderEntity = await CreateOrder();
            orderEntity.OrderLines.Count.ShouldBe(2);
            orderEntity = await UpdateEntity(orderEntity, o => o.Shift = Shift.Shift1);
            SubstituteSetting(AppSettings.General.UseShifts, "true");
            var newDate = Clock.Now.Date.AddDays(1);
            var newShift = Shift.Shift3;

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                OrderId = orderEntity.Id,
                OrderLineId = orderEntity.OrderLines.First().Id,
                Date = newDate,
                Shift = newShift,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            var orderEntityResult = await UsingDbContextAsync(async context =>
            {
                return await context.Orders
                    .Include(o => o.OrderLines)
                    .FirstOrDefaultAsync(o => o.Id != orderEntity.Id);
            });
            orderEntityResult.ShouldNotBeNull();
            orderEntityResult.Id.ShouldNotBe(orderEntity.Id);
            orderEntityResult.DeliveryDate.ShouldNotBe(orderEntity.DeliveryDate);
            orderEntityResult.DeliveryDate.ShouldBe(newDate);
            orderEntityResult.Shift.ShouldBe(newShift);
            orderEntityResult.OrderLines.Count.ShouldBe(1);
            orderEntityResult.OrderLines.First().Id.ShouldBe(orderEntity.OrderLines.First().Id);

            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.OrderLines.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_entire_Order_should_change_Date_when_trucks_are_available_for_Date()
        {
            // Arrange
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            var anotherDate = Clock.Now.Date.AddDays(1);
            var anotherOrderEntity = await CreateOrder(anotherDate);
            var anotherOrderLine = anotherOrderEntity.OrderLines.First();
            var anotherTruck = await CreateTruck("102");
            var driver2 = await CreateDriver();
            await CreateOrderLineTruck(anotherTruck.Id, driver2.Id, anotherOrderLine.Id, 1);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = null,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.NotAvailableTrucks.ShouldBeNull();
            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.DeliveryDate.ShouldBe(anotherDate);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_entire_Order_should_change_Shift_when_trucks_are_available_for_Date_and_Shift()
        {
            // Arrange
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            orderEntity = await UpdateEntity(orderEntity, o => o.Shift = Shift.Shift1);
            SubstituteSetting(AppSettings.General.UseShifts, "true");
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            var anotherOrderEntity = await CreateOrder(date);
            anotherOrderEntity = await UpdateEntity(anotherOrderEntity, o => o.Shift = Shift.Shift2);
            var anotherOrderLine = anotherOrderEntity.OrderLines.First();
            var anotherTruck = await CreateTruck("102");
            var driver2 = await CreateDriver();
            await CreateOrderLineTruck(anotherTruck.Id, driver2.Id, anotherOrderLine.Id, 1);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = date,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = null,
                Shift = Shift.Shift2,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.NotAvailableTrucks.ShouldBeNull();
            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.DeliveryDate.ShouldBe(date);
            orderEntityOriginal.Shift.ShouldBe(Shift.Shift2);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_entire_Order_should_return_NotAvailableTrucks_when_trucks_are_not_available_for_Date()
        {
            // Arrange
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            var anotherDate = Clock.Now.Date.AddDays(1);
            var anotherOrderEntity = await CreateOrder(anotherDate);
            var anotherOrderLine = anotherOrderEntity.OrderLines.First();
            await CreateOrderLineTruck(truck.Id, driver.Id, anotherOrderLine.Id, 1);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = null,
            });

            // Assert
            result.Completed.ShouldBeFalse();
            result.NotAvailableTrucks.ShouldNotBeNull();
            result.NotAvailableTrucks.Count.ShouldBe(1);
            result.NotAvailableTrucks[0].ShouldBe(truck.TruckCode);
            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.DeliveryDate.ShouldBe(date);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_entire_Order_should_return_NotAvailableTrucks_when_trucks_are_not_available_for_Date_and_Shift()
        {
            // Arrange
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            orderEntity = await UpdateEntity(orderEntity, o => o.Shift = Shift.Shift1);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            var anotherDate = Clock.Now.Date.AddDays(1);
            var anotherOrderEntity = await CreateOrder(anotherDate);
            anotherOrderEntity = await UpdateEntity(anotherOrderEntity, o => o.Shift = Shift.Shift3);
            var anotherOrderLine = anotherOrderEntity.OrderLines.First();
            await CreateOrderLineTruck(truck.Id, driver.Id, anotherOrderLine.Id, 1);
            SubstituteSetting(AppSettings.General.UseShifts, "true");

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = null,
                Shift = Shift.Shift3,
            });

            // Assert
            result.Completed.ShouldBeFalse();
            result.NotAvailableTrucks.ShouldNotBeNull();
            result.NotAvailableTrucks.Count.ShouldBe(1);
            result.NotAvailableTrucks[0].ShouldBe(truck.TruckCode);
            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.DeliveryDate.ShouldBe(date);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_entire_Order_should_return_NotAvailableTrucks_when_truck_has_no_driver_for_Date_Shift()
        {
            // Arrange
            var date = Clock.Now.Date;
            var shift = Shift.Shift1;
            var orderEntity = await CreateOrder(date, shift);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            var anotherDate = Clock.Now.Date.AddDays(1);
            await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, anotherDate, shift, null);
            SubstituteSetting(AppSettings.General.UseShifts, "true");

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                Shift = shift,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = null,
            });

            // Assert
            result.Completed.ShouldBeFalse();
            result.NotAvailableTrucks.ShouldNotBeNull();
            result.NotAvailableTrucks.Count.ShouldBe(1);
            result.NotAvailableTrucks[0].ShouldBe(truck.TruckCode);
            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.DeliveryDate.ShouldBe(date);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_entire_Order_should_change_Date_when_truck_has_driver_for_Date_Shift()
        {
            // Arrange
            var date = Clock.Now.Date;
            var shift = Shift.Shift1;
            var orderEntity = await CreateOrder(date, shift);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            var anotherDate = Clock.Now.Date.AddDays(1);
            await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, anotherDate, shift, null);
            SubstituteSetting(AppSettings.General.UseShifts, "true");

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                Shift = Shift.Shift2,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = null,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.NotAvailableTrucks.ShouldBeNull();
            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.DeliveryDate.ShouldBe(anotherDate);
            orderEntityOriginal.Shift.ShouldBe(Shift.Shift2);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_OrderLine_should_return_NotAvailableTrucks_when_truck_has_no_driver_for_Date()
        {
            // Arrange
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            var anotherDate = Clock.Now.Date.AddDays(1);
            await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, anotherDate, null);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = orderLine.Id,
            });

            // Assert
            result.Completed.ShouldBeFalse();
            result.NotAvailableTrucks.ShouldNotBeNull();
            result.NotAvailableTrucks.Count.ShouldBe(1);
            result.NotAvailableTrucks[0].ShouldBe(truck.TruckCode);
            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.DeliveryDate.ShouldBe(date);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_entire_Order_should_change_Date_when_trucks_are_available_for_both_Date_and_Shift()
        {
            // Arrange
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            orderEntity = await UpdateEntity(orderEntity, o => o.Shift = Shift.Shift1);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            var anotherDate = Clock.Now.Date.AddDays(1);
            var anotherOrderEntity = await CreateOrder(anotherDate);
            anotherOrderEntity = await UpdateEntity(anotherOrderEntity, o => o.Shift = Shift.Shift3);
            var anotherOrderLine = anotherOrderEntity.OrderLines.First();
            await CreateOrderLineTruck(truck.Id, driver.Id, anotherOrderLine.Id, 1);
            SubstituteSetting(AppSettings.General.UseShifts, "true");

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = null,
                Shift = Shift.Shift1,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.NotAvailableTrucks.ShouldBeNull();
            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.DeliveryDate.ShouldBe(anotherDate);
            orderEntityOriginal.Shift.ShouldBe(Shift.Shift1);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_entire_Order_should_change_Date_when_trucks_are_not_fully_utilized_for_Date_and_Shift()
        {
            // Arrange
            var date = Clock.Now.Date;
            var order1 = await CreateOrder(date);
            order1.OrderLines.Count.ShouldBe(2);
            order1 = await UpdateEntity(order1, o => o.Shift = Shift.Shift1);
            var orderLine1 = order1.OrderLines.First();
            var order2 = await CreateOrder(date);
            order2.OrderLines.Count.ShouldBe(2);
            order2 = await UpdateEntity(order2, o => o.Shift = Shift.Shift2);
            var orderLine2 = order2.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, .5m);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, .5m);

            SubstituteSetting(AppSettings.General.UseShifts, "true");

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = date,
                KeepTrucks = true,
                OrderId = order1.Id,
                OrderLineId = null,
                Shift = Shift.Shift2,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.NotAvailableTrucks.ShouldBeNull();
            var orderEntityOriginal = await GetOrder(order1.Id);
            orderEntityOriginal.DeliveryDate.ShouldBe(date);
            orderEntityOriginal.Shift.ShouldBe(Shift.Shift2);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_one_OrderLine_should_return_NotAvailableTrucks_when_trucks_are_not_available_for_Date()
        {
            // Arrange
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            var anotherDate = Clock.Now.Date.AddDays(1);
            var anotherOrderEntity = await CreateOrder(anotherDate);
            var anotherOrderLine = anotherOrderEntity.OrderLines.First();
            await CreateOrderLineTruck(truck.Id, driver.Id, anotherOrderLine.Id, 1);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = orderLine.Id,
            });

            // Assert
            result.Completed.ShouldBeFalse();
            result.NotAvailableTrucks.ShouldNotBeNull();
            result.NotAvailableTrucks.Count.ShouldBe(1);
            result.NotAvailableTrucks[0].ShouldBe(truck.TruckCode);
            var ordersNumber = await UsingDbContextAsync(async context => await context.Orders.CountAsync());
            ordersNumber.ShouldBe(2); // No new orders were created
        }

        [Fact]
        public async Task Test_SetOrderDate_for_one_OrderLine_should_return_NotAvailableTrucks_when_trucks_are_not_available_for_Date_and_Shift()
        {
            // Arrange
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity = await UpdateEntity(orderEntity, o => o.Shift = Shift.Shift1);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            var anotherDate = Clock.Now.Date.AddDays(1);
            var anotherOrderEntity = await CreateOrder(anotherDate);
            anotherOrderEntity = await UpdateEntity(anotherOrderEntity, o => o.Shift = Shift.Shift1);
            var anotherOrderLine = anotherOrderEntity.OrderLines.First();
            await CreateOrderLineTruck(truck.Id, driver.Id, anotherOrderLine.Id, 1);
            SubstituteSetting(AppSettings.General.UseShifts, "true");

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = orderLine.Id,
                Shift = Shift.Shift1,
            });

            // Assert
            result.Completed.ShouldBeFalse();
            result.NotAvailableTrucks.ShouldNotBeNull();
            result.NotAvailableTrucks.Count.ShouldBe(1);
            result.NotAvailableTrucks[0].ShouldBe(truck.TruckCode);
            var ordersNumber = await UsingDbContextAsync(async context => await context.Orders.CountAsync());
            ordersNumber.ShouldBe(2); // No new orders were created
        }

        [Fact]
        public async Task Test_SetOrderDate_for_entire_Order_should_change_date_and_remove_trucks_when_trucks_are_not_available_for_Date_and_RemoveNotAvailableTrucks_is_true()
        {
            // Arrange
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            var anotherDate = Clock.Now.Date.AddDays(1);
            var anotherOrderEntity = await CreateOrder(anotherDate);
            var anotherOrderLine = anotherOrderEntity.OrderLines.First();
            await CreateOrderLineTruck(truck.Id, driver.Id, anotherOrderLine.Id, 1);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = null,
                RemoveNotAvailableTrucks = true,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.NotAvailableTrucks.ShouldBeNull();
            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.DeliveryDate.ShouldBe(anotherDate);
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => !olt.IsDeleted && olt.OrderLine.OrderId == orderEntity.Id).ToListAsync());
            orderLineTrucks.Count.ShouldBe(0);
        }
        [Fact]
        public async Task Test_SetOrderDate_for_one_OrderLine_should_change_date_and_remove_trucks_when_trucks_are_not_available_for_Date_and_RemoveNotAvailableTrucks_is_true()
        {
            // Arrange
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);

            var anotherDate = Clock.Now.Date.AddDays(1);
            var anotherOrderEntity = await CreateOrder(anotherDate);
            var anotherOrderLine = anotherOrderEntity.OrderLines.First();
            await CreateOrderLineTruck(truck.Id, driver.Id, anotherOrderLine.Id, 1);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = orderLine.Id,
                RemoveNotAvailableTrucks = true,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.NotAvailableTrucks.ShouldBeNull();
            var allOrders = await UsingDbContextAsync(async context => await context.Orders.Include(o => o.OrderLines).ToListAsync());
            allOrders.Count.ShouldBe(3);
            var originalOrder = allOrders.First(o => o.Id == orderEntity.Id);
            originalOrder.OrderLines.Count.ShouldBe(1);
            var newOrderEntity = allOrders.First(o => o.OrderLines.Any(ol => ol.Id == orderLine.Id));
            newOrderEntity.DeliveryDate.ShouldBe(anotherDate);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_one_OrderLine_should_change_date_when_there_is_Dispatch_for_another_OrderLine()
        {
            // Arrange
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var anotherOrderLine = orderEntity.OrderLines.First(ol => ol.Id != orderLine.Id);
            var truck = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Sent);
            var anotherDate = Clock.Now.Date.AddDays(1);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = anotherOrderLine.Id,
                RemoveNotAvailableTrucks = false,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            var allOrders = await UsingDbContextAsync(async context => await context.Orders.Include(o => o.OrderLines).ToListAsync());
            var newOrderEntity = allOrders.First(o => o.OrderLines.Any(ol => ol.Id == anotherOrderLine.Id));
            newOrderEntity.DeliveryDate.ShouldBe(anotherDate);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_one_OrderLine_should_throw_UserFriendlyException_when_there_is_Dispatch_for_this_OrderLine()
        {
            // Arrange
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Sent);
            var anotherDate = Clock.Now.Date.AddDays(1);

            // Act, Assert
            await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = orderLine.Id,
                RemoveNotAvailableTrucks = false,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_SetOrderDate_for_entire_Order_should_throw_UserFriendlyException_when_there_is_Dispatch_for_OrderLine()
        {
            // Arrange
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Sent);
            var anotherDate = Clock.Now.Date.AddDays(1);

            // Act, Assert
            await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = null,
                RemoveNotAvailableTrucks = false,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_SetOrderDate_for_entire_Order_should_change_Date_of_DriverAssignment_when_there_is_no_DriverAssignment_on_DateShift()
        {
            // Arrange
            SubstituteSetting(AppSettings.General.UseShifts, "true");
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date, Shift.Shift1);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            var driverAssignment = await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, date, Shift.Shift1, driver.Id);
            await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, date, Shift.Shift2, driver.Id);
            var anotherDate = date.AddDays(1);
            await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, anotherDate, Shift.Shift2, driver.Id);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                Shift = Shift.Shift1,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = null,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.NotAvailableTrucks.ShouldBeNull();
            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.DeliveryDate.ShouldBe(anotherDate);
            var driverAssignments = await UsingDbContextAsync(async context => await context.DriverAssignments.ToListAsync());
            driverAssignments.Count.ShouldBe(3);
            var updatedDriverAssignment = driverAssignments.Single(da => da.Id == driverAssignment.Id);
            updatedDriverAssignment.Date.ShouldBe(anotherDate);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_entire_Order_should_not_change_Date_of_DriverAssignment_when_there_is_DriverAssignment_on_DateShift()
        {
            // Arrange
            SubstituteSetting(AppSettings.General.UseShifts, "true");
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date, Shift.Shift1);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            var driverAssignment = await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, date, Shift.Shift1, driver.Id);
            await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, date, Shift.Shift2, driver.Id);
            var anotherDate = date.AddDays(1);
            await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, anotherDate, Shift.Shift1, driver.Id);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                Shift = Shift.Shift1,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = null,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.NotAvailableTrucks.ShouldBeNull();
            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.DeliveryDate.ShouldBe(anotherDate);
            var driverAssignments = await UsingDbContextAsync(async context => await context.DriverAssignments.ToListAsync());
            driverAssignments.Count.ShouldBe(3);
            var updatedDriverAssignment = driverAssignments.Single(da => da.Id == driverAssignment.Id);
            updatedDriverAssignment.Date.ShouldBe(date);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_OrderLine_should_copy_DriverAssignment_when_there_is_no_DriverAssignment_on_DateShift()
        {
            // Arrange
            SubstituteSetting(AppSettings.General.UseShifts, "true");
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date, Shift.Shift1);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            var driverAssignment = await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, date, Shift.Shift1, driver.Id);
            await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, date, Shift.Shift2, driver.Id);
            var anotherDate = date.AddDays(1);
            await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, anotherDate, Shift.Shift2, driver.Id);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                Shift = Shift.Shift1,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = orderLine.Id,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.NotAvailableTrucks.ShouldBeNull();
            var driverAssignments = await UsingDbContextAsync(async context => await context.DriverAssignments.ToListAsync());
            driverAssignments.Count.ShouldBe(4);
            var updatedDriverAssignment = driverAssignments.Single(da => da.Id == driverAssignment.Id);
            updatedDriverAssignment.Date.ShouldBe(date);
            driverAssignments.Count(da =>
                da.TruckId == driverAssignment.TruckId
                && da.DriverId == driverAssignment.DriverId
                && da.Date == anotherDate
                && da.Shift == Shift.Shift1
            ).ShouldBe(1);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_OrderLine_should_not_copy_DriverAssignment_when_there_is_DriverAssignment_on_DateShift()
        {
            // Arrange
            SubstituteSetting(AppSettings.General.UseShifts, "true");
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date, Shift.Shift1);
            orderEntity.OrderLines.Count.ShouldBe(2);
            var orderLine = orderEntity.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            var driverAssignment = await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, date, Shift.Shift1, driver.Id);
            await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, date, Shift.Shift2, driver.Id);
            var anotherDate = date.AddDays(1);
            await CreateDriverAssignmentForTruck(orderEntity.OfficeId, truck.Id, anotherDate, Shift.Shift1, driver.Id);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                Shift = Shift.Shift1,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = orderLine.Id,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.NotAvailableTrucks.ShouldBeNull();
            var driverAssignments = await UsingDbContextAsync(async context => await context.DriverAssignments.ToListAsync());
            driverAssignments.Count.ShouldBe(3);
            var updatedDriverAssignment = driverAssignments.Single(da => da.Id == driverAssignment.Id);
            updatedDriverAssignment.Date.ShouldBe(date);
            driverAssignments.Count(da =>
                da.TruckId == driverAssignment.TruckId
                && da.DriverId == driverAssignment.DriverId
                && da.Date == anotherDate
                && da.Shift == Shift.Shift1
            ).ShouldBe(1);
        }

        [Fact]
        public async Task Test_SetOrderDate_for_entire_Order_should_change_Date_when_and_remove_LeaseHaulerTrucks_when_AllowLeaseHaulersFeature_is_true()
        {
            // Arrange
            SubstituteAllowLeaseHaulersFeature(true);

            var date = Clock.Now.Date;
            var orderEntity = await CreateOrderWithOrderLines(date);
            var orderLine = orderEntity.OrderLines.First();
            var leaseHauler = await CreateLeaseHauler();
            var truck = await CreateLeaseHaulerTruck(leaseHauler.Id);
            await CreateOrderLineTruck(truck.Id, truck.DefaultDriverId, orderLine.Id, 1);

            var anotherDate = Clock.Now.Date.AddDays(1);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                Date = anotherDate,
                KeepTrucks = true,
                OrderId = orderEntity.Id,
                OrderLineId = null,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.NotAvailableTrucks.ShouldBeNull();
            var orderEntityOriginal = await GetOrder(orderEntity.Id);
            orderEntityOriginal.DeliveryDate.ShouldBe(anotherDate);
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => olt.OrderLine.OrderId == orderEntity.Id).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
            orderLineTrucks[0].IsDeleted.ShouldBeTrue();
        }

        [Fact]
        public async Task Test_SetOrderDate_should_copy_Order_and_move_OrderLine_without_LeaseHaulerTruck_when_update_date_for_only_one_OrderLine_and_AllowLeaseHaulersFeature_is_true()
        {
            // Arrange
            SubstituteAllowLeaseHaulersFeature(true);
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrderWithOrderLines(date);
            var orderLine = orderEntity.OrderLines.First();
            var leaseHauler = await CreateLeaseHauler();
            var truck = await CreateLeaseHaulerTruck(leaseHauler.Id);
            await CreateOrderLineTruck(truck.Id, truck.DefaultDriverId, orderLine.Id, 1);
            var newDate = Clock.Now.Date.AddDays(1);

            // Act
            var result = await _orderAppService.SetOrderDate(new SetOrderDateInput
            {
                OrderId = orderEntity.Id,
                OrderLineId = orderLine.Id,
                Date = newDate,
            });

            // Assert
            result.Completed.ShouldBeTrue();
            result.NotAvailableTrucks.ShouldBeNull();
            var allOrders = await UsingDbContextAsync(async context => await context.Orders.Include(o => o.OrderLines).ToListAsync());
            allOrders.Count.ShouldBe(2);
            var newOrderEntity = allOrders.First(o => o.Id != orderEntity.Id);
            newOrderEntity.DeliveryDate.ShouldBe(newDate);
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => olt.OrderLine.OrderId == newOrderEntity.Id).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
            orderLineTrucks[0].IsDeleted.ShouldBeTrue();
        }




        private async Task<Order> CreateOrder(DateTime? date = null, Shift? shift = null)
        {
            date ??= DateTime.UtcNow.Date;

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
                    FreightPricePerUnit = 2,
                    MaterialPricePerUnit = 3,
                    FreightItem = new Item
                    {
                        TenantId = 1,
                        Name = "sss",
                    },
                    MaterialUomId = 1,
                });
                order.OrderLines.Add(new OrderLine
                {
                    TenantId = 1,
                    LineNumber = 2,
                    Designation = DesignationEnum.FreightAndMaterial,
                    FreightPricePerUnit = 20,
                    MaterialPricePerUnit = 30,
                    MaterialQuantity = 2,
                    FreightPrice = 20 * 2,
                    MaterialPrice = 30 * 2,
                    FreightItem = new Item
                    {
                        TenantId = 1,
                        Name = "sss",
                    },
                    MaterialUomId = 1,
                });
                context.Orders.Add(order);

                var user = await context.Users.FirstAsync(u => u.TenantId == 1);
                user.Office = order.Office;

                await context.SaveChangesAsync();

                return order;
            });
            return orderEntity;
        }

        private async Task<Office> CreateOffice()
        {
            var officeEntity = await UsingDbContextAsync(async context =>
            {
                var office = new Office() { TenantId = 1, Name = "Office2", TruckColor = "ccc" };
                await context.Offices.AddAsync(office);
                return office;
            });
            return officeEntity;
        }

        private async Task<Truck> CreateTruck(string truckCode = "101")
        {
            var vehicleCategory = await CreateVehicleCategory();
            var truckEntity = await UsingDbContextAsync(async context =>
            {
                var office = await context.Orders.Select(o => o.Office).FirstAsync();
                var truck = new Truck
                {
                    TenantId = 1,
                    TruckCode = truckCode,
                    IsActive = true,
                    AlwaysShowOnSchedule = true,
                    VehicleCategoryId = vehicleCategory.Id,
                    OfficeId = office.Id,
                };
                await context.Trucks.AddAsync(truck);
                return truck;
            });
            return truckEntity;
        }

        private async Task<Order> GetOrder(int orderId)
        {
            return await UsingDbContextAsync(async context => await
                context.Orders
                    .Include(o => o.OrderLines)
                    .FirstAsync(o => o.Id == orderId));
        }

    }
}
