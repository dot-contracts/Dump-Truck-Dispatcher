using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Runtime.Caching.Redis;
using DispatcherWeb.Configuration;
using DispatcherWeb.SyncRequests.Dto;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace DispatcherWeb.SyncRequests
{
    public class DriverSyncRequestStore : IDriverSyncRequestStore, ISingletonDependency
    {
        private const string SyncRequestKeyPrefix = "DriverSyncRequests:";
        private const string EntityWildcard = "*";
        private readonly IAbpRedisCacheDatabaseProvider _database;
        private readonly ISettingManager _settingManager;
        private readonly IConfigurationRoot _configuration;

        public DriverSyncRequestStore(
            IAbpRedisCacheDatabaseProvider database,
            ISettingManager settingManager,
            IAppConfigurationAccessor configurationAccessor
        )
        {
            _configuration = configurationAccessor.Configuration;
            _database = database;
            _settingManager = settingManager;
        }

        private async Task<bool> IsEnabledAsync()
        {
            return !string.IsNullOrEmpty(_configuration["Abp:RedisCache:ConnectionString"])
                && await _settingManager.GetSettingValueAsync<bool>(AppSettings.DriverApp.IsDriverSyncRequestStoreEnabled);
        }

        /// <summary>
        /// Updates a sync timestamp for the specified entity type.
        /// Null values serve as wildcards: null driverId (all drivers in tenant), 
        /// null tenantId (all tenants), null entityType (all entity types).
        /// </summary>
        public async Task SetAsync(UpdateDriverSyncRequestTimestampInput input)
        {
            if (!await IsEnabledAsync() || input == null)
            {
                return;
            }

            var database = GetDatabase();
            string hashKey = GetRedisKey(input.DriverId, input.TenantId);
            string entityType = input.EntityType ?? EntityWildcard;
            string timestampString = DateTime.UtcNow.ToString("o");

            await database.HashSetAsync(hashKey, entityType, timestampString);
        }


        /// <summary>
        /// Updates multiple sync timestamps in a single operation.
        /// </summary>
        public async Task SetAsync(IReadOnlyCollection<UpdateDriverSyncRequestTimestampInput> inputs)
        {
            if (!await IsEnabledAsync() || inputs == null || !inputs.Any())
            {
                return;
            }

            var database = GetDatabase();
            var batch = database.CreateBatch();
            var timestamp = DateTime.UtcNow.ToString("o");

            var entitiesByKey = inputs
                .GroupBy(input => GetRedisKey(input.DriverId, input.TenantId))
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .Select(x => x.EntityType ?? EntityWildcard)
                        .Distinct()
                        .Select(entityType => new HashEntry(entityType, timestamp))
                        .ToArray()
                );

            var tasks = entitiesByKey
                .Select(keyGroup => batch.HashSetAsync(keyGroup.Key, keyGroup.Value))
                .ToList();

            batch.Execute();
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Gets all sync timestamps for specific driver, including applicable wildcards.
        /// </summary>
        public async Task<Dictionary<string, DateTime>> GetAsync(int driverId, int? tenantId)
        {
            if (!await IsEnabledAsync())
            {
                return new();
            }

            var database = GetDatabase();

            var batch = database.CreateBatch();
            var driverTask = batch.HashGetAllAsync(GetRedisKey(driverId, tenantId));
            var tenantWildcardTask = batch.HashGetAllAsync(GetRedisKey(null, tenantId));
            var globalWildcardTask = batch.HashGetAllAsync(GetRedisKey(null, null));

            batch.Execute();

            await Task.WhenAll(driverTask, tenantWildcardTask, globalWildcardTask);

            var allEntries = new List<HashEntry>();
            allEntries.AddRange(await globalWildcardTask);
            allEntries.AddRange(await tenantWildcardTask);
            allEntries.AddRange(await driverTask);

            var result = allEntries
                .GroupBy(entry => entry.Name)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Max(entry => DateTime.Parse(entry.Value).ToUniversalTime())
                );

            // Apply wildcard if it exists and has a higher timestamp
            if (result.TryGetValue(EntityWildcard, out var wildcardTimestamp))
            {
                foreach (var key in result.Keys.ToList())
                {
                    if (key != EntityWildcard && result[key] < wildcardTimestamp)
                    {
                        result[key] = wildcardTimestamp;
                    }
                }
            }

            return result;
        }

        private static string GetRedisKey(int? driverId, int? tenantId)
        {
            return $"{SyncRequestKeyPrefix}{driverId}@{tenantId}";
        }

        private IDatabase GetDatabase()
        {
            return _database.GetDatabase();
        }
    }
}
