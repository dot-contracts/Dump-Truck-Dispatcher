using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Threading;
using Abp.Threading.BackgroundWorkers;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using DispatcherWeb.Configuration;

namespace DispatcherWeb.AzureServiceBus
{
    public class ServiceBusBackgroundJobManager : BackgroundWorkerBase, IBackgroundJobManager
    {
        private readonly IIocResolver _iocResolver;
        private ServiceBusClient _client;
        private ServiceBusAdministrationClient _adminClient;
        private ServiceBusSender _sender;
        private ServiceBusProcessor _serviceBusProcessor;
        private readonly string _connectionString;
        private readonly string _queueName;


        public ServiceBusBackgroundJobManager(IAppConfigurationAccessor configurationAccessor, IIocResolver iocResolver)
        {
            _iocResolver = iocResolver;
            var configuration = configurationAccessor.Configuration;
            _connectionString = configuration["Abp:ServiceBusConnectionString"];
            _queueName = configuration["Abp:ServiceBusBackgroundJobQueueName"];
        }


        public async Task<string> EnqueueAsync<TJob, TArgs>(TArgs args, BackgroundJobPriority priority = BackgroundJobPriority.Normal,
            TimeSpan? delay = null) where TJob : IBackgroundJobBase<TArgs>
        {
            var messageBody = JsonSerializer.Serialize(new
            {
                JobType = GetStableTypeName(typeof(TJob)),
                JobArgsType = GetStableTypeName(typeof(TArgs)),
                JobArgs = args,
            });

            var message = new ServiceBusMessage(messageBody)
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/json",
            };

            if (delay != null)
            {
                message.ScheduledEnqueueTime = DateTimeOffset.UtcNow.Add(delay.Value);
            }

            await _sender.SendMessageAsync(message);

            return message.MessageId;
        }

        public string Enqueue<TJob, TArgs>(TArgs args, BackgroundJobPriority priority = BackgroundJobPriority.Normal, TimeSpan? delay = null)
            where TJob : IBackgroundJobBase<TArgs>
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(string jobId)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string jobId)
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            base.Start();

            AsyncHelper.RunSync(async () =>
            {
                _adminClient = new ServiceBusAdministrationClient(_connectionString);
                if (await _adminClient.QueueExistsAsync(_queueName) == false)
                {
                    await _adminClient.CreateQueueAsync(_queueName);
                }

                _client = new ServiceBusClient(_connectionString);

                _sender = _client.CreateSender(_queueName);

                _serviceBusProcessor = _client.CreateProcessor(_queueName,
                    new ServiceBusProcessorOptions
                    {
                        AutoCompleteMessages = false,
                        MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(20),
                    });
                _serviceBusProcessor.ProcessMessageAsync += ProcessMessageAsync;
                _serviceBusProcessor.ProcessErrorAsync += ProcessErrorAsync;
                await _serviceBusProcessor.StartProcessingAsync();
            });
        }

        public override void WaitToStop()
        {
            try
            {
                AsyncHelper.RunSync(async () =>
                {
                    _adminClient = null;

                    await _serviceBusProcessor.DisposeAsync();
                    _serviceBusProcessor = null;

                    await _sender.DisposeAsync();
                    _sender = null;

                    await _client.DisposeAsync();
                    _client = null;
                });
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString(), ex);
            }


            base.WaitToStop();
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs messageArgs)
        {
            try
            {
                var jsonSerializerOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                if (messageArgs.Message == null)
                {
                    throw new Exception("Received empty ServiceBus message");
                }

                var message = JsonSerializer.Deserialize<MessageObject>(messageArgs.Message.Body.ToString(), jsonSerializerOptions);

                var jobType = Type.GetType(message.JobType);
                if (jobType == null)
                {
                    throw new Exception($"Cannot deserialize data for type {message.JobType}");
                }

                var jobArgsType = Type.GetType(message.JobArgsType);
                if (jobArgsType == null)
                {
                    throw new Exception($"Cannot deserialize data for type {message.JobArgsType}");
                }

                var jobArgs = message.JobArgs.Deserialize(jobArgsType, jsonSerializerOptions);
                var asyncBackgroundJobType = typeof(IAsyncBackgroundJob<>).MakeGenericType(jobArgsType);
                using (var scope = _iocResolver.CreateScope())
                {
                    var jobInstance = scope.Resolve(jobType);

                    if (asyncBackgroundJobType.IsAssignableFrom(jobType))
                    {
                        var executeAsyncMethod = jobType.GetMethod("ExecuteAsync")!;
                        await (Task)executeAsyncMethod.Invoke(jobInstance, new[] { jobArgs })!;
                    }
                    else
                    {
                        var executeMethod = jobType.GetMethod("Execute")!;
                        executeMethod.Invoke(jobInstance, new[] { jobArgs });
                    }
                }

                await messageArgs.CompleteMessageAsync(messageArgs.Message);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error when processing background job message: {ex.Message}", ex);
                throw;
            }
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs messageArgs)
        {
            Logger.Error($"Error when processing background job message: {messageArgs.Exception.Message}", messageArgs.Exception);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns result similar to type.FullName or type.AssemblyQualifiedName, but with no versions specified for generic types, only FullName and Assembly is included
        /// </summary>
        private string GetStableTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var genericArguments = string.Join(",", type.GetGenericArguments().Select(x => $"[{GetStableTypeName(x)}]"));
                return $"{genericTypeDefinition.FullName?.Split('`')[0]}`{genericTypeDefinition.GetGenericArguments().Length}[{genericArguments}], {genericTypeDefinition.Assembly.GetName().Name}";
            }
            else
            {
                var assemblyName = type.Assembly.GetName().Name;
                return $"{type.FullName}, {assemblyName}";
            }
        }

        internal class MessageObject
        {
            public string JobType { get; set; }

            public string JobArgsType { get; set; }

            public JsonElement JobArgs { get; set; }
        }
    }
}
