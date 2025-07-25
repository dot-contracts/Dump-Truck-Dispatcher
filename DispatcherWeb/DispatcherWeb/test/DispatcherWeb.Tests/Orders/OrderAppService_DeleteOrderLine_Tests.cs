using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.EntityFrameworkCore.Uow;
using Abp.UI;
using DispatcherWeb.Customers;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Items;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Orders
{
    public class OrderAppService_DeleteOrderLine_Tests : AppTestBase, IAsyncLifetime
    {
        private readonly IOrderAppService _orderAppService;
        private int _orderId;
        private int _orderLineId;

        public OrderAppService_DeleteOrderLine_Tests()
        {
            _orderAppService = Resolve<IOrderAppService>();
        }

        public async Task InitializeAsync()
        {
            var order = await CreateOrder();
            _orderId = order.Id;
            _orderLineId = order.OrderLines.First().Id;
        }

        [Fact]
        public async Task Test_DeleteOrder_should_delete_OrderLine()
        {
            // Act 
            await _orderAppService.DeleteOrderLine(new DeleteOrderLineInput(_orderLineId) { OrderId = _orderId });

            // Assert
            var orderLine = await GetOrderLine();
            orderLine.IsDeleted.ShouldBe(true);
        }

        [Fact]
        public async Task Test_DeleteOrderLine_should_throw_UserFriendlyException_when_Order_is_closed()
        {
            // Assign
            await UpdateOrder(order => order.IsClosed = true);

            // Act & Assert
            await DeleteOrderLineShouldThrowUserFriendlyException();
        }

        [Fact]
        public async Task Test_DeleteOrder_should_throw_UserFriendlyException_when_there_is_Ticket()
        {
            // Assign
            await UpdateOrderLine(orderLine =>
            {
                orderLine.Tickets = new List<Ticket>
                {
                    new Ticket { TenantId = 1 },
                };
            });

            // Act & Assert
            await DeleteOrderLineShouldThrowUserFriendlyException();
        }

        [Fact]
        public async Task Test_DeleteOrder_should_throw_UserFriendlyException_when_there_is_Dispatch()
        {
            // Assign
            await UpdateOrderLine(orderLine =>
            {
                orderLine.Dispatches = new List<Dispatch>
                {
                    new Dispatch
                    {
                        TenantId = 1,
                        TruckId = 1,
                        DriverId = 1,
                        UserId = 1,
                    },
                };
            });

            // Act & Assert
            await DeleteOrderLineShouldThrowUserFriendlyException();
        }

        [Fact]
        public async Task Test_DeleteOrder_should_throw_UserFriendlyException_when_there_is_OrderLineTruck()
        {
            // Assign
            await UpdateOrderLine(orderLine =>
            {
                orderLine.OrderLineTrucks = new List<OrderLineTruck>
                {
                    new OrderLineTruck
                    {
                        TenantId = 1,
                        TruckId = 1,
                        DriverId = null,
                    },
                };
            });

            // Act & Assert
            await DeleteOrderLineShouldThrowUserFriendlyException();
        }

        private Task UpdateOrderLine(Action<OrderLine> action)
        {
            return UpdateOrderLine(orderLine => Task.Run(() => action(orderLine)));
        }
        private async Task UpdateOrderLine(Func<OrderLine, Task> action)
        {
            await UsingDbContextAsync(async context =>
            {
                var orderLine = await context.OrderLines.Where(ol => ol.Id == _orderLineId).FirstAsync();
                await action(orderLine);
            });
        }
        private async Task UpdateOrder(Action<Order> action)
        {
            await UsingDbContextAsync(async context =>
            {
                var order = await context.Orders.Where(o => o.Id == _orderId).FirstAsync();
                action(order);
            });
        }
        private async Task<OrderLine> GetOrderLine()
        {
            return await UsingDbContextAsync(async context =>
            {
                return await context.OrderLines.Where(ol => ol.Id == _orderLineId).FirstAsync();
            });
        }

        private async Task DeleteOrderLineShouldThrowUserFriendlyException()
        {
            await _orderAppService.DeleteOrderLine(new DeleteOrderLineInput(_orderLineId) { OrderId = _orderId }).ShouldThrowAsync(typeof(UserFriendlyException));
            var orderLine = await GetOrderLine();
            orderLine.IsDeleted.ShouldBeFalse();
        }

        private async Task<Order> CreateOrder()
        {
            var orderEntity = await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var context = await UnitOfWorkManager.Current.GetDbContextAsync<DispatcherWebDbContext>();
                var order = new Order
                {
                    TenantId = 1,
                    Customer = new Customer() { TenantId = 1, Name = "Cust" },
                    Office = new Office() { TenantId = 1, Name = "Office1", TruckColor = "fff" },
                    SalesTaxRate = 2,
                };
                order.OrderLines.Add(new OrderLine
                {
                    TenantId = 1,
                    Designation = DesignationEnum.FreightAndMaterial,
                    FreightPricePerUnit = 2,
                    MaterialPricePerUnit = 3,
                    MaterialQuantity = 5,
                    FreightPrice = 2 * 5,
                    MaterialPrice = 3 * 5,
                    FreightItem = new Item
                    {
                        TenantId = 1,
                        Name = "sss",
                    },
                    MaterialUomId = 1,
                });
                order.OrderLines.Add(new OrderLine
                {
                    TenantId = 1,
                    Designation = DesignationEnum.FreightAndMaterial,
                    FreightPricePerUnit = 20,
                    MaterialPricePerUnit = 30,
                    MaterialQuantity = 2,
                    FreightPrice = 20 * 2,
                    MaterialPrice = 30 * 2,
                    FreightItem = new Item
                    {
                        TenantId = 1,
                        Name = "sss",
                    },
                    MaterialUomId = 1,
                });
                await context.Orders.AddAsync(order);

                var user = await context.Users.FirstAsync(u => u.TenantId == 1);
                user.Office = order.Office;

                var vehicleCategory = await CreateVehicleCategory();
                var truck = new Truck
                {
                    TenantId = 1,
                    TruckCode = "101",
                    IsActive = true,
                    AlwaysShowOnSchedule = true,
                    VehicleCategoryId = vehicleCategory.Id,
                    Office = order.Office,
                };
                await context.Trucks.AddAsync(truck);

                var driver = new Driver
                {
                    FirstName = "fn",
                    LastName = "ln",
                };
                await context.Drivers.AddAsync(driver);


                return order;
            });
            return orderEntity;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
