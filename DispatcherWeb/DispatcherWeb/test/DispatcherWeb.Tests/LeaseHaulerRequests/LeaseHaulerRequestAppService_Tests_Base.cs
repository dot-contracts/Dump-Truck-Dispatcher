using System;
using System.Threading.Tasks;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.LeaseHaulers;
using Xunit;

namespace DispatcherWeb.Tests.LeaseHaulerRequests
{
    public class LeaseHaulerRequestAppService_Tests_Base : AppTestBase, IAsyncLifetime
    {
        protected int _officeId;

        public async Task InitializeAsync()
        {
            var office = await CreateOfficeAndAssignUserToIt();
            _officeId = office.Id;

        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        protected async Task<LeaseHaulerContact> CreateLeaseHaulerContact(int leaseHaulerId,
            string name = "LeaseHaulerContact1", bool isDispatcher = true, string email = "lhc@example.com",
            string cellPhoneNumber = "+15005550011", OrderNotifyPreferredFormat notifyPreferredFormat = OrderNotifyPreferredFormat.Both)
        {
            return await UsingDbContextAsync(async context =>
            {
                var leaseHaulerContact = new LeaseHaulerContact
                {
                    TenantId = 1,
                    LeaseHaulerId = leaseHaulerId,
                    Name = name,
                    IsDispatcher = isDispatcher,
                    Email = email,
                    CellPhoneNumber = cellPhoneNumber,
                    NotifyPreferredFormat = notifyPreferredFormat,
                };
                await context.LeaseHaulerContacts.AddAsync(leaseHaulerContact);
                return leaseHaulerContact;
            });
        }

        protected async Task<LeaseHaulerRequest> CreateLeaseHaulerRequest(DateTime date, Shift? shift, int officeId, int leaseHaulerId, int available = 0, int approved = 0, DateTime? sent = null)
        {
            return await UsingDbContextAsync(async context =>
            {
                var leaseHaulerRequest = new LeaseHaulerRequest
                {
                    TenantId = 1,
                    Date = date,
                    Shift = shift,
                    OfficeId = officeId,
                    LeaseHaulerId = leaseHaulerId,
                    Sent = sent,
                    Available = available,
                    Approved = approved,
                };
                await context.LeaseHaulerRequests.AddAsync(leaseHaulerRequest);
                return leaseHaulerRequest;
            });
        }
    }
}
