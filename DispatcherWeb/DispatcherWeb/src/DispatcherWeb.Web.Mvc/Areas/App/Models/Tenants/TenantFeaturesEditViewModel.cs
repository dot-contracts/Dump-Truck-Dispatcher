using DispatcherWeb.MultiTenancy.Dto;
using DispatcherWeb.Web.Areas.App.Models.Common;

namespace DispatcherWeb.Web.Areas.App.Models.Tenants
{
    public class TenantFeaturesEditViewModel : GetTenantFeaturesEditOutput, IFeatureEditViewModel
    {
        public TenantFeaturesEditViewModel()
        {
        }

        public TenantFeaturesEditViewModel(GetTenantFeaturesEditOutput tenantFeaturesModel)
        {
            FeatureValues = tenantFeaturesModel.FeatureValues;
            Features = tenantFeaturesModel.Features;
        }

        public string TenantName { get; set; }
    }
}
