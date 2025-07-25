using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_GetOrderLinesToAssignTrucksToSelectList_Tests : SchedulingAppService_Tests_Base
    {
        [Fact]
        public async Task Test_GetOrderLinesToAssignTrucksToSelectList_should_return_OrderLines_with_same_Date_and_Shift()
        {
            // Arrange
            var date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            await CreateOrderWithOrderLines(date, Shift.Shift2);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);

            // Act
            var orderLineSelectList = await _schedulingAppService.GetOrderLinesToAssignTrucksToSelectList(new GetSelectListIdInput() { Id = orderLine1.Id });

            // Assert
            orderLineSelectList.Items.Count.ShouldBe(1);
            orderLineSelectList.Items[0].Id.ShouldBe(order.OrderLines.First(ol => ol.Id == orderLine2.Id).Id.ToString());
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_GetOrderLinesToAssignTrucksToSelectList_should_not_return_fully_utilized_OrderLines()
        {
            // Arrange
            var date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);
            await UpdateEntity(orderLine2, ol =>
            {
                ol.NumberOfTrucks = .5;
                ol.ScheduledTrucks = .5;
            });
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, orderLine2.Id, .5m);

            // Act
            var orderLineSelectList = await _schedulingAppService.GetOrderLinesToAssignTrucksToSelectList(new GetSelectListIdInput() { Id = orderLine1.Id });

            // Assert
            orderLineSelectList.Items.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_GetOrderLinesToAssignTrucksToSelectList_should_not_return_OrderLines_from_another_Office()
        {
            // Arrange
            var office2 = await CreateOffice();
            var date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date, Shift.Shift1);
            var order2 = await CreateOrderWithOrderLines(date, Shift.Shift1);
            order2 = await UpdateEntity(order2, o => o.Office = office2);
            var orderLine1 = order.OrderLines.First(ol => ol.LineNumber == 1);
            var orderLine2 = order.OrderLines.First(ol => ol.LineNumber == 2);

            // Act
            var orderLineSelectList = await _schedulingAppService.GetOrderLinesToAssignTrucksToSelectList(new GetSelectListIdInput() { Id = orderLine1.Id });

            // Assert
            orderLineSelectList.Items.Count.ShouldBe(1);
            orderLineSelectList.Items[0].Id.ShouldBe(order.OrderLines.First(ol => ol.Id == orderLine2.Id).Id.ToString());
        }
    }
}
