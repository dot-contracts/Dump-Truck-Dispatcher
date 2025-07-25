using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.EntityFrameworkCore.Uow;
using Abp.UI;
using DispatcherWeb.Customers;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Emailing;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Items;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Orders
{
    public class OrderAppService_DeleteOrder_Tests : AppTestBase, IAsyncLifetime
    {
        private readonly IOrderAppService _orderAppService;
        private int _orderId;

        public OrderAppService_DeleteOrder_Tests()
        {
            _orderAppService = Resolve<IOrderAppService>();
        }

        public async Task InitializeAsync()
        {
            _orderId = (await CreateOrder()).Id;
        }

        [Fact]
        public async Task Test_DeleteOrder_should_delete_Order_with_OrderLines()
        {
            // Act 
            await _orderAppService.DeleteOrder(new EntityDto(_orderId));

            // Assert
            var order = await GetOrder();
            order.IsDeleted.ShouldBe(true);
            order.OrderLines.ToList().ForEach(ol => ol.IsDeleted.ShouldBeTrue());
        }

        [Fact]
        public async Task Test_DeleteOrder_should_throw_UserFriendlyException_when_it_is_closed()
        {
            // Assign
            await UpdateOrder(order => order.IsClosed = true);

            // Act & Assert
            await DeleteOrderShouldThrowUserFriendlyException();
        }

        [Fact]
        public async Task Test_DeleteOrder_should_throw_UserFriendlyException_when_OrderLine_is_closed()
        {
            // Assign
            await UpdateOrder(order => order.OrderLines.First().IsComplete = true);

            // Act & Assert
            await DeleteOrderShouldThrowUserFriendlyException();
        }

        [Fact]
        public async Task Test_DeleteOrder_should_throw_UserFriendlyException_when_there_is_BilledOrder()
        {
            // Assign
            await UpdateOrder(order =>
            {
                order.BilledOrders.Add(new BilledOrder
                {
                    OfficeId = order.OfficeId,
                });
            });

            // Act & Assert
            await DeleteOrderShouldThrowUserFriendlyException();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_DeleteOrder_should_throw_UserFriendlyException_when_there_is_OrderEmail()
        {
            // Assign
            await UpdateOrder(order =>
            {
                order.OrderEmails.Add(new OrderEmail
                {
                    Email = new TrackableEmail { Id = Guid.NewGuid() },
                });
            });

            // Act & Assert
            await DeleteOrderShouldThrowUserFriendlyException();
        }

        [Fact]
        public async Task Test_DeleteOrder_should_throw_UserFriendlyException_when_there_is_Ticket()
        {
            // Assign
            await UpdateOrder(order =>
            {
                order.OrderLines.First().Tickets = new List<Ticket>
                {
                    new Ticket { TenantId = 1 },
                };
            });

            // Act & Assert
            await DeleteOrderShouldThrowUserFriendlyException();
        }

        [Fact]
        public async Task Test_DeleteOrder_should_throw_UserFriendlyException_when_there_is_Dispatch()
        {
            // Assign
            await UpdateOrder(order =>
            {
                order.OrderLines.First().Dispatches = new List<Dispatch>
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
            await DeleteOrderShouldThrowUserFriendlyException();
        }

        [Fact]
        public async Task Test_DeleteOrder_should_throw_UserFriendlyException_when_there_is_OrderLineTruck()
        {
            // Assign
            await UpdateOrder(order =>
            {
                order.OrderLines.First().OrderLineTrucks = new List<OrderLineTruck>
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
            await DeleteOrderShouldThrowUserFriendlyException();
        }


        private Task UpdateOrder(Action<Order> action)
        {
            return UpdateOrder(order => Task.Run(() => action(order)));
        }
        private async Task UpdateOrder(Func<Order, Task> action)
        {
            await UsingDbContextAsync(async context =>
            {
                var order = await context.Orders.Include(o => o.OrderLines).Where(o => o.Id == _orderId).FirstAsync();
                await action(order);
            });
        }
        private async Task<Order> GetOrder()
        {
            return await UsingDbContextAsync(async context =>
            {
                return await context.Orders.Include(o => o.OrderLines).Where(o => o.Id == _orderId).FirstAsync();
            });
        }

        private async Task DeleteOrderShouldThrowUserFriendlyException()
        {
            await _orderAppService.DeleteOrder(new EntityDto(_orderId)).ShouldThrowAsync(typeof(UserFriendlyException));
            var order = await GetOrder();
            order.IsDeleted.ShouldBeFalse();
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
