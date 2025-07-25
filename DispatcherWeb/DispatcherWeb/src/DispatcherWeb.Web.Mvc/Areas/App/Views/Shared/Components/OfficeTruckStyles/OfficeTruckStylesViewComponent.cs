using System.Linq;
using System.Threading.Tasks;
using Abp.Runtime.Session;
using DispatcherWeb.Caching;
using DispatcherWeb.Offices;
using DispatcherWeb.Offices.Dto;
using DispatcherWeb.Runtime.Session;
using DispatcherWeb.Web.Areas.App.Models.Layout;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Areas.App.Views.Shared.Components.OfficeTruckStyles
{
    public class OfficeTruckStylesViewComponent : ViewComponent
    {
        public IExtendedAbpSession Session { get; }
        private readonly IOfficeListCache _officeListCache;

        public OfficeTruckStylesViewComponent(
            IExtendedAbpSession session,
            IOfficeListCache officeListCache
        )
        {
            Session = session;
            _officeListCache = officeListCache;
        }

        public virtual async Task<IViewComponentResult> InvokeAsync()
        {
            var offices = await _officeListCache.GetList(new ListCacheTenantKey(await Session.GetTenantIdAsync()));

            var model = new OfficeTruckStylesViewModel
            {
                Offices = offices.Items.Select(x => new OfficeDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    TruckColor = x.TruckColor,
                }).ToList(),
            };

            return View(model);
        }
    }
}
