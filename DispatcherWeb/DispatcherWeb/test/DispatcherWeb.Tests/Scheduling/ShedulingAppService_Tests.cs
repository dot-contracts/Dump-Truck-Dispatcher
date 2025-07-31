using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Infrastructure.Sms.Dto;
using DispatcherWeb.Scheduling.Dto;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;
using System.Collections.Generic;

namespace DispatcherWeb.Tests.Scheduling
{
    public class ShedulingAppService_Tests : SchedulingAppService_Tests_Base
    {


        [Fact]
        public async Task Test_HasDispatches_should_return_Result_with_Unacknowledged_is_true_when_there_is_Created_dispatch()
        {
            // Assign
            var input = await CreateHasDispatchesTestData(DispatchStatus.Created);

            // Act
            var result = await _schedulingAppService.HasDispatches(input);

            // Assert
            result.Unacknowledged.ShouldBeTrue();
            result.AcknowledgedOrLoaded.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_HasDispatches_should_return_Result_with_Unacknowledged_is_true_when_there_is_Sent_dispatch()
        {
            // Assign
            var input = await CreateHasDispatchesTestData(DispatchStatus.Sent);

            // Act
            var result = await _schedulingAppService.HasDispatches(input);

            // Assert
            result.Unacknowledged.ShouldBeTrue();
            result.AcknowledgedOrLoaded.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_HasDispatches_should_return_Result_with_AcknowledgedOrLoaded_is_true_when_there_is_Acknowledged_dispatch()
        {
            // Assign
            var input = await CreateHasDispatchesTestData(DispatchStatus.Acknowledged);

            // Act
            var result = await _schedulingAppService.HasDispatches(input);

            // Assert
            result.Unacknowledged.ShouldBeFalse();
            result.AcknowledgedOrLoaded.ShouldBeTrue();
        }

        [Fact]
        public async Task Test_HasDispatches_should_return_Result_with_AcknowledgedOrLoaded_is_true_when_there_is_Loaded_dispatch()
        {
            // Assign
            var input = await CreateHasDispatchesTestData(DispatchStatus.Loaded);

            // Act
            var result = await _schedulingAppService.HasDispatches(input);

            // Assert
            result.Unacknowledged.ShouldBeFalse();
            result.AcknowledgedOrLoaded.ShouldBeTrue();
        }

        [Fact]
        public async Task Test_HasDispatches_should_return_Result_with_Unacknowledged_and_AcknowledgedOrLoaded_are_false_when_there_is_Completed_dispatch()
        {
            // Assign
            var input = await CreateHasDispatchesTestData(DispatchStatus.Completed);

            // Act
            var result = await _schedulingAppService.HasDispatches(input);

            // Assert
            result.Unacknowledged.ShouldBeFalse();
            result.AcknowledgedOrLoaded.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_HasDispatches_should_return_Result_with_Unacknowledged_and_AcknowledgedOrLoaded_are_false_when_there_is_Canceled_dispatch()
        {
            // Assign
            var input = await CreateHasDispatchesTestData(DispatchStatus.Canceled);

            // Act
            var result = await _schedulingAppService.HasDispatches(input);

            // Assert
            result.Unacknowledged.ShouldBeFalse();
            result.AcknowledgedOrLoaded.ShouldBeFalse();
        }

        private async Task<DeleteOrderLineTruckInput> CreateHasDispatchesTestData(DispatchStatus status)
        {
            var today = Clock.Now.Date;
            var orderEntity = await CreateOrder(today);
            var orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            var truck = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            var orderLineTruck = await CreateOrderLineTruck(truck.Id, driver.Id, orderLineId, 1);
            var dispatch = await CreateDispatch(truck.Id, driver.Id, orderLineId, status);
            return new DeleteOrderLineTruckInput
            {
                OrderLineTruckId = orderLineTruck.Id,
            };
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SetOrderLineLoads_should_throw_UserFriendlyException_for_Order_from_another_office()
        {
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrder(date);
            var office2 = await CreateOffice();
            await ChangeOrderOffice(orderEntity.Id, office2.Id);
            var orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;

            await _schedulingAppService.SetOrderLineLoads(new SetOrderLineLoadsInput
            {
                OrderLineId = orderLineId,
                Loads = 1,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_SetOrderLineIsComplete_should_set_IsComplete_and_delete_OrderLineTrucks()
        {
            var originalDate = DateTime.Today;
            var orderEntity = await CreateOrder(originalDate);
            var orderLine = orderEntity.OrderLines.First();
            var truckEntity = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truckEntity.Id, driver.Id, orderLine.Id, 1m);

            // Act
            await _schedulingAppService.SetOrderLineIsComplete(new SetOrderLineIsCompleteInput
            {
                IsComplete = true,
                OrderLineId = orderLine.Id,
            });

            // Assert
            var orderLineTrucks = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => olt.OrderLineId == orderLine.Id && !olt.IsDeleted).ToListAsync());
            orderLineTrucks.Count.ShouldBe(1);
            orderLineTrucks[0].IsDone.ShouldBeTrue();
            orderLineTrucks[0].Utilization.ShouldBe(0);
            var updatedOrderLine = await UsingDbContextAsync(async context => await context.OrderLines.Where(ol => ol.Id == orderLine.Id && !ol.IsDeleted).FirstAsync());
            updatedOrderLine.IsComplete.ShouldBeTrue();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SetOrderLineIsComplete_should_throw_ApplicationExcepiton_when_there_is_sent_dispatch()
        {
            var originalDate = DateTime.Today;
            var orderEntity = await CreateOrder(originalDate);
            var orderLine = orderEntity.OrderLines.First();
            var truckEntity = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truckEntity.Id);
            await CreateOrderLineTruck(truckEntity.Id, driver.Id, orderLine.Id, 1m);
            var dispatchEntity = await CreateDispatch(truckEntity.Id, driver.Id, orderLine.Id, DispatchStatus.Sent);

            // Act, Assert
            await _schedulingAppService.SetOrderLineIsComplete(new SetOrderLineIsCompleteInput
            {
                IsComplete = true,
                OrderLineId = orderLine.Id,
            }).ShouldThrowAsync(typeof(ApplicationException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SetOrderLineIsComplete_should_throw_ApplicationExcepiton_when_there_is_acknowledged_dispatch()
        {
            var originalDate = DateTime.Today;
            var orderEntity = await CreateOrder(originalDate);
            var orderLine = orderEntity.OrderLines.First();
            var truckEntity = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truckEntity.Id);
            await CreateOrderLineTruck(truckEntity.Id, driver.Id, orderLine.Id, 1m);
            var dispatchEntity = await CreateDispatch(truckEntity.Id, driver.Id, orderLine.Id, DispatchStatus.Acknowledged);
            _smsSender.SendAsync(new SmsSendInput()).ReturnsForAnyArgs(new SmsSendResult("12345", SmsStatus.Sent, null, null));

            // Act, Assert
            await _schedulingAppService.SetOrderLineIsComplete(new SetOrderLineIsCompleteInput
            {
                IsComplete = true,
                OrderLineId = orderLine.Id,
            }).ShouldThrowAsync(typeof(ApplicationException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SetOrderLineIsComplete_should_throw_ApplicationExcepiton_when_there_is_loaded_dispatch()
        {
            var originalDate = DateTime.Today;
            var orderEntity = await CreateOrder(originalDate);
            var orderLine = orderEntity.OrderLines.First();
            var truckEntity = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truckEntity.Id);
            await CreateOrderLineTruck(truckEntity.Id, driver.Id, orderLine.Id, 1m);
            var dispatchEntity = await CreateDispatch(truckEntity.Id, driver.Id, orderLine.Id, DispatchStatus.Loaded);

            // Act, Assert
            await _schedulingAppService.SetOrderLineIsComplete(new SetOrderLineIsCompleteInput
            {
                IsComplete = true,
                OrderLineId = orderLine.Id,
            }).ShouldThrowAsync(typeof(ApplicationException));
        }

        [Fact]
        public async Task Test_SetOrderLineIsComplete_should_not_cancel_completed_dispatch()
        {
            var originalDate = DateTime.Today;
            var orderEntity = await CreateOrder(originalDate);
            var orderLine = orderEntity.OrderLines.First();
            var truckEntity = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truckEntity.Id);
            await CreateOrderLineTruck(truckEntity.Id, driver.Id, orderLine.Id, 1m);
            var dispatchEntity = await CreateDispatch(truckEntity.Id, driver.Id, orderLine.Id, DispatchStatus.Completed);

            // Act
            await _schedulingAppService.SetOrderLineIsComplete(new SetOrderLineIsCompleteInput
            {
                IsComplete = true,
                OrderLineId = orderLine.Id,
            });

            // Assert
            var updatedDispatch = await UsingDbContextAsync(async context => await context.Dispatches.FindAsync(dispatchEntity.Id));
            updatedDispatch.Status.ShouldBe(DispatchStatus.Completed);
        }

        [Fact]
        public async Task Test_SetOrderLineIsCompleteBatch_should_set_multiple_order_lines_complete()
        {
            // Arrange
            var originalDate = DateTime.Today;
            var orderEntity1 = await CreateOrder(originalDate);
            var orderEntity2 = await CreateOrder(originalDate);
            var orderLine1 = orderEntity1.OrderLines.First();
            var orderLine2 = orderEntity2.OrderLines.First();
            
            var truckEntity1 = await CreateTruck();
            var truckEntity2 = await CreateTruck();
            var driver1 = await CreateDriver();
            var driver2 = await CreateDriver();
            
            await CreateOrderLineTruck(truckEntity1.Id, driver1.Id, orderLine1.Id, 1m);
            await CreateOrderLineTruck(truckEntity2.Id, driver2.Id, orderLine2.Id, 1m);

            // Act
            await _schedulingAppService.SetOrderLineIsCompleteBatch(new SetOrderLineIsCompleteBatchInput
            {
                OrderLineIds = new List<int> { orderLine1.Id, orderLine2.Id },
                IsComplete = true,
                IsCancelled = false,
            });

            // Assert
            var updatedOrderLine1 = await UsingDbContextAsync(async context => await context.OrderLines.Where(ol => ol.Id == orderLine1.Id && !ol.IsDeleted).FirstAsync());
            var updatedOrderLine2 = await UsingDbContextAsync(async context => await context.OrderLines.Where(ol => ol.Id == orderLine2.Id && !ol.IsDeleted).FirstAsync());
            
            updatedOrderLine1.IsComplete.ShouldBeTrue();
            updatedOrderLine2.IsComplete.ShouldBeTrue();
            
            var orderLineTrucks1 = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => olt.OrderLineId == orderLine1.Id && !olt.IsDeleted).ToListAsync());
            var orderLineTrucks2 = await UsingDbContextAsync(async context => await context.OrderLineTrucks.Where(olt => olt.OrderLineId == orderLine2.Id && !olt.IsDeleted).ToListAsync());
            
            orderLineTrucks1.Count.ShouldBe(1);
            orderLineTrucks2.Count.ShouldBe(1);
            orderLineTrucks1[0].IsDone.ShouldBeTrue();
            orderLineTrucks2[0].IsDone.ShouldBeTrue();
            orderLineTrucks1[0].Utilization.ShouldBe(0);
            orderLineTrucks2[0].Utilization.ShouldBe(0);
        }

        [Fact]
        public async Task Test_SetOrderLineIsCompleteBatch_should_throw_exception_for_empty_order_line_ids()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _schedulingAppService.SetOrderLineIsCompleteBatch(new SetOrderLineIsCompleteBatchInput
                {
                    OrderLineIds = new List<int>(),
                    IsComplete = true,
                    IsCancelled = false,
                });
            });
        }

        [Fact]
        public async Task Test_SetOrderLineIsCompleteBatch_should_throw_exception_for_null_order_line_ids()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _schedulingAppService.SetOrderLineIsCompleteBatch(new SetOrderLineIsCompleteBatchInput
                {
                    OrderLineIds = null,
                    IsComplete = true,
                    IsCancelled = false,
                });
            });
        }

    }
}
