using System.Threading.Tasks;
using DispatcherWeb.LeaseHaulers;
using DispatcherWeb.Tests.LeaseHaulerRequests;
using Xunit;

namespace DispatcherWeb.Tests.LeaseHaulers
{
    public class LeaseHaulerAppService_Tests_Base : LeaseHaulerRequestAppService_Tests_Base, IAsyncLifetime
    {
        protected ILeaseHaulerAppService _leaseHaulerAppService;

        public new async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _leaseHaulerAppService = Resolve<ILeaseHaulerAppService>();
        }


    }
}
