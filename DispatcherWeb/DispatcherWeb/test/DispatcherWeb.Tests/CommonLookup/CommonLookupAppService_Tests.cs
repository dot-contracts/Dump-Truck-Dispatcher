using System.Threading.Tasks;
using DispatcherWeb.Common;
using Shouldly;

namespace DispatcherWeb.Tests.CommonLookup
{
    public class CommonLookupAppService_Tests : AppTestBase
    {
        private readonly ICommonLookupAppService _commonLookupAppService;

        public CommonLookupAppService_Tests()
        {
            LoginAsHostAdmin();
            _commonLookupAppService = Resolve<ICommonLookupAppService>();
        }

        [MultiTenantFact]
        public async Task Should_Get_Editions()
        {
            var paidEditions = await _commonLookupAppService.GetEditionsForCombobox();
            paidEditions.Items.Count.ShouldBe(10);

            var freeEditions = await _commonLookupAppService.GetEditionsForCombobox(true);
            freeEditions.Items.Count.ShouldBe(7);
        }
    }
}
