using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Authorization.Users;

namespace DispatcherWeb.Customers
{
    public interface ICustomerContactUserLinkService : IApplicationService
    {
        Task EnsureCanDeleteCustomerContact(CustomerContact customerContact);
        Task EnsureCanDeleteUser(User user);
        Task UpdateCustomerContact(User user);
        Task<User> UpdateUser(CustomerContact customerContact, bool sendEmail = true);

    }
}
