namespace DispatcherWeb.Web.Areas.App.Models.Common.Modals
{
    public class ExtendedModalHeaderViewModel
    {
        public string Title { get; set; }

        public string ExtraText { get; set; }

        public ExtendedModalHeaderViewModel(string title, string extraText)
        {
            Title = title;
            ExtraText = extraText;
        }
    }
}
