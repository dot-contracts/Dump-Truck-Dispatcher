using System;
using System.Linq;
using System.Threading.Tasks;
using DispatcherWeb.Customers;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Infrastructure.Sms;
using DispatcherWeb.Items;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Scheduling;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace DispatcherWeb.Tests.Scheduling
{
    public class SchedulingAppService_Tests_Base : AppTestBase, IAsyncLifetime
    {
        protected ISchedulingAppService _schedulingAppService;
        protected int _officeId;
        protected ISmsSender _smsSender;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _smsSender = Substitute.For<ISmsSender>();
            var dispatchingAppService = Resolve<IDispatchingAppService>(new { smsSender = _smsSender });
            _schedulingAppService = Resolve<ISchedulingAppService>(new { smsSender = _smsSender, dispatchingAppService = dispatchingAppService });
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        protected async Task<Order> CreateOrder(DateTime? date)
        {
            date ??= DateTime.Today;

            var orderEntity = await UsingDbContextAsync(async context =>
            {
                var order = new Order
                {
                    TenantId = 1,
                    DeliveryDate = date.Value,
                    Customer = context.Customers.FirstOrDefault() ?? new Customer() { TenantId = 1, Name = "Cust" },
                    Office = context.Offices.FirstOrDefault() ?? new Office() { TenantId = 1, Name = "Office1", TruckColor = "fff" },
                    SalesTaxRate = 2,
                };
                order.OrderLines.Add(new OrderLine
                {
                    TenantId = 1,
                    LineNumber = 1,
                    Designation = DesignationEnum.FreightAndMaterial,
                    FreightPricePerUnit = 2,
                    MaterialPricePerUnit = 3,
                    FreightItem = new Item
                    {
                        TenantId = 1,
                        Name = "sss",
                    },
                    MaterialUomId = 1,
                    NumberOfTrucks = 1,
                });
                order.OrderLines.Add(new OrderLine
                {
                    TenantId = 1,
                    LineNumber = 2,
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
                    NumberOfTrucks = 1,
                });
                context.Orders.Add(order);

                var user = await context.Users.FirstAsync(u => u.TenantId == 1);
                user.Office = order.Office;

                await context.SaveChangesAsync();

                return order;
            });
            return orderEntity;
        }
    }
}
