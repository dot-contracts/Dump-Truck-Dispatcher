using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.UI;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Customers;
using DispatcherWeb.Features;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.CustomerContacts
{
    public class CustomerContactUserLinkService : DispatcherWebDomainServiceBase, ICustomerContactUserLinkService
    {
        private readonly IRepository<CustomerContact> _customerContactRepository;
        private readonly IUserCreatorService _userCreatorService;

        public CustomerContactUserLinkService(
            IRepository<CustomerContact> customerContactRepository,
            IUserCreatorService userCreatorService
        )
        {
            _customerContactRepository = customerContactRepository;
            _userCreatorService = userCreatorService;
        }

        /// <summary>
        /// Should be called when a user is modified
        /// </summary>
        public async Task UpdateCustomerContact(User user)
        {
            if (await Session.GetTenantIdOrNullAsync() == null)
            {
                return;
            }

            if (user.EmailAddress.IsNullOrEmpty())
            {
                throw new UserFriendlyException("EmailAddress is required");
            }

            if (user.Id == 0)
            {
                throw new ArgumentException("User should be saved first", nameof(user));
            }

            var userIsInCustomerRole = await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Customer);
            var customerPortalFeatureEnabled = await FeatureChecker.IsEnabledAsync(AppFeatures.CustomerPortal);
            var shouldCustomerContactHaveAccessToCustomerPortal = userIsInCustomerRole && customerPortalFeatureEnabled;

            var linkedCustomerContact = await (await _customerContactRepository.GetQueryAsync())
                                            .FirstOrDefaultAsync(x => x.Id == user.CustomerContactId);

            if (linkedCustomerContact != null)
            {
                linkedCustomerContact.HasCustomerPortalAccess = shouldCustomerContactHaveAccessToCustomerPortal;
                UpdateCustomerContactFromUser(linkedCustomerContact, user);
            }
        }

        /// <summary>
        /// Should be called when a CustomerContact is modified
        /// </summary>
        public async Task<User> UpdateUser(CustomerContact customerContact, bool sendEmail = true)
        {
            if (customerContact.Id == 0)
            {
                throw new ArgumentException("CustomerContact should be saved first", nameof(customerContact));
            }

            var linkedUser = await (await UserManager.GetQueryAsync()).FirstOrDefaultAsync(x => x.CustomerContactId == customerContact.Id);
            var shouldCustomerContactBeLinkedToUser = await ShouldUserBeLinkedToCustomerContact(customerContact);
            if (linkedUser != null)
            {
                var isUserInCustomerRole = await IsUserInCustomerRole(linkedUser);
                if (!shouldCustomerContactBeLinkedToUser && isUserInCustomerRole)
                {
                    await UserManager.RemoveFromRoleAsync(linkedUser, StaticRoleNames.Tenants.Customer);
                }
                else if (shouldCustomerContactBeLinkedToUser && !isUserInCustomerRole)
                {
                    await UserManager.AddToRoleAsync(linkedUser, StaticRoleNames.Tenants.Customer);
                }
                await UpdateUserFromCustomerContactAsync(linkedUser, customerContact);
                return linkedUser;
            }

            if (!shouldCustomerContactBeLinkedToUser)
            {
                return null;
            }

            var user = await UserManager.FindByEmailAsync(customerContact.Email);
            if (user != null)
            {
                if (user.CustomerContactId != null && user.CustomerContactId != customerContact.Id)
                {
                    throw new UserFriendlyException("A user with this email address is already associated with another customer contact. Please use a different email address.");
                }
                if (!await IsUserInCustomerRole(user))
                {
                    await UserManager.AddToRoleAsync(user, StaticRoleNames.Tenants.Customer);
                }
                await UpdateUserFromCustomerContactAsync(user, customerContact);
                return user;
            }
            else
            {
                var nameParts = customerContact.Name.Split(' ');

                var newUser = new UserEditDto
                {
                    CustomerContactId = customerContact.Id,
                    EmailAddress = customerContact.Email,
                    Name = nameParts.First(),
                    Surname = nameParts.Length > 1 ? nameParts[1] : nameParts.First(),
                    PhoneNumber = customerContact.PhoneNumber,
                    UserName = customerContact.Email,
                    IsActive = true,
                    IsLockoutEnabled = true,
                    ShouldChangePasswordOnNextLogin = false,
                };

                var createOrUpdateUserInput = new CreateOrUpdateUserInput
                {
                    User = newUser,
                    AssignedRoleNames = new[] { StaticRoleNames.Tenants.Customer },
                    SendActivationEmail = sendEmail,
                    SetRandomPassword = true,
                };

                user = await _userCreatorService.CreateUser(createOrUpdateUserInput);

                return user;
            }
        }

        public async Task EnsureCanDeleteUser(User user)
        {
            if (user.CustomerContactId == null)
            {
                return;
            }

            var customerContact = await (await _customerContactRepository.GetQueryAsync())
                .FirstOrDefaultAsync(x => x.Id == user.CustomerContactId);

            if (customerContact != null)
            {
                customerContact.HasCustomerPortalAccess = false;
            }
        }

        public async Task EnsureCanDeleteCustomerContact(CustomerContact customerContact)
        {
            var users = await (await UserManager.GetQueryAsync()).Where(x => x.CustomerContactId == customerContact.Id).ToListAsync();
            foreach (var user in users)
            {
                if (await IsUserInCustomerRole(user))
                {
                    await UserManager.RemoveFromRoleAsync(user, StaticRoleNames.Tenants.Customer);
                }
                user.CustomerContactId = null;
            }
        }

        private async Task<bool> IsUserInCustomerRole(User user)
        {
            return await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Customer);
        }

        private async Task<bool> ShouldUserBeLinkedToCustomerContact(CustomerContact customerContact)
        {
            if (!await FeatureChecker.IsEnabledAsync(AppFeatures.CustomerPortal))
            {
                return false;
            }

            if (!customerContact.HasCustomerPortalAccess || string.IsNullOrEmpty(customerContact.Email))
            {
                return false;
            }

            return true;
        }

        private async Task UpdateUserFromCustomerContactAsync(User user, CustomerContact customerContact)
        {
            if (customerContact.Email.IsNullOrEmpty())
            {
                throw new UserFriendlyException("EmailAddress is required");
            }

            var nameParts = customerContact.Name.Split(' ');
            user.Name = nameParts.First();
            user.Surname = nameParts.Length > 1 ? nameParts[1] : nameParts.First();
            user.Title = customerContact.Title;
            user.EmailAddress = customerContact.Email;
            user.PhoneNumber = customerContact.PhoneNumber;
            user.CustomerContactId = customerContact.Id;

            (await UserManager.UpdateAsync(user)).CheckErrors(LocalizationManager);
        }

        private void UpdateCustomerContactFromUser(CustomerContact customerContact, User user)
        {
            customerContact.Name = $"{user.Name} {user.Surname}";
            customerContact.Title = user.Title;
            customerContact.Email = user.EmailAddress;
            customerContact.PhoneNumber = user.PhoneNumber;
        }
    }
}
