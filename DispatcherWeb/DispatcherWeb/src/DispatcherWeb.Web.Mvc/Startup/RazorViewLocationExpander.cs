using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace DispatcherWeb.Web.Startup
{
    /// <summary>
    /// That class is generated so that new areas that use default layout can use default components.
    /// </summary>
    public class RazorViewLocationExpander : IViewLocationExpander
    {
        public RazorViewLocationExpander()
        {
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            var locations = viewLocations.ToList();

            //{0} is like "Components/{componentname}/{viewname}"
            locations.Add("~/Areas/App/Views/Shared/{0}.cshtml");
            //locations.Add("~/Areas/App/Views/Shared/Themes/Default/{0}.cshtml"); //we're not using this folder from AspNetZero11, i.e. did not even merge it into the Dev branch

            return locations;
        }
    }
}
