using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Scheduling;
using DispatcherWeb.Scheduling.Dto;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_SetOrderLineQuantity_Tests : AppTestBase, IAsyncLifetime
    {
        private ISchedulingAppService _schedulingAppService;
        private ISmsSender _smsSender;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _smsSender = Substitute.For<ISmsSender>();
            var dispatchingAppService = Resolve<IDispatchingAppService>(new { smsSender = _smsSender });
            _schedulingAppService = Resolve<ISchedulingAppService>(new { smsSender = _smsSender, dispatchingAppService = dispatchingAppService });
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SetOrderLineQuantity_should_update_price_fields()
        {
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrderWithOrderLines(date);

            var orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            var input = new SetOrderLineMaterialQuantityInput
            {
                OrderLineId = orderLineId,
                MaterialQuantity = 10,
            };
            await _schedulingAppService.SetOrderLineMaterialQuantity(input);

            var orderEntityResult = await UsingDbContextAsync(async context =>
            {
                return await context.Orders
                    .Include(o => o.OrderLines)
                    .FirstAsync(o => o.Id == orderEntity.Id);
            });
            orderEntityResult.Id.ShouldBe(orderEntity.Id);
            orderEntityResult.FreightTotal.ShouldBe(20 + 2 * 20); // 1st OrderLine + 2nd OrderLine
            orderEntityResult.MaterialTotal.ShouldBe(30 + 2 * 30);
            orderEntityResult.SalesTax.ShouldBe(3);
            orderEntityResult.CODTotal.ShouldBe(153);
            var orderLine = orderEntityResult.OrderLines.First(ol => ol.Id == orderLineId);
            orderLine.FreightPrice.ShouldBe(20);
            orderLine.MaterialPrice.ShouldBe(30);
        }

        [Fact]
        public async Task Test_SetOrderLineQuantity_should_not_update_Totals_if_price_is_overridden()
        {
            // Arrange
            var date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);

            var orderLine = order.OrderLines.First();
            await UpdateEntity(orderLine, ol =>
            {
                ol.IsFreightPriceOverridden = true;
                ol.IsMaterialPriceOverridden = true;
                ol.FreightPrice = 11;
                ol.MaterialPrice = 22;
            });
            var input = new SetOrderLineMaterialQuantityInput
            {
                OrderLineId = orderLine.Id,
                MaterialQuantity = 10,
            };

            // Act
            await _schedulingAppService.SetOrderLineMaterialQuantity(input);

            // Assert
            var updatedOrder = await UsingDbContextAsync(async context =>
            {
                return await context.Orders
                    .Include(o => o.OrderLines)
                    .FirstAsync(o => o.Id == order.Id);
            });
            updatedOrder.Id.ShouldBe(order.Id);
            updatedOrder.FreightTotal.ShouldBe(order.OrderLines.Sum(ol => ol.FreightPrice));
            updatedOrder.MaterialTotal.ShouldBe(order.OrderLines.Sum(ol => ol.MaterialPrice));
            var updatedOrderLine = updatedOrder.OrderLines.First(ol => ol.Id == orderLine.Id);
            updatedOrderLine.FreightPrice.ShouldBe(orderLine.FreightPrice);
            updatedOrderLine.MaterialPrice.ShouldBe(orderLine.MaterialPrice);
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SetOrderLineQuantity_should_throw_UserFriendlyException_for_Order_from_another_office()
        {
            var date = Clock.Now.Date;
            var orderEntity = await CreateOrderWithOrderLines(date);
            var office2 = await CreateOffice();
            await ChangeOrderOffice(orderEntity.Id, office2.Id);
            var orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;

            var input = new SetOrderLineMaterialQuantityInput
            {
                OrderLineId = orderLineId,
                MaterialQuantity = 10,
            };
            await _schedulingAppService.SetOrderLineMaterialQuantity(input).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_SetOrderLineQuantity_should_throw_UserFriendlyException_when_Order_is_paid()
        {
            // Arrange
            var date = Clock.Now.Date;
            var order = await CreateOrderWithOrderLines(date);
            var payment = await CreateOrderPayment(order);
            await UpdateEntity(payment, p => p.AuthorizationCaptureDateTime = DateTime.UtcNow);
            var orderLineId = order.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;

            var input = new SetOrderLineMaterialQuantityInput
            {
                OrderLineId = orderLineId,
                MaterialQuantity = 10,
            };

            // Act, Assert
            await _schedulingAppService.SetOrderLineMaterialQuantity(input).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
