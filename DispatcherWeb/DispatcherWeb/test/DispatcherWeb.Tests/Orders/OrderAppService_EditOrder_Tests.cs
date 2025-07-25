using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Customers;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Orders
{
    public class OrderAppService_EditOrder_Tests : AppTestBase, IAsyncLifetime
    {
        private IOrderAppService _orderAppService;
        private int _officeId;
        private Order _order;
        private int _orderLineId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _orderAppService = Resolve<IOrderAppService>();

            var today = Clock.Now.Date;
            _order = await CreateOrderWithOrderLines(today);
            _order.OrderLines.Count.ShouldBe(2);
            _orderLineId = _order.OrderLines.First().Id;
        }

        [Fact]
        public async Task Test_EditOrder_should_change_Status_to_IsPending_is_true_and_there_are_no_any_prerequisites()
        {
            // Assign
            await SetOrderLineNumberOfTrucks(_orderLineId, 1);

            // Act
            await _orderAppService.EditOrder(new OrderEditDto
            {
                IsPending = true,
                Id = _order.Id,
                DeliveryDate = _order.DeliveryDate,
                CustomerId = _order.CustomerId,
                OfficeId = _order.OfficeId,
                SalesTaxRate = _order.SalesTaxRate,
            });

            var order = await UsingDbContextAsync(async context => await context.Orders.FindAsync(_order.Id));
            order.IsPending.ShouldBeTrue();
        }

        [Fact]
        public async Task Test_EditOrder_should_throw_UserFriendlyException_when_IsPending_is_true_and_there_are_OrderLineTrucks()
        {
            // Assign
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await SetOrderLineNumberOfTrucks(_orderLineId, 1);
            await CreateOrderLineTruck(truck.Id, driver.Id, _orderLineId, 1);
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(olt => !olt.IsDeleted && olt.OrderLineId == _orderLineId).ToListAsync()
            );
            orderLineTrucks.Count.ShouldBe(1);

            // Act
            await _orderAppService.EditOrder(new OrderEditDto
            {
                IsPending = true,
                Id = _order.Id,
                DeliveryDate = _order.DeliveryDate,
                CustomerId = _order.CustomerId,
                OfficeId = _order.OfficeId,
                SalesTaxRate = _order.SalesTaxRate,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditOrder_should_throw_UserFriendlyException_when_changing_office_and_Order_has_tickets()
        {
            // Assign
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await SetOrderLineNumberOfTrucks(_orderLineId, 1);
            await CreateOrderLineTruck(truck.Id, driver.Id, _orderLineId, 1);
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(olt => !olt.IsDeleted && olt.OrderLineId == _orderLineId).ToListAsync()
            );
            orderLineTrucks.Count.ShouldBe(1);
            await UsingDbContextAsync(async context =>
            {
                var orderLine = await context.OrderLines.FindAsync(_orderLineId);
                orderLine.Tickets = new List<Ticket> {
                    new Ticket
                    {
                        TenantId = 1,
                        OfficeId = _officeId,
                    },
                };
            });
            var office2 = await CreateOffice();

            // Act
            await _orderAppService.EditOrder(new OrderEditDto
            {
                Id = _order.Id,
                DeliveryDate = _order.DeliveryDate,
                CustomerId = _order.CustomerId,
                OfficeId = office2.Id,
                SalesTaxRate = _order.SalesTaxRate,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_EditOrder_should_throw_UserFriendlyException_when_changing_Customer_and_OrderLine_has_dispatches()
        {
            // Assign
            var truck = await CreateTruck();
            var driver = await SetDefaultDriverForTruck(truck.Id);
            await SetOrderLineNumberOfTrucks(_orderLineId, 1);
            await CreateOrderLineTruck(truck.Id, driver.Id, _orderLineId, 1);
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(olt => !olt.IsDeleted && olt.OrderLineId == _orderLineId).ToListAsync()
            );
            orderLineTrucks.Count.ShouldBe(1);
            await CreateDispatch(truck.Id, driver.Id, _orderLineId, DispatchStatus.Sent);
            var customer2 = await UsingDbContextAsync(async context =>
            {
                var c = new Customer() { TenantId = 1, Name = "Cust2" };
                await context.Customers.AddAsync(c);
                return c;
            });

            // Act
            await _orderAppService.EditOrder(new OrderEditDto
            {
                Id = _order.Id,
                DeliveryDate = _order.DeliveryDate,
                CustomerId = customer2.Id,
                OfficeId = _order.OfficeId,
                SalesTaxRate = _order.SalesTaxRate,
            }).ShouldThrowAsync(typeof(UserFriendlyException));
        }

        [Fact(Skip = "#7551 requires to prevent trasferring an order in this case")]
        public async Task Test_EditOrder_should_delete_OrderLineTrucks_when_Office_is_changed()
        {
            // Assign
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await SetOrderLineNumberOfTrucks(_orderLineId, 1);
            await CreateOrderLineTruck(truck.Id, driver.Id, _orderLineId, 1);
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(olt => !olt.IsDeleted && olt.OrderLineId == _orderLineId).ToListAsync()
            );
            orderLineTrucks.Count.ShouldBe(1);
            var office2 = await CreateOffice();

            // Act
            await _orderAppService.EditOrder(new OrderEditDto
            {
                OfficeId = office2.Id,
                Id = _order.Id,
                DeliveryDate = _order.DeliveryDate,
                CustomerId = _order.CustomerId,
                SalesTaxRate = _order.SalesTaxRate,
            });


            orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(olt => !olt.IsDeleted && olt.OrderLineId == _orderLineId).ToListAsync()
            );
            orderLineTrucks.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Test_EditOrder_should_not_delete_OrderLineTrucks_when_Office_is_not_changed()
        {
            // Assign
            var truck = await CreateTruck();
            var driver = await CreateDriver();
            await SetOrderLineNumberOfTrucks(_orderLineId, 1);
            await CreateOrderLineTruck(truck.Id, driver.Id, _orderLineId, 1);
            var orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(olt => !olt.IsDeleted && olt.OrderLineId == _orderLineId).ToListAsync()
            );
            orderLineTrucks.Count.ShouldBe(1);

            // Act
            await _orderAppService.EditOrder(new OrderEditDto
            {
                Id = _order.Id,
                DeliveryDate = _order.DeliveryDate,
                CustomerId = _order.CustomerId,
                OfficeId = _order.OfficeId,
                SalesTaxRate = _order.SalesTaxRate,
            });

            // Assert
            orderLineTrucks = await UsingDbContextAsync(async context =>
                await context.OrderLineTrucks.Where(olt => !olt.IsDeleted && olt.OrderLineId == _orderLineId).ToListAsync()
            );
            orderLineTrucks.Count.ShouldBe(1);
        }

        [Fact]
        public async Task Test_EditOrder_should_not_update_TimeOnJob_of_OrderLine_with_not_empty_TimeOnJob()
        {
            // Arrange
            var timeOnJob = Clock.Now.Date.AddHours(15).AddMinutes(37);
            var olTimeOnJob = Clock.Now.Date.AddHours(1).AddMinutes(11);
            var orderLines = _order.OrderLines.ToList();
            await UpdateEntity(orderLines[0], ol => ol.TimeOnJob = olTimeOnJob);
            await UpdateEntity(orderLines[1], ol => ol.TimeOnJob = olTimeOnJob);

            // Act
            await _orderAppService.EditOrder(new OrderEditDto
            {
                Id = _order.Id,
                DeliveryDate = _order.DeliveryDate,
                CustomerId = _order.CustomerId,
                OfficeId = _order.OfficeId,
                SalesTaxRate = _order.SalesTaxRate,
            });

            // Assert
            orderLines = await UsingDbContextAsync(async context => await context.OrderLines.ToListAsync());
            orderLines.Count.ShouldBe(2);
            orderLines[0].TimeOnJob.ShouldBe(olTimeOnJob);
            orderLines[1].TimeOnJob.ShouldBe(olTimeOnJob);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

    }
}
