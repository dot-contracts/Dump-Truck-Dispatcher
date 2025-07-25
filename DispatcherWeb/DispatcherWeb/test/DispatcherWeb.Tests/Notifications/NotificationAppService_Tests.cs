using System.Threading.Tasks;
using DispatcherWeb.Notifications;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Notifications
{
    public class NotificationAppService_Tests : AppTestBase
    {
        private readonly INotificationAppService _notificationAppService;

        public NotificationAppService_Tests()
        {
            _notificationAppService = Resolve<INotificationAppService>();
        }

        [Fact]
        public async Task Test_ChangeNotificationSettings()
        {
            var settings = await _notificationAppService.GetNotificationSettings();
            settings.ReceiveNotifications.ShouldBe(true);
            //settings.Notifications.Count.ShouldBeGreaterThan(0); // We removed "user registered" notification so the count is now at 0
        }
    }
}
