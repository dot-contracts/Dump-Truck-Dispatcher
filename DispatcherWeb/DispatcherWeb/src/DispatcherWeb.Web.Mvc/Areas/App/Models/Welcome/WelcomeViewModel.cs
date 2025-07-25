namespace DispatcherWeb.Web.Areas.App.Models.Welcome
{
    public class WelcomeViewModel
    {
        public WelcomeViewModel(string headerMessage)
        {
            HeaderMessage = headerMessage;
        }

        public string HeaderMessage { get; }
        public string DetailsMessage { get; set; }
    }
}