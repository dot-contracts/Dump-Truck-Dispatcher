using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Infrastructure.Sms.Dto;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dispatching
{
    public class DispatchingAppService_CompleteDispatch_Tests : DispatchingAppService_Tests_Base
    {
        [Fact]
        public async Task Test_CompleteDispatch_should_set_Status_Complete_and_return_false_when_there_are_no_dispatches()
        {
            // Arrange
            var coords = new { Lat = 50.6995393, Long = 28.6414887 };
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Loaded);

            // Act
            var result = await _dispatchingAppService.CompleteDispatch(new CompleteDispatchDto
            {
                Guid = dispatch.Guid,
                DestinationLatitude = coords.Lat,
                DestinationLongitude = coords.Long,
            });

            // Assert
            result.IsCanceled.ShouldBeFalse();
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Completed);
            updatedDispatch.Loads.Count.ShouldBe(1);
            var load = updatedDispatch.Loads.First();
            load.DestinationLatitude.ShouldBe(coords.Lat);
            load.DestinationLongitude.ShouldBe(coords.Long);
            load.DestinationDateTime.ShouldNotBeNull();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_CompleteDispatch_should_set_Status_Complete_and_return_true_and_send_message_when_there_are_dispatches()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Loaded);
            var dispatch2 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Created);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            var result = await _dispatchingAppService.CompleteDispatch(new CompleteDispatchDto
            {
                Guid = dispatch.Guid,
            });

            // Assert
            result.IsCanceled.ShouldBeFalse();
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Completed);
            updatedDispatch.Loads.Count.ShouldBe(1);
            var load = updatedDispatch.Loads.First();
            load.DestinationDateTime.ShouldNotBeNull();

            var updatedDispatch2 = await GetDispatch(dispatch2.Guid);
            updatedDispatch2.Status.ShouldBe(DispatchStatus.Sent);
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
        }

        [Fact]
        public async Task Test_CompleteDispatch_should_set_Status_Complete_and_return_true_and_do_not_send_message_when_there_are_dispatches_and_Driver_acknowledged_dispatch_today()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Loaded);
            await UpdateEntity(dispatch, d => d.Acknowledged = today);
            var dispatch2 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Created);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            var result = await _dispatchingAppService.CompleteDispatch(new CompleteDispatchDto
            {
                Guid = dispatch.Guid,
            });

            // Assert
            result.IsCanceled.ShouldBeFalse();
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Completed);
            updatedDispatch.Loads.Count.ShouldBe(1);
            var load = updatedDispatch.Loads.First();
            load.DestinationDateTime.ShouldNotBeNull();

            var updatedDispatch2 = await GetDispatch(dispatch2.Guid);
            updatedDispatch2.Status.ShouldBe(DispatchStatus.Created);
            await _smsSender.DidNotReceiveWithAnyArgs().SendAsync(new SmsSendInput());
        }

        [Fact]
        public async Task Test_CompleteDispatch_should_set_Status_Acknowledged_and_return_true_and_do_not_send_message_when_MultipleLoads_is_true()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Loaded);
            await UpdateEntity(dispatch, d => d.IsMultipleLoads = true);
            await UpdateEntity(dispatch, d => d.Acknowledged = today);
            var dispatch2 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Created);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            var result = await _dispatchingAppService.CompleteDispatch(new CompleteDispatchDto
            {
                Guid = dispatch.Guid,
                ContinueMultiload = true,
            });

            // Assert
            result.IsCanceled.ShouldBeFalse();
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Acknowledged);
            updatedDispatch.Loads.Count.ShouldBe(2);
            var load = updatedDispatch.Loads.OrderBy(l => l.Id).First();
            load.DestinationDateTime.ShouldNotBeNull();

            var updatedDispatch2 = await GetDispatch(dispatch2.Guid);
            updatedDispatch2.Status.ShouldBe(DispatchStatus.Created);
            await _smsSender.DidNotReceiveWithAnyArgs().SendAsync(new SmsSendInput());
        }

        [Fact]
        public async Task Test_CompleteDispatch_should_update_last_Load_when_MultipleLoads_is_true()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);
            var previousLoad = await CreateLoad(dispatch.Id, Clock.Now, Clock.Now);
            var currentLoad = await CreateLoad(dispatch.Id, Clock.Now);
            await UpdateEntity(dispatch, d =>
            {
                d.Status = DispatchStatus.Loaded;
                d.IsMultipleLoads = true;
                d.Acknowledged = today;
            });

            // Act
            var result = await _dispatchingAppService.CompleteDispatch(new CompleteDispatchDto
            {
                Guid = dispatch.Guid,
                ContinueMultiload = true,
            });

            // Assert
            result.IsCanceled.ShouldBeFalse();
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Acknowledged);
            updatedDispatch.Loads.Count.ShouldBe(3);
            var load = updatedDispatch.Loads.First(l => l.Id == currentLoad.Id);
            load.DestinationDateTime.ShouldNotBeNull();

        }

        [Fact]
        public async Task Test_CompleteDispatch_should_create_new_Load_when_MultipleLoads_is_true()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);
            var currentLoad = await CreateLoad(dispatch.Id, Clock.Now);
            await UpdateEntity(dispatch, d =>
            {
                d.Status = DispatchStatus.Loaded;
                d.IsMultipleLoads = true;
                d.Acknowledged = today;
            });

            // Act
            var result = await _dispatchingAppService.CompleteDispatch(new CompleteDispatchDto
            {
                Guid = dispatch.Guid,
                ContinueMultiload = true,
            });

            // Assert
            result.IsCanceled.ShouldBeFalse();
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Acknowledged);
            updatedDispatch.Loads.Count.ShouldBe(2);
            var newLoad = updatedDispatch.Loads.First(l => l.Id != currentLoad.Id);
            newLoad.SourceDateTime.ShouldBeNull();
            newLoad.DestinationDateTime.ShouldBeNull();

        }

    }
}
