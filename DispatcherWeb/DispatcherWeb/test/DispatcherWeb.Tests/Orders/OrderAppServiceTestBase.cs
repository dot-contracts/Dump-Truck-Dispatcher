using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Orders;
using DispatcherWeb.Payments;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Orders
{
    public class OrderAppServiceTestBase : AppTestBase, IAsyncLifetime
    {
        protected IOrderAppService _orderAppService;
        protected int _officeId;
        protected Order _order;
        protected int _orderLineId;
        protected OrderLine _orderLine;
        protected Payment _payment;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            var orderLineScheduledTrucksUpdater = Resolve<IOrderLineScheduledTrucksUpdater>();
            _orderAppService = Resolve<IOrderAppService>(new { orderLineScheduledTrucksUpdater });

            var today = Clock.Now.Date;
            _order = await CreateOrderWithOrderLines(today);
            _order.OrderLines.Count.ShouldBe(2);
            _orderLineId = _order.OrderLines.First().Id;
            _orderLine = _order.OrderLines.First(ol => ol.Id == _orderLineId);
            _payment = await CreateOrderPayment(_order);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
