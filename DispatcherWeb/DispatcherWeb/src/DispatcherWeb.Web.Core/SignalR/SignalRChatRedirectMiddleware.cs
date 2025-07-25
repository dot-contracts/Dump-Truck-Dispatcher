using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DispatcherWeb.Web.SignalR
{
    public class SignalRChatRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public SignalRChatRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Check if the request is for the old SignalR hub URL
            if (context.Request.Path.StartsWithSegments("/signalr-chat")
                && context.Request.Path.Value != null)
            {
                // Modify the request path to route to the new URL
                var newPath = "/signalr" + context.Request.Path.Value["/signalr-chat".Length..];
                context.Request.Path = newPath;
            }

            await _next(context);
        }
    }
}
