using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DispatcherWeb.Web.Areas.App.Models.Settings
{
    public class QuickbooksOnlineViewModel
    {
        public List<SelectListItem> IncomeAccountList { get; set; }
        public string DefaultIncomeAccountId { get; set; }
        public string DefaultIncomeAccountName { get; set; }
    }
}
