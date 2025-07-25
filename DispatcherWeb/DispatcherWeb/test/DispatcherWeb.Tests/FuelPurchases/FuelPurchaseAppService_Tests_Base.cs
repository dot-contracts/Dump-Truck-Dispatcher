using System.Threading.Tasks;
using DispatcherWeb.FuelPurchases;
using Xunit;

namespace DispatcherWeb.Tests.FuelPurchases
{
    public class FuelPurchaseAppService_Tests_Base : AppTestBase, IAsyncLifetime
    {
        protected IFuelPurchaseAppService _fuelPurchaseAppService;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _fuelPurchaseAppService = Resolve<IFuelPurchaseAppService>();
            SubstituteServiceDependencies(_fuelPurchaseAppService);
        }

        public Task DisposeAsync() => Task.CompletedTask;

    }
}
