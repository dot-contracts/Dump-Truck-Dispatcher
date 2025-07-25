using System;
using System.Threading.Tasks;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.RealTime;
using DispatcherWeb.SignalR;

namespace DispatcherWeb.Web.SignalR
{
    public class AsyncOnlineClientHubBase : OnlineClientHubBase
    {
        protected new IAsyncOnlineClientManager OnlineClientManager { get; set; }

        public AsyncOnlineClientHubBase(
            IAsyncOnlineClientManager asyncOnlineClientManager,
            IOnlineClientManager onlineClientManager,
            IOnlineClientInfoProvider clientInfoProvider
            )
            : base(
                onlineClientManager,
                clientInfoProvider
            )
        {
            OnlineClientManager = asyncOnlineClientManager;
        }

        public override async Task OnConnectedAsync()
        {
            var client = CreateClientForCurrentConnection();

            Logger.Debug("A client is connected: " + client);

            await OnlineClientManager.AddAsync(client);
        }

        //Current Abp version is inherited from older Hub version which does not have OnReconnected method.
        //public override async Task OnReconnected()
        //{
        //    await base.OnReconnected();
        //    var client = await OnlineClientManager.GetByConnectionIdOrNullAsync(Context.ConnectionId);
        //    if (client == null)
        //    {
        //        client = CreateClientForCurrentConnection();
        //        await OnlineClientManager.AddAsync(client);
        //        Logger.Debug("A client is connected (on reconnected event): " + client);
        //    }
        //    else
        //    {
        //        Logger.Debug("A client is reconnected: " + client);
        //    }
        //}

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Logger.Debug("A client is disconnected: " + Context.ConnectionId);

            try
            {
                await OnlineClientManager.RemoveAsync(Context.ConnectionId);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString(), ex);
            }
        }
    }
}
