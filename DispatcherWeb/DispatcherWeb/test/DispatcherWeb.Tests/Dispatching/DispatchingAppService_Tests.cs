using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Infrastructure.Sms.Dto;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Dispatching
{
    public class DispatchingAppService_Tests : DispatchingAppService_Tests_Base
    {


        [Fact]
        public async Task Test_CancelDispatch_should_set_Status_Cancel()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Loaded);

            // Act 
            await _dispatchingAppService.CancelDispatch(new CancelDispatchDto
            {
                DispatchId = dispatch.Id,
            });

            // Assert
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Canceled);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_CancelDispatch_should_set_Status_Cancel_and_send_another_dispatch()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Created);
            var dispatch2 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Created);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act 
            await _dispatchingAppService.CancelDispatch(new CancelDispatchDto
            {
                DispatchId = dispatch.Id,
            });

            // Assert
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Canceled);

            var updatedDispatch2 = await GetDispatch(dispatch2.Guid);
            updatedDispatch2.Status.ShouldBe(DispatchStatus.Sent);
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
        }

        [Fact]
        public async Task Test_CancelDispatch_should_set_Status_Cancel_for_all_dispatches_and_dont_send_another_dispatch()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Created);
            var dispatch2 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Created);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act 
            await _dispatchingAppService.CancelDispatch(new CancelDispatchDto
            {
                DispatchId = dispatch.Id,
                CancelAllDispatchesForDriver = true,
            });

            // Assert
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Canceled);

            var updatedDispatch2 = await GetDispatch(dispatch2.Guid);
            updatedDispatch2.Status.ShouldBe(DispatchStatus.Canceled);
            await _smsSender.DidNotReceiveWithAnyArgs().SendAsync(new SmsSendInput());
        }

        [Fact]
        public async Task Test_CancelDispatch_should_throw_UserFriendlyException_when_status_is_completed()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Completed);

            // Act, Assert
            await _dispatchingAppService.CancelDispatch(new CancelDispatchDto
            {
                DispatchId = dispatch.Id,
            }).ShouldThrowAsync(typeof(UserFriendlyException));

        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_CancelDispatches_should_cancel_dispatches_and_send_sms_from_queue()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Created);
            var dispatch2 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Created);
            var dispatch3 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Sent);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act 
            await _dispatchingAppService.CancelDispatches(new CancelDispatchesInput
            {
                OrderLineId = orderLine.Id,
                TruckId = truck.Id,
            });

            // Assert
            var updatedDispatch = await GetDispatch(dispatch.Guid);
            updatedDispatch.Status.ShouldBe(DispatchStatus.Canceled);

            var updatedDispatch2 = await GetDispatch(dispatch2.Guid);
            updatedDispatch2.Status.ShouldBe(DispatchStatus.Canceled);

            var updatedDispatch3 = await GetDispatch(dispatch3.Guid);
            updatedDispatch3.Status.ShouldBe(DispatchStatus.Sent);

            await _smsSender.DidNotReceiveWithAnyArgs().SendAsync(new SmsSendInput());
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_DuplicateDispatch_should_duplicate_existing_Dispatch()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch1 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Completed, "Click this link to acknowledge this dispatch http://test.com/app/acknowledge/nm9zHT0jUECOarWWDET5Ug\nLoad aaa from Location.Deliver to");
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act
            await _dispatchingAppService.DuplicateDispatch(new DuplicateDispatchInput
            {
                DispatchId = dispatch1.Id,
                NumberOfDispatches = 2,
            });

            // Assert
            var dispatches = await UsingDbContextAsync(async context => await context.Dispatches.Where(d => d.Id != dispatch1.Id).OrderBy(d => d.Id).ToListAsync());
            dispatches.Count.ShouldBe(2);
            foreach (var dispatch in dispatches)
            {
                dispatch.TruckId.ShouldBe(dispatch1.TruckId);
                dispatch.DriverId.ShouldBe(dispatch1.DriverId);
                dispatch.OrderLineId.ShouldBe(dispatch1.OrderLineId);
                dispatch.PhoneNumber.ShouldBe(dispatch1.PhoneNumber);
                dispatch.Note.ShouldBe(dispatch1.Note);
            }
            await _smsSender.ReceivedWithAnyArgs().SendAsync(new SmsSendInput());
            dispatches[0].Status.ShouldBe(DispatchStatus.Sent);
            dispatches[0].Guid.ShouldNotBe(dispatch1.Guid);
            dispatches[1].Status.ShouldBe(DispatchStatus.Created);
            dispatches[1].Guid.ShouldNotBe(dispatches[0].Guid);
        }

        [Fact]
        public async Task Test_GetDispatchTruckStatus_should_return_true_when_Truck_has_driver_and_not_IsOutOfService()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch1 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Completed, "Click this link to acknowledge this dispatch http://test.com/app/acknowledge/nm9zHT0jUECOarWWDET5Ug\nLoad aaa from Location.Deliver to");

            // Act
            var result = await _dispatchingAppService.GetDispatchTruckStatus(dispatch1.Id);

            // Assert
            result.ShouldBeTrue();
        }

        [Fact]
        public async Task Test_GetDispatchTruckStatus_should_return_false_when_Truck_has_no_driver_and_not_IsOutOfService()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            await CreateDriverAssignmentForTruck(_officeId, truck.Id, today, null);
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch1 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Completed, "Click this link to acknowledge this dispatch http://test.com/app/acknowledge/nm9zHT0jUECOarWWDET5Ug\nLoad aaa from Location.Deliver to");

            // Act
            var result = await _dispatchingAppService.GetDispatchTruckStatus(dispatch1.Id);

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_GetDispatchTruckStatus_should_return_false_when_Truck_has_driver_and_IsOutOfService()
        {
            // Arrange
            var today = Clock.Now.Date;
            var driver = await CreateDriver();
            var truck = await CreateTruck();
            truck = await UpdateEntity(truck, t => t.IsOutOfService = true);
            var order = await CreateOrderWithOrderLines(today);
            var orderLine = order.OrderLines.First();
            var dispatch1 = await CreateDispatch(truck.Id, driver.Id, orderLine.Id, DispatchStatus.Completed, "Click this link to acknowledge this dispatch http://test.com/app/acknowledge/nm9zHT0jUECOarWWDET5Ug\nLoad aaa from Location.Deliver to");

            // Act
            var result = await _dispatchingAppService.GetDispatchTruckStatus(dispatch1.Id);

            // Assert
            result.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_TruckDispatchList_should_return_List()
        {
            // Arrange
            var today = Clock.Now.Date;
            await CreateDispathes();

            async Task CreateDispathes()
            {
                for (int i = 0; i < 5; i++)
                {
                    await CreateDisptchesForTruckDriver();
                }
                async Task CreateDisptchesForTruckDriver()
                {
                    var truck = await CreateTruck();
                    var driver = await CreateDriver(officeId: _officeId, phoneNumber: "+15005550055");
                    var dispatchStatuses = new[] { DispatchStatus.Sent, DispatchStatus.Canceled, DispatchStatus.Completed };
                    for (var i = 0; i < 10; i++)
                    {
                        await CreateOrdersAndDispatchesForDate(today.AddDays(i), i < dispatchStatuses.Length ? dispatchStatuses[i] : DispatchStatus.Created);
                    }

                    async Task CreateOrdersAndDispatchesForDate(DateTime date, DispatchStatus dispatchStatus)
                    {
                        await CreateDriverAssignmentForTruck(_officeId, truck.Id, date, driver.Id);

                        var order1 = await CreateOrderWithOrderLines(today);
                        var orderLine1 = order1.OrderLines.First();
                        await CreateOrderLineTruck(truck.Id, driver.Id, orderLine1.Id, 1);

                        await CreateDispatch(truck.Id, driver.Id, orderLine1.Id, dispatchStatus);
                    }
                }
            }

            // Act
            var list = await _dispatchingAppService.TruckDispatchList(new TruckDispatchListInput { View = DispatchListViewEnum.OpenDispatches });

            // Assert
            list.Count.ShouldBe(5);
            foreach (var truckDispatchListItemDto in list)
            {
                truckDispatchListItemDto.Dispatches.Count.ShouldBe(8);
            }
        }
    }
}
