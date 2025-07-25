using Abp.UI;

namespace DispatcherWeb.Web.Areas.App.Models.Shared
{
    public class UserFriendlyExceptionViewModel
    {
        public string Message { get; set; }
        public string Details { get; set; }

        public UserFriendlyExceptionViewModel()
        {
        }

        public UserFriendlyExceptionViewModel(UserFriendlyException exception)
        {
            Message = exception.Message;
            Details = exception.Details;
        }
    }
}
