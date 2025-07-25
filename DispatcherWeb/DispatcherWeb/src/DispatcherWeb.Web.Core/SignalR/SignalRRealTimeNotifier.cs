using System;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Notifications;
using Castle.Core.Logging;
using DispatcherWeb.Configuration;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace DispatcherWeb.Web.SignalR
{
    /// <summary>
    /// Implements <see cref="IRealTimeNotifier"/> to send notifications via SignalR.
    /// </summary>
    public class SignalRRealTimeNotifier : IRealTimeNotifier, ITransientDependency
    {
        /// <summary>
        /// Reference to the logger.
        /// </summary>
        public ILogger Logger { get; set; }

        private readonly IAsyncOnlineClientManager _onlineClientManager;

        private readonly IHubContext<SignalRHub> _hubContext;

        private readonly IConfigurationRoot _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalRRealTimeNotifier"/> class.
        /// </summary>
        public SignalRRealTimeNotifier(
            IAppConfigurationAccessor configurationAccessor,
            IAsyncOnlineClientManager onlineClientManager,
            IHubContext<SignalRHub> hubContext)
        {
            _configuration = configurationAccessor.Configuration;
            _onlineClientManager = onlineClientManager;
            _hubContext = hubContext;
            Logger = NullLogger.Instance;
        }

        /// <inheritdoc/>
        public async Task SendNotificationsAsync(UserNotification[] userNotifications)
        {
            var delay = _configuration.GetCombinedCacheInvalidationDelay();
            if (delay > 0)
            {
                await Task.Delay(delay);
            }
            var allOnlineClients = await _onlineClientManager.GetAllClientsAsync();
            foreach (var userNotification in userNotifications)
            {
                try
                {
                    var onlineClients = allOnlineClients.FilterBy(userNotification).ToList();
                    var signalRClients = _hubContext.Clients.Clients(onlineClients.Select(x => x.ConnectionId));
#pragma warning disable CS0618 // Type or member is obsolete, this line will be removed once the EntityType property is removed
                    userNotification.Notification.EntityType = null; // Serialization of System.Type causes SignalR to disconnect. See https://github.com/aspnetboilerplate/aspnetboilerplate/issues/5230
#pragma warning restore CS0618 // Type or member is obsolete, this line will be removed once the EntityType property is removed
                    await signalRClients.SendAsync("getNotification", userNotification);
                }
                catch (Exception ex)
                {
                    Logger.Warn("Could not send notification to user: " + userNotification.ToUserIdentifier());
                    Logger.Warn(ex.ToString(), ex);
                }
            }
        }
    }
}
