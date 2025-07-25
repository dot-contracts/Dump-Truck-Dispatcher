using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.UI;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Scheduling.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Orders
{
    public class OrderAppService_SetOrderOfficeId_Tests : AppTestBase, IAsyncLifetime
    {
        private IOrderAppService _orderAppService;
        private int _officeId;
        private Order _order;
        private Office _office2;
        private OrderLine _orderLine;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _orderAppService = Resolve<IOrderAppService>();

            _order = await CreateOrderWithOrderLines();
            _orderLine = _order.OrderLines.First();
            _office2 = await CreateOffice();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SetOrderOfficeId_should_throw_UserFriendlyException_when_Order_has_completed_OrderLine_and_transfer_entire_order()
        {
            // Arrange
            await CompleteOrderLine(_orderLine.Id);

            // Act, Assert
            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OfficeId = _office2.Id,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SetOrderOfficeId_should_throw_UserFriendlyException_when_Order_has_completed_OrderLine_and_transfer_this_OrderLine()
        {
            // Arrange
            await CompleteOrderLine(_orderLine.Id);

            // Act, Assert
            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OfficeId = _office2.Id,
                OrderLineId = _orderLine.Id,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_SetOrderOfficeId_should_transfer_OrderLine_when_Order_has_completed_OrderLine_and_transfer_another_OrderLine()
        {
            // Arrange
            await CompleteOrderLine(_orderLine.Id);
            var anotherOrderLineId = _order.OrderLines.First(ol => ol.Id != _orderLine.Id).Id;
            anotherOrderLineId.ShouldNotBe(_orderLine.Id);

            // Act
            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OfficeId = _office2.Id,
                OrderLineId = anotherOrderLineId,
            });

            // Assert
            var transferedOrder = await UsingDbContextAsync(async context => await context.Orders.Include(o => o.OrderLines).FirstAsync(o => o.OfficeId == _office2.Id));
            transferedOrder.OrderLines.Count.ShouldBe(1);
            transferedOrder.OrderLines.First().Id.ShouldBe(anotherOrderLineId);
        }


        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SetOrderOfficeId_should_throw_UserFriendlyException_when_Order_has_OrderLine_with_Tickets_and_transfer_entire_order()
        {
            // Arrange
            await AddTicketToOrderLine(_orderLine.Id);

            // Act, Assert
            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OfficeId = _office2.Id,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SetOrderOfficeId_should_throw_UserFriendlyException_when_Order_has_OrderLine_with_Tickets_and_transfer_this_OrderLine()
        {
            // Arrange
            await AddTicketToOrderLine(_orderLine.Id);

            // Act, Assert
            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OfficeId = _office2.Id,
                OrderLineId = _orderLine.Id,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_SetOrderOfficeId_should_transfer_OrderLine_when_Order_has_OrderLine_with_Tickets_and_transfer_another_OrderLine()
        {
            // Arrange
            await AddTicketToOrderLine(_orderLine.Id);
            var anotherOrderLineId = _order.OrderLines.First(ol => ol.Id != _orderLine.Id).Id;
            anotherOrderLineId.ShouldNotBe(_orderLine.Id);

            // Act
            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OfficeId = _office2.Id,
                OrderLineId = anotherOrderLineId,
            });

            // Assert
            var transferedOrder = await UsingDbContextAsync(async context => await context.Orders.Include(o => o.OrderLines).FirstAsync(o => o.OfficeId == _office2.Id));
            transferedOrder.OrderLines.Count.ShouldBe(1);
            transferedOrder.OrderLines.First().Id.ShouldBe(anotherOrderLineId);
        }


        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SetOrderOfficeId_should_throw_UserFriendlyException_when_Order_has_OrderLine_with_OrderLineTruck_and_transfer_entire_order()
        {
            // Arrange
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, _orderLine.Id, 1);

            // Act, Assert
            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OfficeId = _office2.Id,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_SetOrderOfficeId_should_throw_UserFriendlyException_when_Order_has_OrderLine_with_OrderLineTruck_and_transfer_this_OrderLine()
        {
            // Arrange
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, _orderLine.Id, 1);

            // Act, Assert
            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OfficeId = _office2.Id,
                OrderLineId = _orderLine.Id,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact]
        public async Task Test_SetOrderOfficeId_should_transfer_OrderLine_when_Order_has_OrderLine_with_OrderLineTruck_and_transfer_another_OrderLine()
        {
            // Arrange
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await CreateOrderLineTruck(truck.Id, driver.Id, _orderLine.Id, 1);
            var anotherOrderLineId = _order.OrderLines.First(ol => ol.Id != _orderLine.Id).Id;
            anotherOrderLineId.ShouldNotBe(_orderLine.Id);

            // Act
            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OfficeId = _office2.Id,
                OrderLineId = anotherOrderLineId,
            });

            // Assert
            var transferedOrder = await UsingDbContextAsync(async context => await context.Orders.Include(o => o.OrderLines).FirstAsync(o => o.OfficeId == _office2.Id));
            transferedOrder.OrderLines.Count.ShouldBe(1);
            transferedOrder.OrderLines.First().Id.ShouldBe(anotherOrderLineId);
        }


        [Fact]
        public async Task Test_SetOrderOfficeId_should_transfer_entrire_Order_when_Order_has_no_any_prerequisites()
        {
            // Arrange

            // Act
            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OfficeId = _office2.Id,
            });

            // Assert
            var transferedOrder = await UsingDbContextAsync(async context => await context.Orders.Include(o => o.OrderLines).FirstAsync(o => o.OfficeId == _office2.Id));
            transferedOrder.OrderLines.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Test_SetOrderOfficeId_should_update_OfficeId_when_transfer_entire_order()
        {
            var secondOffice = await CreateOffice();

            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OfficeId = secondOffice.Id,
            });

            var orderEntityResult = await UsingDbContextAsync(async context =>
            {
                return await context.Orders
                    .Include(o => o.OrderLines)
                    .FirstAsync(o => o.Id == _order.Id);
            });
            orderEntityResult.Id.ShouldBe(_order.Id);
            orderEntityResult.OfficeId.ShouldNotBe(_order.OfficeId);
            orderEntityResult.OfficeId.ShouldBe(secondOffice.Id);
        }

        [Fact]
        public async Task Test_SetOrderOfficeId_should_update_OfficeId_and_delete_OrderLines_with_0_MaterialQuantity_0_NumberOfTrucks_when_transfer_entire_order()
        {
            // Arrange
            var secondOffice = await CreateOffice();
            await UsingDbContextAsync(async context =>
            {
                var ol = new OrderLine
                {
                    TenantId = 1,
                    OrderId = _order.Id,
                    LineNumber = 3,
                    Designation = DesignationEnum.FreightAndMaterial,
                    MaterialQuantity = 0,
                    FreightPricePerUnit = 2,
                    MaterialPricePerUnit = 3,
                    FreightItemId = _order.OrderLines.First().FreightItemId,
                    LoadAtId = _order.OrderLines.First().LoadAtId,
                    MaterialUomId = 1,
                    NumberOfTrucks = 0,
                };
                await context.OrderLines.AddAsync(ol);

            });

            var order2 = await CreateOrderWithOrderLines();
            await UpdateEntity(order2.OrderLines.First(), ol => ol.MaterialQuantity = 0);

            // Act
            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OfficeId = secondOffice.Id,
            });

            // Assert
            var updatedOrder = await UsingDbContextAsync(async context => await context.Orders.Include(o => o.OrderLines).FirstAsync(o => o.Id == _order.Id));
            updatedOrder.Id.ShouldBe(_order.Id);
            updatedOrder.OfficeId.ShouldNotBe(_order.OfficeId);
            updatedOrder.OfficeId.ShouldBe(secondOffice.Id);
            updatedOrder.OrderLines.Count(ol => !ol.IsDeleted).ShouldBe(2);

            var updatedOrder2 = await UsingDbContextAsync(async context => await context.Orders.Include(o => o.OrderLines).FirstAsync(o => o.Id == order2.Id));
            updatedOrder2.OrderLines.Count(ol => !ol.IsDeleted).ShouldBe(2);
        }

        [Fact]
        public async Task Test_SetOrderOfficeId_should_update_OfficeId_and_should_not_delete_OrderLines_with_0_MaterialQuantity_and_not_0_NumberOfTrucks_when_transfer_entire_order()
        {
            // Arrange
            var secondOffice = await CreateOffice();
            var orderLine = await UsingDbContextAsync(async context =>
            {
                var ol = new OrderLine
                {
                    TenantId = 1,
                    OrderId = _order.Id,
                    LineNumber = 3,
                    Designation = DesignationEnum.FreightAndMaterial,
                    MaterialQuantity = 0,
                    FreightPricePerUnit = 2,
                    MaterialPricePerUnit = 3,
                    FreightItemId = _order.OrderLines.First().FreightItemId,
                    LoadAtId = _order.OrderLines.First().LoadAtId,
                    MaterialUomId = 1,
                    NumberOfTrucks = 2,
                };
                await context.OrderLines.AddAsync(ol);
                return ol;
            });

            // Act
            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OfficeId = secondOffice.Id,
            });

            // Assert
            var updatedOrder = await UsingDbContextAsync(async context => await context.Orders.Include(o => o.OrderLines).FirstAsync(o => o.Id == _order.Id));
            updatedOrder.Id.ShouldBe(_order.Id);
            updatedOrder.OfficeId.ShouldNotBe(_order.OfficeId);
            updatedOrder.OfficeId.ShouldBe(secondOffice.Id);
            var updatedOrderLine = updatedOrder.OrderLines.First(ol => ol.Id == orderLine.Id);
            updatedOrderLine.IsDeleted.ShouldBeFalse();
        }

        [Fact]
        public async Task Test_SetOrderOfficeId_should_copy_Office_and_move_OrderLine_when_transfer_only_one_OrderLine()
        {
            _order.OrderLines.Count.ShouldBe(2);
            var secondOffice = await CreateOffice();

            await _orderAppService.SetOrderOfficeId(new SetOrderOfficeIdInput
            {
                OrderId = _order.Id,
                OrderLineId = _order.OrderLines.First().Id,
                OfficeId = secondOffice.Id,
            });

            var orderEntityResult = await UsingDbContextAsync(async context =>
            {
                return await context.Orders
                    .Include(o => o.OrderLines)
                    .FirstOrDefaultAsync(o => o.Id != _order.Id);
            });
            orderEntityResult.ShouldNotBeNull();
            orderEntityResult.Id.ShouldNotBe(_order.Id);
            orderEntityResult.OfficeId.ShouldNotBe(_order.OfficeId);
            orderEntityResult.OfficeId.ShouldBe(secondOffice.Id);
            orderEntityResult.OrderLines.Count.ShouldBe(1);
            orderEntityResult.OrderLines.First().Id.ShouldBe(_order.OrderLines.First().Id);

            var orderEntityOriginal = await UsingDbContextAsync(async context =>
            {
                return await context.Orders
                    .Include(o => o.OrderLines)
                    .FirstAsync(o => o.Id == _order.Id);
            });
            orderEntityOriginal.OrderLines.Count.ShouldBe(1);
        }

        private async Task AddTicketToOrderLine(int orderLineId)
        {
            await UsingDbContextAsync(async context =>
            {
                var orderLine = await context.OrderLines.FindAsync(orderLineId);
                orderLine.Tickets = new List<Ticket> {
                    new Ticket
                    {
                        TenantId = 1,
                        OfficeId = _office2.Id,
                    },
                };
            });
        }

        private async Task CompleteOrderLine(int orderLineId)
        {
            await UsingDbContextAsync(async context =>
            {
                var orderLine = await context.OrderLines.FindAsync(orderLineId);
                orderLine.IsComplete = true;
            });
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

    }
}
