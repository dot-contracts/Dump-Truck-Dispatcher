using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Customers;
using DispatcherWeb.Items;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Orders.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Orders
{
    public class OrderAppService_Tests : AppTestBase
    {
        private readonly IOrderAppService _orderAppService;

        public OrderAppService_Tests()
        {
            _orderAppService = Resolve<IOrderAppService>();
        }

        [Fact(Skip = "#14648 Historically failing")]
        public async Task Test_DeleteOrderLine_should_update_price_fields()
        {
            var orderEntity = await CreateOrder();

            var orderLineId = orderEntity.OrderLines.First(ol => ol.FreightPricePerUnit == 2).Id;
            var input = new DeleteOrderLineInput
            {
                Id = orderLineId,
                OrderId = orderEntity.Id,
            };
            await _orderAppService.DeleteOrderLine(input);

            var orderEntityResult = await UsingDbContextAsync(async context =>
            {
                return await context.Orders
                    .Include(o => o.OrderLines)
                    .FirstAsync(o => o.Id == orderEntity.Id);
            });
            orderEntityResult.Id.ShouldBe(orderEntity.Id);
            orderEntityResult.OrderLines.Count(ol => !ol.IsDeleted).ShouldBe(1);
            orderEntityResult.FreightTotal.ShouldBe(0 + 2 * 20); //  2nd OrderLine
            orderEntityResult.MaterialTotal.ShouldBe(0 + 2 * 30);
            orderEntityResult.SalesTax.ShouldBe(2);
            orderEntityResult.CODTotal.ShouldBe(102);
            var orderLine = orderEntityResult.OrderLines.First(ol => !ol.IsDeleted);
            orderLine.FreightPrice.ShouldBe(20 * 2);
            orderLine.MaterialPrice.ShouldBe(30 * 2);
            orderLine.LineNumber.ShouldBe(1);
        }

        private async Task<Order> CreateOrder()
        {
            var orderEntity = await UsingDbContextAsync(async context =>
            {
                var order = new Order
                {
                    TenantId = 1,
                    DeliveryDate = Clock.Now.Date,
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
                await context.SaveChangesAsync();

                return order;
            });
            return orderEntity;
        }

    }
}
