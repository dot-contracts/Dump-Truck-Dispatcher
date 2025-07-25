using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Infrastructure.Sms.Dto;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dispatching
{
    public class DispatchingAppService_SendOrdersToDrivers_Tests : DispatchingAppService_Tests_Base
    {
        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SendOrdersToDrivers_should_create_Dispatches_in_Complete_state_when_DispatchVia_setting_is_None()
        {
            // Arrange
            var today = Clock.Now.Date;

            var truck1 = await CreateTruck();
            var driver1 = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550055");
            var driverAssignment1 = await CreateDriverAssignmentForTruck(_officeId, truck1.Id, today, driver1.Id);
            await UpdateEntity(driverAssignment1, da => da.StartTime = today.AddHours(12));

            var truck2 = await CreateTruck();
            var driver2 = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550056");
            var driverAssignment2 = await CreateDriverAssignmentForTruck(_officeId, truck2.Id, today, driver2.Id);
            await UpdateEntity(driverAssignment2, da => da.StartTime = today.AddHours(12));

            var order = await CreateOrderWithOrderLines(today);
            var orderLine1 = order.OrderLines.ToList()[0];
            var orderLine2 = order.OrderLines.ToList()[1];
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine1.Id, 1);
            await CreateOrderLineTruck(truck2.Id, driver2.Id, orderLine2.Id, 1);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));
            SubstituteServiceDependencies(_dispatchingAppService);
            SubstituteDispatchViaSetting(DispatchVia.None);

            // Act
            await _dispatchingAppService.SendOrdersToDrivers(new SendOrdersToDriversInput
            {
                DeliveryDate = today,
                OfficeIds = new[] { _officeId },
            });

            // Assert
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.OrderBy(d => d.Id).ToListAsync());
            dispatches.Count.ShouldBe(2);
            dispatches[0].Status.ShouldBe(DispatchStatus.Completed);
            dispatches[1].Status.ShouldBe(DispatchStatus.Completed);
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SendOrdersToDrivers_should_create_Dispatches_for_each_unique_driver_per_OrderLine()
        {
            // Arrange
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driver = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550055");

            var driverAssignment1 = await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driver.Id);
            await UpdateEntity(driverAssignment1, da => da.StartTime = today.AddHours(12));
            var order = await CreateOrderWithOrderLines(today);
            var orderLine1 = order.OrderLines.ToList()[0];
            var orderLine2 = order.OrderLines.ToList()[1];
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, .5m);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, .5m);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendOrdersToDrivers(new SendOrdersToDriversInput
            {
                DeliveryDate = today,
                OfficeIds = new[] { _officeId },
            });

            // Assert
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.OrderBy(d => d.Id).ToListAsync());
            dispatches.Count.ShouldBe(1);
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SendOrdersToDrivers_should_create_Dispatches_for_Date_and_Shift()
        {
            // Arrange
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driver = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550055");

            var driverAssignment1 = await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, Shift.Shift1, driver.Id);
            await UpdateEntity(driverAssignment1, da => da.StartTime = today.AddHours(12));
            var driverAssignment2 = await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, Shift.Shift2, driver.Id);
            await UpdateEntity(driverAssignment2, da => da.StartTime = today.AddHours(12));
            var order1 = await CreateOrderWithOrderLines(today);
            order1 = await UpdateEntity(order1, o => o.Shift = Shift.Shift1);
            var orderLine1 = order1.OrderLines.ToList()[0];
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);
            var order2 = await CreateOrderWithOrderLines(today);
            order2 = await UpdateEntity(order2, o => o.Shift = Shift.Shift2);
            var orderLine2 = order2.OrderLines.ToList()[0];
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, 1);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendOrdersToDrivers(new SendOrdersToDriversInput
            {
                DeliveryDate = today,
                Shift = Shift.Shift1,
                OfficeIds = new[] { _officeId },
            });

            // Assert
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.OrderBy(d => d.Id).ToListAsync());
            dispatches.Count.ShouldBe(1);
            dispatches.First().OrderLineId.ShouldBe(orderLine1.Id);
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SendOrdersToDrivers_should_create_Dispatches_for_Date_and_Shift_with_MultipleLoads_from_OrderLine()
        {
            // Arrange
            var today = Clock.Now.Date;
            var truck1 = await CreateTruck();
            var truck2 = await CreateTruck();
            var driver1 = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550055");
            var driver2 = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550055");

            var driverAssignment1 = await CreateDriverAssignmentForTruck(_officeId, truck1.Id, today, driver1.Id);
            var driverAssignment2 = await CreateDriverAssignmentForTruck(_officeId, truck2.Id, today, driver2.Id);
            await UpdateEntity(driverAssignment1, da => da.StartTime = today.AddHours(12));
            await UpdateEntity(driverAssignment2, da => da.StartTime = today.AddHours(12));
            var order = await CreateOrderWithOrderLines(today);
            var orderLine1 = order.OrderLines.ToList()[0];
            await UpdateEntity(orderLine1, ol => ol.IsMultipleLoads = true);
            var orderLine2 = order.OrderLines.ToList()[1];
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine1.Id, 1);
            await CreateOrderLineTruck(truck2.Id, driver2.Id, orderLine2.Id, 1);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendOrdersToDrivers(new SendOrdersToDriversInput
            {
                DeliveryDate = today,
                OfficeIds = new[] { _officeId },
            });

            // Assert
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.OrderBy(d => d.Id).ToListAsync());
            dispatches.Count.ShouldBe(2);
            var dispatch1 = dispatches.First(d => d.OrderLineId == orderLine1.Id);
            dispatch1.Status.ShouldBe(DispatchStatus.Sent);
            dispatch1.IsMultipleLoads.ShouldBeTrue();
            var dispatch2 = dispatches.First(d => d.OrderLineId == orderLine2.Id);
            dispatch2.Status.ShouldBe(DispatchStatus.Sent);
            dispatch2.IsMultipleLoads.ShouldBeFalse();
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SendOrdersToDrivers_should_create_Dispatches_for_Date_and_Shift_when_Driver_is_from_another_office()
        {
            // Arrange
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var office2 = await CreateOffice();
            var driver = await CreateDriver(officeId: office2.Id, phoneNumber: "+15005550055");

            var driverAssignment1 = await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, Shift.Shift1, driver.Id);
            await UpdateEntity(driverAssignment1, da => da.StartTime = today.AddHours(12));
            var driverAssignment2 = await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, Shift.Shift2, driver.Id);
            await UpdateEntity(driverAssignment2, da => da.StartTime = today.AddHours(12));
            var order1 = await CreateOrderWithOrderLines(today);
            order1 = await UpdateEntity(order1, o => o.Shift = Shift.Shift1);
            var orderLine1 = order1.OrderLines.ToList()[0];
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);
            var order2 = await CreateOrderWithOrderLines(today);
            order2 = await UpdateEntity(order2, o => o.Shift = Shift.Shift2);
            var orderLine2 = order2.OrderLines.ToList()[0];
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, 1);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendOrdersToDrivers(new SendOrdersToDriversInput
            {
                DeliveryDate = today,
                Shift = Shift.Shift1,
                OfficeIds = new[] { _officeId },
            });

            // Assert
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.OrderBy(d => d.Id).ToListAsync());
            dispatches.Count.ShouldBe(1);
            dispatches.First().OrderLineId.ShouldBe(orderLine1.Id);
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SendOrdersToDrivers_should_create_Dispatches_for_Date_and_Shift_and_OrderLineTruck_not_IsDone()
        {
            // Arrange
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var truck2 = await CreateTruck();
            var driver = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550055");
            var driver2 = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550056");

            var driverAssignment1 = await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, Shift.Shift1, driver.Id);
            await UpdateEntity(driverAssignment1, da => da.StartTime = today.AddHours(12));
            var driverAssignment2 = await CreateDriverAssignmentForTruck(_officeId, truck2.Id, today, Shift.Shift1, driver2.Id);
            await UpdateEntity(driverAssignment2, da => da.StartTime = today.AddHours(12));
            var order1 = await CreateOrderWithOrderLines(today);
            order1 = await UpdateEntity(order1, o => o.Shift = Shift.Shift1);
            var order1Line1 = order1.OrderLines.ToList()[0];
            var order1Line2 = order1.OrderLines.ToList()[1];
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, order1Line1.Id, 0);
            orderLineTruck = await UpdateEntity(orderLineTruck, olt => olt.IsDone = true);
            await CreateOrderLineTruck(truck.Id, driver.Id, order1Line2.Id, 1);

            var order2 = await CreateOrderWithOrderLines(today);
            order2 = await UpdateEntity(order2, o => o.Shift = Shift.Shift1);
            var orderLine2 = order2.OrderLines.ToList()[0];
            var orderLineTruck2 = await CreateOrderLineTruck(truck2.Id, driver2.Id, orderLine2.Id, 0);
            orderLineTruck2 = await UpdateEntity(orderLineTruck2, olt2 => olt2.IsDone = true);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendOrdersToDrivers(new SendOrdersToDriversInput
            {
                DeliveryDate = today,
                Shift = Shift.Shift1,
                OfficeIds = new[] { _officeId },
            });

            // Assert
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.OrderBy(d => d.Id).ToListAsync());
            dispatches.Count.ShouldBe(1);
            dispatches.First().OrderLineId.ShouldBe(order1Line2.Id);
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
        }

        [Fact]
        public async Task Test_SendOrdersToDrivers_should_not_create_Dispatches_for_closed_OrderLine()
        {
            // Arrange
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driver = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550055");

            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, Shift.Shift1, driver.Id);
            var order1 = await CreateOrderWithOrderLines(today);
            order1 = await UpdateEntity(order1, o => o.Shift = Shift.Shift1);
            var orderLine1 = order1.OrderLines.ToList()[0];
            orderLine1 = await UpdateEntity(orderLine1, ol => ol.IsComplete = true);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendOrdersToDrivers(new SendOrdersToDriversInput
            {
                DeliveryDate = today,
                Shift = Shift.Shift1,
                OfficeIds = new[] { _officeId },
            }).ShouldThrowAsync(typeof(UserFriendlyException));

            // Assert
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.OrderBy(d => d.Id).ToListAsync());
            dispatches.Count.ShouldBe(0);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SendOrdersToDrivers_should_not_create_Dispatches_for_OrderLineTruck_IsDone_true()
        {
            // Arrange
            var today = Clock.Now.Date;
            var truck1 = await CreateTruck();
            var truck2 = await CreateTruck();
            var driver = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550055");

            var driverAssignment1 = await CreateDriverAssignmentForTruck(_officeId, truck1.Id, today, Shift.Shift1, driver.Id);
            await UpdateEntity(driverAssignment1, da => da.StartTime = today.AddHours(12));
            var driverAssignment2 = await CreateDriverAssignmentForTruck(_officeId, truck2.Id, today, Shift.Shift1, driver.Id);
            await UpdateEntity(driverAssignment2, da => da.StartTime = today.AddHours(12));
            var order1 = await CreateOrderWithOrderLines(today);
            order1 = await UpdateEntity(order1, o => o.Shift = Shift.Shift1);
            var orderLine1 = order1.OrderLines.ToList()[0];
            orderLine1.IsComplete.ShouldBeFalse();
            var orderLineTruck = await CreateOrderLineTruck(truck1.Id, driver.Id, orderLine1.Id, 1);
            await UpdateEntity(orderLineTruck, olt => olt.IsDone = true);
            var orderLineTruck2 = await CreateOrderLineTruck(truck2.Id, driver.Id, orderLine1.Id, 1);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendOrdersToDrivers(new SendOrdersToDriversInput
            {
                DeliveryDate = today,
                Shift = Shift.Shift1,
                OfficeIds = new[] { _officeId },
            });

            // Assert
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.OrderBy(d => d.Id).ToListAsync());
            dispatches.Count.ShouldBe(1);
            dispatches[0].TruckId.ShouldBe(truck2.Id);
        }

        [Fact]
        public async Task Test_SendOrdersToDrivers_should_throw_UserFriendlyException_when_there_are_no_TruckDrivers_for_Date_and_Shift()
        {
            // Arrange
            var today = Clock.Now.Date;
            var order1 = await CreateOrderWithOrderLines(today);
            order1 = await UpdateEntity(order1, o => o.Shift = Shift.Shift1);
            var orderLine1 = order1.OrderLines.ToList()[0];

            // Act, Assert
            await _dispatchingAppService.SendOrdersToDrivers(new SendOrdersToDriversInput
            {
                DeliveryDate = today,
                Shift = Shift.Shift1,
                OfficeIds = new[] { _officeId },
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SendOrdersToDrivers_should_create_Dispatches_for_Tomorrow()
        {
            // Arrange
            var tomorrow = Clock.Now.Date.AddDays(1);
            var truck = await CreateTruck();
            var driver = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550055");

            var driverAssignment1 = await CreateDriverAssignmentForTruck(_officeId, truck.Id, tomorrow, Shift.Shift1, driver.Id);
            await UpdateEntity(driverAssignment1, da => da.StartTime = tomorrow.AddHours(12));
            var order1 = await CreateOrderWithOrderLines(tomorrow);
            order1 = await UpdateEntity(order1, o => o.Shift = Shift.Shift1);
            var orderLine1 = order1.OrderLines.ToList()[0];
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendOrdersToDrivers(new SendOrdersToDriversInput
            {
                DeliveryDate = tomorrow,
                Shift = Shift.Shift1,
                OfficeIds = new[] { _officeId },
            });

            // Assert
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.OrderBy(d => d.Id).ToListAsync());
            dispatches.Count.ShouldBe(1);
            dispatches.First().OrderLineId.ShouldBe(orderLine1.Id);
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SendOrdersToDrivers_should_create_Dispatches_for_Date_and_Shift_per_OrderLine()
        {
            // Arrange
            var today = Clock.Now.Date;

            var truck1 = await CreateTruck();
            var driver1 = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550055");
            var driverAssignment1 = await CreateDriverAssignmentForTruck(_officeId, truck1.Id, today, Shift.Shift1, driver1.Id);
            await UpdateEntity(driverAssignment1, da => da.StartTime = today.AddHours(12));

            var truck2 = await CreateTruck();
            var driver2 = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550056");
            var driverAssignment2 = await CreateDriverAssignmentForTruck(_officeId, truck2.Id, today, Shift.Shift1, driver2.Id);
            await UpdateEntity(driverAssignment2, da => da.StartTime = today.AddHours(12));

            var order1 = await CreateOrderWithOrderLines(today);
            order1 = await UpdateEntity(order1, o => o.Shift = Shift.Shift1);
            var orderLine1 = order1.OrderLines.ToList()[0];
            var orderLine2 = order1.OrderLines.ToList()[1];
            await CreateOrderLineTruck(truck1.Id, driver1.Id, orderLine1.Id, 1);
            await CreateOrderLineTruck(truck2.Id, driver2.Id, orderLine2.Id, 1);
            //_smsSender.SendAsync("", "+15005550055", true).Returns(new SmsSendResult("1", SmsStatus.Sent, null, null));
            //_smsSender.SendAsync("", "+15005550056", true).Returns(new SmsSendResult("2", SmsStatus.Sent, null, null));
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("0", SmsStatus.Unknown, null, null));
            SubstituteServiceDependencies(_dispatchingAppService);
            SubstituteDispatchViaSetting(DispatchVia.SimplifiedSms);
            SubstituteSetting(AppSettings.DispatchingAndMessaging.DriverDispatchSmsTemplate, "");

            // Act
            await _dispatchingAppService.SendOrdersToDrivers(new SendOrdersToDriversInput
            {
                DeliveryDate = today,
                Shift = Shift.Shift1,
                OfficeIds = new[] { _officeId },
            });

            // Assert
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.OrderBy(d => d.Id).ToListAsync());
            dispatches.Count.ShouldBe(2);
            dispatches[0].OrderLineId.ShouldBe(orderLine1.Id);
            dispatches[1].OrderLineId.ShouldBe(orderLine2.Id);
            //await _smsSender.Received().SendAsync(Arg.Any<string>(), "+15005550055", true);
            //await _smsSender.Received().SendAsync(Arg.Any<string>(), "+15005550056", true);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SendOrdersToDrivers_should_create_Dispatches_for_each_Order_sorted_by_TimeOnJob()
        {
            // Arrange
            var today = Clock.Now.Date;
            var truck = await CreateTruck();
            var driver = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550055");

            var driverAssignment1 = await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, driver.Id);
            await UpdateEntity(driverAssignment1, da => da.StartTime = today.AddHours(12));

            var order1 = await CreateOrderWithOrderLines(today);
            var orderLine1 = order1.OrderLines.First();
            orderLine1 = await UpdateEntity(orderLine1, ol => ol.TimeOnJob = today.AddDays(1).AddHours(10)); // Should be used only time part
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);

            var order2 = await CreateOrderWithOrderLines(today);
            var orderLine2 = order2.OrderLines.First();
            orderLine2 = await UpdateEntity(orderLine2, ol => ol.TimeOnJob = today.AddHours(12));
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, 1);

            var order3 = await CreateOrderWithOrderLines(today);
            var orderLine3 = order3.OrderLines.First();
            orderLine3 = await UpdateEntity(orderLine3, ol => ol.TimeOnJob = null);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine3.Id, 1);
            SubstituteServiceDependencies(_dispatchingAppService);
            SubstituteDispatchViaSetting(DispatchVia.SimplifiedSms);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.SendOrdersToDrivers(new SendOrdersToDriversInput
            {
                DeliveryDate = today,
                OfficeIds = new[] { _officeId },
            });

            // Assert
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.OrderBy(d => d.Id).ToListAsync());
            dispatches.Count.ShouldBe(1);
            var firstDispatch = dispatches.First(d => d.Status == DispatchStatus.Sent);
            firstDispatch.OrderLineId.ShouldBe(orderLine1.Id);
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
        }

    }
}
