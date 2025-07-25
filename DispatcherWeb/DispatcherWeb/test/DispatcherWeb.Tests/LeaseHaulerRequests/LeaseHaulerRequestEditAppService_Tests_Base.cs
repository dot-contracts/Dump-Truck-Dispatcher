using System.Threading.Tasks;
using DispatcherWeb.LeaseHaulerRequests;
using Xunit;

namespace DispatcherWeb.Tests.LeaseHaulerRequests
{
    public class LeaseHaulerRequestEditAppService_Tests_Base : LeaseHaulerRequestAppService_Tests_Base, IAsyncLifetime
    {
        protected ILeaseHaulerRequestEditAppService _leaseHaulerRequestEditAppService;

        public new async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _leaseHaulerRequestEditAppService = Resolve<ILeaseHaulerRequestEditAppService>();
        }
    }
}
