using System.Threading.Tasks;
using AutoFixture;
using DispatcherWeb.Customers;
using DispatcherWeb.Customers.Dto;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Customers
{
    public class CustomerAppService_CustomerExists_Tests : AppTestBase, IAsyncLifetime
    {
        private ICustomerAppService _customerAppService;
        private int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;
            _customerAppService = Resolve<ICustomerAppService>();

        }

        [Fact]
        public async Task Test_CustomerExists_should_return_true_when_Customer_exists()
        {
            // Arrange
            var fixture = new Fixture();
            string customerName = fixture.Create<string>();
            string accountNameIsCod = fixture.Create("cod");
            var customer = await CreateCustomer(customerName, accountNameIsCod);

            // Act
            var result = await _customerAppService.GetCustomerIfExistsOrNull(new GetCustomerIdIfExistsOrNullInput() { Name = customerName });

            // Arrange
            result.Id.ShouldBe(customer.Id);
            result.Name.ShouldBe(customerName);
            result.AccountNumber.ShouldBe(accountNameIsCod);
        }

        [Fact]
        public async Task Test_CustomerExists_should_return_false_when_Customer_does_not_exist()
        {
            // Arrange
            var fixture = new Fixture();
            string customerName = fixture.Create<string>();
            await CreateCustomer("AnotherCustomerName");

            // Act
            var result = await _customerAppService.GetCustomerIfExistsOrNull(new GetCustomerIdIfExistsOrNullInput() { Name = customerName });

            // Arrange
            result.ShouldBeNull();
        }

        private async Task<Customer> CreateCustomer(string name, string accountNumber = null)
        {
            return await UsingDbContextAsync(async context =>
            {
                var customer = new Customer() { TenantId = 1, Name = name, AccountNumber = accountNumber };
                await context.Customers.AddAsync(customer);
                return customer;
            });
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
