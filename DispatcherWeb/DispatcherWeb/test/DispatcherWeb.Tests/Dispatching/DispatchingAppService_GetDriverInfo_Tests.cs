using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Dispatching.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dispatching
{
    public class DispatchingAppService_GetDispatchInfo_Tests : DispatchingAppService_Tests_Base
    {
        [Fact]
        public async Task Test_GetDispatchInfo_should_not_return_DriverInfoExpiredDto_when_status_is_Sent_and_pass_24_hours()
        {
            // Arrange
            var date = Clock.Now.Date.AddDays(-3);
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(date);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Sent);
            await UsingDbContextAsync(async context =>
            {
                var d = await context.Dispatches.FindAsync(dispatch.Id);
                d.Sent = Clock.Now.AddDays(-2);
            });

            // Act 
            var result = await _dispatchingAppService.GetDispatchInfo(new GetDispatchInfoInput
            {
                Id = dispatch.Id,
            });

            // Assert
            result.ShouldBeOfType<DispatchLoadInfoDto>();
            var driverLoadInfoDto = (DispatchLoadInfoDto)result;
            driverLoadInfoDto.DispatchStatus.ShouldBe(DispatchStatus.Sent);
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Sent);
        }

        [Fact]
        public async Task Test_GetDispatchInfo_should_return_DriverDestinationInfoDto_when_status_is_Loaded()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Loaded);

            // Act 
            var result = await _dispatchingAppService.GetDispatchInfo(new GetDispatchInfoInput
            {
                Id = dispatch.Id,
            });

            // Assert
            result.ShouldBeOfType<DispatchDestinationInfoDto>();
            var driverDestinationInfoDto = (DispatchDestinationInfoDto)result;
            driverDestinationInfoDto.DispatchStatus.ShouldBe(DispatchStatus.Loaded);
            driverDestinationInfoDto.CustomerAddress.ShouldBe(orderLine.DeliverTo.Name + ". ");
            driverDestinationInfoDto.Note.ShouldBe("Note");
        }

        [Theory]
        [InlineData(DispatchStatus.Sent)]
        [InlineData(DispatchStatus.Acknowledged)]
        [InlineData(DispatchStatus.Loaded)]
        [InlineData(DispatchStatus.Completed)]
        [InlineData(DispatchStatus.Canceled)]
        public async Task Test_GetDispatchInfo_should_return_filled_DriverInfoDto(DispatchStatus dispatchStatus)
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, dispatchStatus);

            // Act 
            var result = await _dispatchingAppService.GetDispatchInfo(new GetDispatchInfoInput
            {
                Id = dispatch.Id,
            });
            // Assert
            var driverDestinationInfoDto = result as DispatchInfoDto;
            driverDestinationInfoDto.ShouldNotBeNull();
            driverDestinationInfoDto.DispatchStatus.ShouldBe(dispatchStatus);
            driverDestinationInfoDto.Guid.ShouldBe(dispatch.Guid);
            driverDestinationInfoDto.TenantId.ShouldBe(dispatch.TenantId);
            driverDestinationInfoDto.IsMultipleLoads.ShouldBe(dispatch.IsMultipleLoads);
        }

        [Fact]
        public async Task Test_GetDispatchInfo_should_return_DriverInfoNotFoundDto_when_there_is_no_guid()
        {
            // Act 
            var result = await _dispatchingAppService.GetDispatchInfo(new GetDispatchInfoInput
            {
                Id = 0,
            });

            // Assert
            result.ShouldBeOfType<DriverInfoNotFoundDto>();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_GetDispatchInfo_should_return_DriverLoadInfoDto_when_status_is_Sent()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            await UpdateEntity(order, o => o.ChargeTo = "Charge to");
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Sent);
            await UpdateEntity(dispatch, d => d.IsMultipleLoads = true);

            // Act 
            var result = await _dispatchingAppService.GetDispatchInfo(new GetDispatchInfoInput
            {
                Id = dispatch.Id,
            });

            // Assert
            result.ShouldBeOfType<DispatchLoadInfoDto>();
            var driverLoadInfoDto = (DispatchLoadInfoDto)result;
            driverLoadInfoDto.DispatchStatus.ShouldBe(DispatchStatus.Sent);
            driverLoadInfoDto.MaterialQuantity.ShouldBe(orderLine.MaterialQuantity);
            driverLoadInfoDto.Note.ShouldBe("Note");
            driverLoadInfoDto.ChargeTo.ShouldBe(order.ChargeTo);
            driverLoadInfoDto.IsMultipleLoads.ShouldBeTrue();

            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Sent);
            updatedDispatch.Acknowledged.ShouldBeNull();
        }

        [Fact]
        public async Task Test_GetDispatchInfo_should_return_DriverLoadInfoDto_when_status_is_Loaded_and_EditTicket_is_true()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Loaded);
            var ticket = await CreateTicketForDispatch(dispatch, order, truck, orderLine);

            // Act 
            var result = await _dispatchingAppService.GetDispatchInfo(new GetDispatchInfoInput
            {
                Id = dispatch.Id,
                EditTicket = true,
            });

            // Assert
            result.ShouldBeOfType<DispatchLoadInfoDto>();
            var driverLoadInfoDto = (DispatchLoadInfoDto)result;
            driverLoadInfoDto.DispatchStatus.ShouldBe(DispatchStatus.Loaded);
            driverLoadInfoDto.LastLoad.LastTicket.TicketNumber.ShouldBe(ticket.TicketNumber);
            driverLoadInfoDto.LastLoad.LastTicket.MaterialQuantity.ShouldBe(ticket.MaterialQuantity);
            ticket.FreightUomId.ShouldNotBeNull();
            driverLoadInfoDto.MaterialUomName.ShouldBe("Hours");
        }

    }
}
