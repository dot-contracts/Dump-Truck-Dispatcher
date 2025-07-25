using System.Collections.Generic;

namespace DispatcherWeb.DashboardCustomization.Dto
{
    public class SavePageInput
    {
        public string DashboardName { get; set; }

        public string Application { get; set; }

        public List<Page> Pages { get; set; }
    }
}
