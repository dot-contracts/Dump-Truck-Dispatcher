using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Chat.Dto;
using DispatcherWeb.Drivers;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Trucks;

namespace DispatcherWeb.Chat
{
    public class ChatMessageDriverDetailsEnricher : IChatMessageDriverDetailsEnricher, ISingletonDependency
    {
        private readonly IDriverCache _driverCache;
        private readonly ILeaseHaulerCache _leaseHaulerCache;
        private readonly ITruckCache _truckCache;

        public ChatMessageDriverDetailsEnricher(
            IDriverCache driverCache,
            ITruckCache truckCache,
            ILeaseHaulerCache leaseHaulerCache
        )
        {
            _driverCache = driverCache;
            _truckCache = truckCache;
            _leaseHaulerCache = leaseHaulerCache;
        }

        public async Task<T> EnrichDriverDetails<T>(T chatMessage) where T : class, IChatMessageWithDriverDetails
        {
            if (chatMessage.TargetDriverId.HasValue)
            {
                var driver = await _driverCache.GetDriverFromCacheOrDefault(chatMessage.TargetDriverId.Value);
                if (driver != null)
                {
                    chatMessage.TargetDriverName = $"{driver.LastName}, {driver.FirstName}";
                }
            }
            if (chatMessage.TargetTruckId.HasValue)
            {
                var truck = await _truckCache.GetTruckFromCacheOrDbOrDefault(chatMessage.TargetTruckId.Value);
                if (truck != null)
                {
                    chatMessage.TargetTruckCode = truck.TruckCode;
                    if (truck.LeaseHaulerId.HasValue)
                    {
                        var leaseHauler = await _leaseHaulerCache.GetLeaseHaulerFromCacheOrDefault(truck.LeaseHaulerId.Value);
                        chatMessage.TargetTruckLeaseHaulerName = leaseHauler?.Name;
                    }
                }
            }

            return chatMessage;
        }

        public async Task EnrichDriverDetails(IEnumerable<IChatMessageWithDriverDetails> chatMessages)
        {
            var chatMessagesList = chatMessages.ToList();
            foreach (var driverGroup in chatMessagesList.GroupBy(x => x.TargetDriverId))
            {
                var driverId = driverGroup.Key;
                if (driverId.HasValue)
                {
                    var driver = await _driverCache.GetDriverFromCacheOrDefault(driverId.Value);
                    if (driver != null)
                    {
                        var driverName = $"{driver.LastName}, {driver.FirstName}";
                        foreach (var chatMessage in driverGroup)
                        {
                            chatMessage.TargetDriverName = driverName;
                        }
                    }
                }
            }

            foreach (var truckGroup in chatMessagesList.GroupBy(x => x.TargetTruckId))
            {
                var truckId = truckGroup.Key;
                if (truckId.HasValue)
                {
                    var truck = await _truckCache.GetTruckFromCacheOrDbOrDefault(truckId.Value);
                    if (truck != null)
                    {
                        var leaseHauler = truck.LeaseHaulerId.HasValue
                            ? await _leaseHaulerCache.GetLeaseHaulerFromCacheOrDefault(truck.LeaseHaulerId.Value)
                            : null;
                        foreach (var chatMessage in truckGroup)
                        {
                            chatMessage.TargetTruckCode = truck.TruckCode;
                            chatMessage.TargetTruckLeaseHaulerName = leaseHauler?.Name;
                        }
                    }
                }
            }
        }
    }
}
