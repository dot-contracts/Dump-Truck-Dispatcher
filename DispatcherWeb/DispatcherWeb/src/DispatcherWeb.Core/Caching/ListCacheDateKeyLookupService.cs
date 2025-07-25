using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using DispatcherWeb.Configuration;
using DispatcherWeb.Orders;

namespace DispatcherWeb.Caching
{
    public partial class ListCacheDateKeyLookupService : ISingletonDependency
    {
        private readonly OrderToDateKey _orderToDateKey;
        private readonly OrderLineToOrder _orderLineToOrder;
        private readonly DispatchToOrderLine _dispatchToOrderLine;
        private readonly LeaseHaulerRequestToOrderLine _leaseHaulerRequestToOrderLine;

        public ISettingManager SettingManager { get; }

        public ListCacheDateKeyLookupService(
            OrderToDateKey orderToDateKey,
            OrderLineToOrder orderLineToOrder,
            DispatchToOrderLine dispatchToOrderLine,
            LeaseHaulerRequestToOrderLine leaseHaulerRequestToOrderLine,
            BaseCacheDependency baseCacheDependency
        )
        {
            _orderToDateKey = orderToDateKey;
            _orderLineToOrder = orderLineToOrder;
            _dispatchToOrderLine = dispatchToOrderLine;
            _leaseHaulerRequestToOrderLine = leaseHaulerRequestToOrderLine;
            SettingManager = baseCacheDependency.SettingManager;
        }

        public async Task<ListCacheDateKey> GetKeyForOrder(int orderId)
        {
            return await _orderToDateKey.LookupParentKey(orderId);
        }

        public async Task<ListCacheDateKey> GetKeyForOrderLine(int orderLineId)
        {
            var orderId = await _orderLineToOrder.LookupParentKey(orderLineId);
            if (orderId == null)
            {
                return null;
            }
            return await _orderToDateKey.LookupParentKey(orderId.Value);
        }

        public async Task<ListCacheDateKey> GetKeyForDispatch(int dispatchId)
        {
            var orderLineId = await _dispatchToOrderLine.LookupParentKey(dispatchId);
            if (orderLineId == null)
            {
                return null;
            }
            return await GetKeyForOrderLine(orderLineId.Value);
        }

        public async Task<ListCacheDateKey> GetKeyForLeaseHaulerRequest(int leaseHaulerRequestId)
        {
            var orderLineId = await _leaseHaulerRequestToOrderLine.LookupParentKey(leaseHaulerRequestId);
            if (orderLineId == null)
            {
                return null;
            }
            return await GetKeyForOrderLine(orderLineId.Value);
        }

        public async Task InvalidateKeyForOrder(Order order)
        {
            await _orderToDateKey.InvalidateKeyForEntity(order);
        }

        public async Task<bool> IsEnabled()
        {
            return await SettingManager.GetSettingValueAsync<bool>(AppSettings.ListCaches.DateKeyLookup.IsEnabled);
        }

    }
}
