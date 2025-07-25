using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Abp;
using Abp.Json;
using Abp.RealTime;
using DispatcherWeb.SignalR.Dto;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class OnlineClientExtensions
    {
        public static IEnumerable<IOnlineClient> FilterBy(this IEnumerable<IOnlineClient> clients, IUserIdentifier userIdentifier)
        {
            return clients.Where(x => x.UserId == userIdentifier.UserId && x.TenantId == userIdentifier.TenantId);
        }

        public const string LastHeartbeatDateTimeFieldName = "LastHeartbeatDateTime";

        public static IOnlineClient SetLastHeartbeatDateTime(this IOnlineClient onlineClient, DateTime heartbeatTime)
        {
            //onlineClient.Properties.Add
            onlineClient[LastHeartbeatDateTimeFieldName] = heartbeatTime.ToString("u");
            return onlineClient;
        }

        public static DateTime? GetLastHeartbeatDateTime(this IOnlineClient onlineClient)
        {
            if (onlineClient.Properties?.TryGetValue(LastHeartbeatDateTimeFieldName, out var stringResult) != true
                || string.IsNullOrEmpty(stringResult as string)
                || !DateTime.TryParse((string)stringResult, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var result))
            {
                return null;
            }

            return result;
        }

        public const string IsSubscribedToSyncRequestsFieldName = "IsSubscribedToSyncRequests";

        public static IOnlineClient SetIsSubscribedToSyncRequests(this IOnlineClient client, bool value = true)
        {
            client[IsSubscribedToSyncRequestsFieldName] = value.ToLowerCaseString();
            return client;
        }

        public static bool IsSubscribedToSyncRequests(this IOnlineClient client)
        {
            if (client.Properties?.TryGetValue(IsSubscribedToSyncRequestsFieldName, out var stringResult) != true
                || string.IsNullOrEmpty(stringResult as string)
                || !bool.TryParse((string)stringResult, out var result))
            {
                return false;
            }

            return result;
        }

        public const string SyncRequestFilterFieldName = "SyncRequestFilter";

        public static IOnlineClient SetSyncRequestFilter(this IOnlineClient client, SyncRequestFilterDto filter)
        {
            client[SyncRequestFilterFieldName] = filter.ToJsonString();
            return client;
        }

        public static SyncRequestFilterDto GetSyncRequestFilter(this IOnlineClient client)
        {
            if (client.Properties?.TryGetValue(SyncRequestFilterFieldName, out var stringResult) != true
                || string.IsNullOrEmpty(stringResult as string))
            {
                return null;
            }
            return ((string)stringResult).FromJsonString<SyncRequestFilterDto>();
        }
    }
}
