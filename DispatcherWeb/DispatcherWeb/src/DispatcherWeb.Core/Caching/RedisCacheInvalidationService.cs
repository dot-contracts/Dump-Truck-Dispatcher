using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Events.Bus;
using Abp.Runtime.Caching.Redis;
using Abp.Timing;
using Castle.Core.Logging;
using DispatcherWeb.Configuration;
using DispatcherWeb.Timing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DispatcherWeb.Caching
{
    public class RedisCacheInvalidationService : ICacheInvalidationService, ISingletonDependency, IDisposable
    {
        private const string StoreKey = "DispatcherWeb.Caching.CacheInvalidation.Instructions";
        private const string ChannelName = "DispatcherWeb.Caching.CacheInvalidation.Channel";
        private static readonly RedisChannel RedisChannel = new RedisChannel(ChannelName, RedisChannel.PatternMode.Literal);
        private readonly AppTimes _appTimes;
        private readonly IAbpRedisCacheDatabaseProvider _databaseProvider;
        private readonly IConfigurationRoot _configuration;
        private readonly ConcurrentDictionary<string, CacheInvalidationInstruction> _localCache = new();
        private readonly ConcurrentDictionary<string, Timer> _debounceTimers = new();
        private readonly TimeSpan _debounceDelay;
        private readonly TimeSpan _cleanupInterval;
        private ISubscriber _subscriber;

        public IEventBus EventBus { get; set; }
        public ILogger Logger { get; set; }

        public RedisCacheInvalidationService(
            AppTimes appTimes,
            IAbpRedisCacheDatabaseProvider databaseProvider,
            IAppConfigurationAccessor configurationAccessor
            )
        {
            _appTimes = appTimes;
            _databaseProvider = databaseProvider;
            _configuration = configurationAccessor.Configuration;

            EventBus = NullEventBus.Instance;
            Logger = NullLogger.Instance;

            _debounceDelay = TimeSpan.FromMilliseconds(_configuration.GetCacheInvalidationDebounceMs());
            _cleanupInterval = TimeSpan.FromSeconds(_configuration.ParseInt("CacheInvalidation:CleanupInterval"));

            if (IsEnabled && _configuration["CacheInvalidation:PubSubEnabled"] == "true")
            {
                _subscriber = GetDatabase().Multiplexer.GetSubscriber();
                _subscriber.Subscribe(RedisChannel).OnMessage(ReceiveCacheInvalidationInstructionAsync);
            }
        }

        public void Dispose()
        {
            try
            {
                _subscriber?.UnsubscribeAll();

                foreach (var timer in _debounceTimers.Values)
                {
                    timer.Dispose();
                }
                _debounceTimers.Clear();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error disposing RedisCacheInvalidationService: {ex.Message}", ex);
            }
        }

        private bool IsEnabled => !string.IsNullOrEmpty(_configuration["Abp:RedisCache:ConnectionString"]);

        public Task SendCacheInvalidationInstructionAsync(string cacheName, string cacheKey = null, bool hardInvalidate = false)
        {
            SendCacheInvalidationInstruction(cacheName, cacheKey, hardInvalidate);
            return Task.CompletedTask;
        }

        public void SendCacheInvalidationInstruction(string cacheName, string cacheKey = null, bool hardInvalidate = false)
        {
            if (!IsEnabled)
            {
                return;
            }

            var instructionKey = CacheInvalidationInstruction.GetKey(cacheName, cacheKey, hardInvalidate);

            _debounceTimers.AddOrUpdate(instructionKey,
                // Create new timer if one doesn't exist
                _ => new Timer(_ =>
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                await SendCacheInvalidationInstructionInternalAsync(cacheName, cacheKey, hardInvalidate);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"Error invalidating cache: {ex.Message}", ex);
                            }
                        }).ConfigureAwait(false);
                    }, null, _debounceDelay, Timeout.InfiniteTimeSpan),
                // Reset the timer if it already exists
                (_, existingTimer) =>
                {
                    existingTimer.Change(_debounceDelay, Timeout.InfiniteTimeSpan);
                    return existingTimer;
                });
        }

        private async Task SendCacheInvalidationInstructionInternalAsync(string cacheName, string cacheKey = null, bool hardInvalidate = false)
        {
            var instruction = new CacheInvalidationInstruction
            {
                CacheName = cacheName,
                CacheKey = cacheKey,
                Guid = Guid.NewGuid(),
                CreationDateTime = Clock.Now,
                HardInvalidate = hardInvalidate,
            };

            try
            {
                _localCache.AddOrUpdate(instruction.GetKey(), instruction, (_, _) => instruction);

                var database = GetDatabase();
                var serializedInstruction = JsonConvert.SerializeObject(instruction);
                await database.HashSetAsync(StoreKey, new[]
                {
                    new HashEntry(instruction.GetKey(), serializedInstruction),
                });

                await database.PublishAsync(RedisChannel, serializedInstruction);
            }
            finally
            {
                if (_debounceTimers.TryRemove(instruction.GetKey(), out var timer))
                {
                    await timer.DisposeAsync();
                }
            }
        }

        public async Task<IReadOnlyList<CacheInvalidationInstruction>> GetAllInvalidationInstructionsAsync()
        {
            if (!IsEnabled)
            {
                return ImmutableList<CacheInvalidationInstruction>.Empty;
            }

            var database = GetDatabase();
            var entries = await database.HashGetAllAsync(StoreKey);
            var instructions = entries
                .Select(entry => JsonConvert.DeserializeObject<CacheInvalidationInstruction>(entry.Value))
                .ToImmutableList();

            return instructions;
        }

        public async Task ReceiveNewPersistentInstructions()
        {
            if (!IsEnabled)
            {
                return;
            }

            var cleanupCutoffTime = Clock.Now.Subtract(_cleanupInterval);

            var allInstructions = await GetAllInvalidationInstructionsAsync();
            var newInstructions = new List<CacheInvalidationInstruction>();
            var instructionsToDelete = new List<CacheInvalidationInstruction>();
            foreach (var instruction in allInstructions)
            {
                if (instruction.CreationDateTime < cleanupCutoffTime)
                {
                    instructionsToDelete.Add(instruction);
                    continue;
                }

                if (instruction.CreationDateTime < _appTimes.StartupTime)
                {
                    continue;
                }

                if (_localCache.TryGetValue(instruction.GetKey(), out var existingInstruction)
                    && existingInstruction.Equals(instruction))
                {
                    continue;
                }

                newInstructions.Add(instruction);

                _localCache.AddOrUpdate(instruction.GetKey(), instruction, (_, _) => instruction);
            }

            if (instructionsToDelete.Any())
            {
                var database = GetDatabase();
                await database.HashDeleteAsync(StoreKey, instructionsToDelete.Select(x => (RedisValue)x.GetKey()).ToArray());
                foreach (var instructionToDelete in instructionsToDelete)
                {
                    _localCache.TryRemove(instructionToDelete.GetKey(), out _);
                }
            }

            foreach (var instructionToDelete in _localCache.Values
                         .Where(x => x.CreationDateTime < cleanupCutoffTime)
                         .ToList())
            {
                _localCache.TryRemove(instructionToDelete.GetKey(), out _);
            }

            if (!newInstructions.Any())
            {
                return;
            }

            await EventBus.TriggerAsync(new CacheInvalidationInstructionsEventData
            {
                Instructions = newInstructions,
            });
        }

        private async Task ReceiveCacheInvalidationInstructionAsync(ChannelMessage message)
        {
            var instruction = JsonConvert.DeserializeObject<CacheInvalidationInstruction>(message.Message);
            await ReceiveCacheInvalidationInstructionAsync(instruction);
        }

        private async Task ReceiveCacheInvalidationInstructionAsync(CacheInvalidationInstruction instruction)
        {
            if (_localCache.TryGetValue(instruction.GetKey(), out var existingInstruction)
                && existingInstruction.Equals(instruction))
            {
                return;
            }

            _localCache.AddOrUpdate(instruction.GetKey(), instruction, (_, _) => instruction);

            await EventBus.TriggerAsync(new CacheInvalidationInstructionEventData
            {
                Instruction = instruction,
            });
        }

        private IDatabase GetDatabase()
        {
            return _databaseProvider.GetDatabase();
        }
    }
}
