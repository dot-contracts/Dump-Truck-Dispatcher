using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.Locations;
using DispatcherWeb.UnitsOfMeasure;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dispatching
{
    public class DispatchingAppService_CreateSendDispatchMessageDto_Tests : DispatchingAppService_Tests_Base
    {
        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_CreateSendDispatchMessageDto_should_return_message()
        {
            // Arrange
            var today = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(today);
            order = await UpdateEntity(order, o =>
            {
                o.Shift = Shift.Shift1;
                o.Directions = "Comments";
            });
            var orderLine = order.OrderLines.First();
            var loadAt = new Location { Name = "name" };
            var deliverTo = new Location { Name = "deliverTo" };
            orderLine = await UpdateEntity(orderLine, ol =>
            {
                ol.LoadAt = loadAt;
                ol.DeliverTo = deliverTo;
                ol.MaterialQuantity = 10;
                ol.FreightQuantity = 10;
                ol.MaterialUom = new UnitOfMeasure() { Name = "UOM" };
                ol.FreightUom = new UnitOfMeasure() { Name = "UOM" };
                ol.TimeOnJob = Clock.Now;
                ol.Note = "Note";
            });
            SubstituteSetting(AppSettings.DispatchingAndMessaging.DriverDispatchSmsTemplate,
                "|{DeliveryDate}|{Shift}|{OrderNumber}|{Customer}|{Comments}|{TimeOnJob}|{Item}|{LoadAt}|{Quantity}|{UOM}|{DeliverTo}");
            SubstituteSetting(AppSettings.General.ShiftName1, "ShiftName1");

            // Act
            var dto = await _dispatchingAppService.CreateSendDispatchMessageDto(orderLine.Id);

            // Assert
            var tokens = dto.Message.Split('|');
            tokens.Length.ShouldBe(15);
            int i = 1;
            tokens[i++].ShouldBe(order.DeliveryDate.ToString("d"));
            tokens[i++].ShouldBe("ShiftName1");
            tokens[i++].ShouldBe(order.Id.ToString());
            tokens[i++].ShouldBe(order.Customer.Name);
            tokens[i++].ShouldBe(order.Directions);
            tokens[i++].ShouldBe(orderLine.TimeOnJob?.ToString("t"));
            tokens[i++].ShouldBe(orderLine.FreightItem.Name);
            tokens[i++].ShouldBe(loadAt.Name);
            tokens[i++].ShouldBe(orderLine.MaterialQuantity?.ToString("N"));
            tokens[i++].ShouldBe(orderLine.MaterialUom.Name);
            tokens[i].ShouldBe(deliverTo.Name);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Test_CreateSendDispatchMessageDto_should_return_IsMultipleLoad_from_OrderLine(bool isMultipleLoads)
        {
            // Arrange
            var today = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(today, Shift.Shift1);
            var orderLine = order.OrderLines.First();
            await UpdateEntity(orderLine, ol => ol.IsMultipleLoads = isMultipleLoads);
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, Shift.Shift1, driver.Id);

            // Act
            var dto = await _dispatchingAppService.CreateSendDispatchMessageDto(orderLine.Id);

            // Assert
            dto.IsMultipleLoads.ShouldBe(isMultipleLoads);
        }

        [Fact]
        public async Task Test_CreateSendDispatchMessageDto_should_return_Drivers_only_for_Shift_and_Date()
        {
            // Arrange
            var today = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(today, Shift.Shift1);
            var order2 = await CreateOrderWithOrderLines(today, Shift.Shift2);
            var orderLine = order.OrderLines.First();
            var orderLine2 = order2.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, 1);
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, Shift.Shift2, driver.Id);
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, null, driver.Id);
            var driverAssignment = await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, Shift.Shift1, driver.Id);

            // Act
            var dto = await _dispatchingAppService.CreateSendDispatchMessageDto(orderLine.Id);

            // Assert
            dto.OrderLineTrucks.Count.ShouldBe(1);
            dto.OrderLineTrucks[0].DriverId.ShouldBe(driver.Id);
            dto.OrderLineTrucks[0].TruckId.ShouldBe(truck.Id);
        }

        [Fact]
        public async Task Test_CreateSendDispatchMessageDto_should_not_return_Drivers_for_IsDone_true_Trucks()
        {
            // Arrange
            var today = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(today, Shift.Shift1);
            var orderLine = order.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            orderLineTruck = await UpdateEntity(orderLineTruck, olt => olt.IsDone = true);
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, Shift.Shift1, driver.Id);

            // Act
            var dto = await _dispatchingAppService.CreateSendDispatchMessageDto(orderLine.Id);

            // Assert
            dto.OrderLineTrucks.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_CreateSendDispatchMessageDto_should_not_return_Drivers_for_LeaseHaulers()
        {
            // Arrange
            var today = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(today, Shift.Shift1);
            var orderLine = order.OrderLines.First();
            var dumpTrucks = await CreateVehicleCategory();
            var tractors = await CreateTractorVehicleCategory();
            var truck = await CreateTruck(vehicleCategory: dumpTrucks, alwaysShowOnSchedule: true);
            await SetDefaultDriverForTruck(truck.Id);
            var truck2 = await CreateTruck(vehicleCategory: tractors, alwaysShowOnSchedule: true, canPullTrailer: true);
            await SetDefaultDriverForTruck(truck2.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, truck.DefaultDriverId, orderLine.Id, 1);
            var orderLineTruck2 = await CreateOrderLineTruck(truck2.Id, truck2.DefaultDriverId, orderLine.Id, 1);

            // Act
            var dto = await _dispatchingAppService.CreateSendDispatchMessageDto(orderLine.Id);

            // Assert
            dto.OrderLineTrucks.Count.ShouldBe(0);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_CreateSendDispatchMessageDto_should_return_Drivers_for_LeaseHaulers_when_AllowLeaseHaulersFeature_is_true()
        {
            // Arrange
            SubstituteAllowLeaseHaulersFeature(true);
            var today = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(today, Shift.Shift1);
            var orderLine = order.OrderLines.First();
            var leaseHauler = await CreateLeaseHauler();

            var dumpTrucks = await CreateVehicleCategory();
            var trailers = await CreateTrailerVehicleCategory();
            var tractors = await CreateTractorVehicleCategory();
            var driver = await CreateLeaseHaulerDriver(leaseHauler.Id);
            var truck = await CreateLeaseHaulerTruck(leaseHauler.Id, driver.Id, vehicleCategory: dumpTrucks);
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, Shift.Shift1, driver.Id);

            var driver2 = await CreateLeaseHaulerDriver(leaseHauler.Id);
            var truck2 = await CreateLeaseHaulerTruck(leaseHauler.Id, driver2.Id, vehicleCategory: tractors, canPullTrailer: true);
            await CreateDriverAssignmentForTruck(_officeId, truck2.Id, today, Shift.Shift1, driver2.Id);

            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            var orderLineTruck2 = await CreateOrderLineTruck(truck2.Id, driver2.Id, orderLine.Id, 1);

            // Act
            var dto = await _dispatchingAppService.CreateSendDispatchMessageDto(orderLine.Id);

            // Assert
            dto.OrderLineTrucks.Count.ShouldBe(2);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_CreateSendDispatchMessageDto_should_not_return_Default_Driver_for_Truck_with_DriverAssignment_null_and_existing_DefaultDriver()
        {
            // Arrange
            var today = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(today, Shift.Shift1);
            var orderLine = order.OrderLines.First();
            var truck = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLine.Id, 1);
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, Shift.Shift1, null);

            // Act
            var dto = await _dispatchingAppService.CreateSendDispatchMessageDto(orderLine.Id);

            // Assert
            dto.OrderLineTrucks.Count.ShouldBe(0);
        }

    }
}
