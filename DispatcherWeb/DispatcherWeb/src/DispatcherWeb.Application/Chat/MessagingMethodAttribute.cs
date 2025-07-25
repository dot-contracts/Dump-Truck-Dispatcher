using Microsoft.AspNetCore.Mvc.Filters;

namespace DispatcherWeb.Chat
{
    /// <summary>
    /// This attributes needs to be added to the methods which we intend to be used with a messaging app service URL as a base URL, i.e. with abp.signalRPath instead of abp.appPath
    /// The attribute will be implemented later
    /// </summary>
    public class MessagingMethodAttribute : ActionFilterAttribute
    {
    }
}
