using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization.Roles;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Authorization.Users.Dto;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers.Dto;
using DispatcherWeb.Features;
using DispatcherWeb.TimeClassifications;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Drivers
{
    public class DriverUserLinkService : DispatcherWebDomainServiceBase, IDriverUserLinkService
    {
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly IRepository<EmployeeTimeClassification> _employeeTimeClassificationRepository;
        private readonly IDriverInactivatorService _driverInactivatorService;
        private readonly IUserCreatorService _userCreatorService;

        public DriverUserLinkService(
            IRepository<Driver> driverRepository,
            IRepository<Dispatch> dispatchRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            IRepository<EmployeeTimeClassification> employeeTimeClassificationRepository,
            IDriverInactivatorService driverInactivatorService,
            IUserCreatorService userCreatorService
        )
        {
            _driverRepository = driverRepository;
            _dispatchRepository = dispatchRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _employeeTimeClassificationRepository = employeeTimeClassificationRepository;
            _driverInactivatorService = driverInactivatorService;
            _userCreatorService = userCreatorService;
        }

        /// <summary>
        /// Should be called when a user is modified
        /// </summary>
        public async Task UpdateDriver(User user)
        {
            if (user.EmailAddress.IsNullOrEmpty())
            {
                throw new UserFriendlyException("EmailAddress is required");
            }
            if (user.Id == 0)
            {
                throw new ArgumentException("User should be saved first", nameof(user));
            }

            var linkedDrivers = await (await _driverRepository.GetQueryAsync())
                .Include(x => x.LeaseHaulerDriver)
                .Where(x => x.UserId == user.Id).ToListAsync();

            var userIsInDriverRole = await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Driver);
            var userIsInLeaseHaulerRole = await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.LeaseHaulerDriver);

            foreach (var linkedDriver in linkedDrivers)
            {
                if (linkedDriver.IsExternal && !userIsInLeaseHaulerRole
                    || !linkedDriver.IsExternal && !userIsInDriverRole)
                {
                    await EnsureCanUnlinkAsync(linkedDriver);
                    linkedDriver.UserId = null;
                }
                await UpdateDriverFromUserAsync(linkedDriver, user);

                //Is it possible for them to only want to inactivate the user, but not drivers? (e.g. to prevent them from using the driver app). If so, we should comment out the below block
                if (!user.IsActive)
                {
                    if (!linkedDriver.IsInactive)
                    {
                        linkedDriver.IsInactive = true;
                        await _driverInactivatorService.InactivateDriverAsync(linkedDriver, linkedDriver.LeaseHaulerDriver?.LeaseHaulerId);
                    }
                }
            }

            if (userIsInDriverRole && !linkedDrivers.Any(x => !x.IsExternal) && await ShouldCreateNewUserDriverLinks(await Session.GetTenantIdAsync()))
            {
                foreach (var linkedDriver in linkedDrivers)
                {
                    linkedDriver.IsInactive = true;
                    await _driverInactivatorService.InactivateDriverAsync(linkedDriver, linkedDriver.LeaseHaulerDriver?.LeaseHaulerId);
                }
                //await EnsureDriverEmailIsUniqueAsync(user.EmailAddress, null);
                var existingDriver = await (await _driverRepository.GetQueryAsync())
                    .Include(x => x.LeaseHaulerDriver)
                    .FirstOrDefaultAsync(d => d.EmailAddress == user.EmailAddress);
                if (existingDriver != null)
                {
                    if (existingDriver.UserId.HasValue && IsUserPermaLinkedToDriver(existingDriver))
                    {
                        //not an expected case, just in case of bad historical data
                        throw new UserFriendlyException("Driver with this email is already linked to another user and uses Driver Application");
                    }
                    existingDriver.UserId = user.Id;
                    await UpdateDriverFromUserAsync(existingDriver, user);
                    return;
                }

                if (user.OfficeId == null)
                {
                    throw new UserFriendlyException("Office is required");
                }

                var driver = new Driver
                {
                    UserId = user.Id,
                    EmailAddress = user.EmailAddress,
                    FirstName = user.Name,
                    LastName = user.Surname,
                    OfficeId = user.OfficeId,
                    CellPhoneNumber = user.PhoneNumber,
                };
                await _driverRepository.InsertAsync(driver);

                var defaultTimeClassifications = await GetDefaultTimeClassifications();
                foreach (var classification in defaultTimeClassifications)
                {
                    await _employeeTimeClassificationRepository.InsertAsync(new EmployeeTimeClassification
                    {
                        Driver = driver,
                        TimeClassificationId = classification.TimeClassificationId,
                        PayRate = classification.PayRate ?? 0,
                        IsDefault = classification.IsDefault,
                    });
                }
            }
        }

        public async Task<List<EmployeeTimeClassificationEditDto>> GetDefaultTimeClassifications()
        {
            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay) && await FeatureChecker.IsEnabledAsync(AppFeatures.DriverProductionPayFeature);

            var defaultTimeClassificationId = await SettingManager.GetSettingValueAsync<int>(AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId);
            var defaultTimeClassification = await (await _timeClassificationRepository.GetQueryAsync())
                .Where(x => x.Id == defaultTimeClassificationId)
                .Select(x => new
                {
                    x.DefaultRate,
                    x.IsProductionBased,
                }).FirstAsync();

            var result = new List<EmployeeTimeClassificationEditDto>
            {
                new EmployeeTimeClassificationEditDto
                {
                    Id = 0,
                    TimeClassificationId = defaultTimeClassificationId,
                    IsDefault = true,
                    PayRate = defaultTimeClassification.DefaultRate ?? 0,
                },
            };

            if (!defaultTimeClassification.IsProductionBased && allowProductionPay)
            {
                var productionPay = await (await _timeClassificationRepository.GetQueryAsync())
                    .Where(x => x.IsProductionBased)
                    .Select(x => new TimeClassificationDto
                    {
                        Id = x.Id,
                    })
                    .FirstAsync();

                result.Add(new EmployeeTimeClassificationEditDto
                {
                    Id = 0,
                    TimeClassificationId = productionPay.Id,
                });
            }

            return result;
        }

        /// <summary>
        /// Should be called when a driver is modified
        /// </summary>
        public async Task<User> UpdateUser(Driver driver, bool sendEmail = true, bool inactivateUser = false)
        {
            //driver email can be empty
            //driver can have id 0

            await EnsureDriverEmailIsUniqueAsync(driver.EmailAddress, driver);

            var linkedUser = driver.UserId.HasValue ? await (await UserManager.GetQueryAsync()).FirstOrDefaultAsync(x => x.Id == driver.UserId) : null;
            if (linkedUser != null)
            {
                if (!await IsUserInDriverRole(linkedUser) && IsUserPermaLinkedToDriver(driver))
                {
                    await UserManager.AddToRoleAsync(linkedUser, StaticRoleNames.Tenants.Driver);
                }
                await UpdateUserFromDriverAsync(linkedUser, driver, inactivateUser);
                return linkedUser;
            }

            if (!await ShouldCreateNewUserDriverLinks(driver.TenantId)
                || string.IsNullOrEmpty(driver.EmailAddress))
            {
                return null;
            }

            var user = await UserManager.FindByEmailAsync(driver.EmailAddress);
            if (user != null)
            {
                driver.UserId = user.Id;
                if (!await IsUserInDriverRole(user))
                {
                    await UserManager.AddToRoleAsync(user, StaticRoleNames.Tenants.Driver);
                }
                await UpdateUserFromDriverAsync(user, driver, inactivateUser);
                return user;
            }
            else
            {
                user = await _userCreatorService.CreateUser(new CreateOrUpdateUserInput
                {
                    User = new UserEditDto
                    {
                        EmailAddress = driver.EmailAddress,
                        Name = driver.FirstName,
                        Surname = driver.LastName,
                        PhoneNumber = driver.CellPhoneNumber,
                        OfficeId = driver.OfficeId,
                        UserName = driver.EmailAddress.Split("@").First(),
                        IsActive = true,
                        IsLockoutEnabled = true,
                        ShouldChangePasswordOnNextLogin = false,
                    },
                    AssignedRoleNames = new[] { StaticRoleNames.Tenants.Driver },
                    SendActivationEmail = sendEmail,
                    SetRandomPassword = true,
                });

                driver.UserId = user.Id;
                //UpdateUserFromDriver(user, driver);
                return user;
            }
        }

        public async Task EnsureCanDeleteUser(User user)
        {
            var drivers = await (await _driverRepository.GetQueryAsync())
                .Include(x => x.LeaseHaulerDriver)
                .Where(x => x.UserId == user.Id).ToListAsync();
            if (drivers.Any(IsUserPermaLinkedToDriver))
            {
                throw new UserFriendlyException("This user is already using Driver Application and can't be deleted", "You can mark user as inactive instead of deleting");
            }
            foreach (var driver in drivers)
            {
                if (await HasOpenDispatchesAsync(driver))
                {
                    throw new UserFriendlyException("This user has dispatches in their name. If you want to remove this user, you will first need to remove their dispatches.");
                }
                driver.UserId = null;
            }
        }

        public async Task EnsureCanDeleteDriver(Driver driver)
        {
            if (IsUserPermaLinkedToDriver(driver) || await HasOpenDispatchesAsync(driver))
            {
                throw new UserFriendlyException("You can’t delete this driver because it has data associated with it.");
            }
        }

        private async Task<bool> IsUserInDriverRole(User user)
        {
            return await UserManager.IsInRoleAsync(user, StaticRoleNames.Tenants.Driver);
        }

        private async Task<bool> ShouldCreateNewUserDriverLinks(int tenantId)
        {
            //return await SettingManager.DispatchViaDriverApplication();
            return (DispatchVia)await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.DispatchingAndMessaging.DispatchVia, tenantId) == DispatchVia.DriverApplication;
        }

        private bool IsUserPermaLinkedToDriver(Driver driver)
        {
            if (driver != null && driver.UserId.HasValue && driver.Guid.HasValue)
            {
                return true;
            }
            return false;
        }

        private async Task<bool> HasOpenDispatchesAsync(Driver driver)
        {
            return await (await _dispatchRepository.GetQueryAsync())
                .AnyAsync(x => x.DriverId == driver.Id && !Dispatch.ClosedDispatchStatuses.Contains(x.Status));
        }

        public async Task EnsureCanUnlinkAsync(Driver driver)
        {
            if (IsUserPermaLinkedToDriver(driver))
            {
                if (driver.IsExternal)
                {
                    throw new UserFriendlyException("This user is already using Driver Application and can't be unlinked from the driver", "You can mark user as inactive instead of unchecking 'Enable for driver application'");
                }
                else
                {
                    throw new UserFriendlyException("This user is already using Driver Application and can't be unlinked from the driver", "You can mark user as inactive instead of removing Driver role");
                }
            }
            if (await HasOpenDispatchesAsync(driver))
            {
                if (driver.IsExternal)
                {
                    throw new UserFriendlyException("This driver has dispatches in their name. If you want to uncheck 'Enable for driver application', you will first need to remove their dispatches.");
                }
                else
                {
                    throw new UserFriendlyException("This driver has dispatches in their name. If you want to remove their driver role, you will first need to remove their dispatches.");
                }
            }
        }

        private async Task UpdateUserFromDriverAsync(User user, Driver driver, bool inactivateUser)
        {
            user.Name = driver.FirstName;
            user.Surname = driver.LastName;
            if (driver.EmailAddress.IsNullOrEmpty())
            {
                throw new UserFriendlyException("EmailAddress is required");
            }
            user.EmailAddress = driver.EmailAddress;
            user.PhoneNumber = driver.CellPhoneNumber;
            user.OfficeId = driver.OfficeId;

            var otherDrivers = await (await _driverRepository.GetQueryAsync())
                .Include(x => x.LeaseHaulerDriver)
                .Where(x => x.UserId == user.Id && x.Id != driver.Id).ToListAsync();

            if (!driver.IsInactive) //driver is active
            {
                user.IsActive = true;
                foreach (var otherDriver in otherDrivers)
                {
                    otherDriver.IsInactive = true;
                    await _driverInactivatorService.InactivateDriverAsync(otherDriver, otherDriver.LeaseHaulerDriver?.LeaseHaulerId);
                }
            }
            else //driver is inactive
            {
                if (otherDrivers.Any(x => !x.IsInactive) || !inactivateUser) //any other driver is still active
                {
                    //keep user active
                }
                else
                {
                    //if no other drivers, or all other drivers are also inactive, then inactivate user
                    user.IsActive = false;
                }
            }

            (await UserManager.UpdateAsync(user)).CheckErrors(LocalizationManager);
        }

        private async Task UpdateDriverFromUserAsync(Driver driver, User user)
        {
            driver.FirstName = user.Name;
            driver.LastName = user.Surname;
            await EnsureDriverEmailIsUniqueAsync(user.EmailAddress, driver);
            driver.EmailAddress = user.EmailAddress;
            driver.CellPhoneNumber = user.PhoneNumber;
            driver.OfficeId = user.OfficeId;
        }

        private async Task EnsureDriverEmailIsUniqueAsync(string emailAddress, Driver driverToIgnore)
        {
            var driverToIgnoreId = driverToIgnore?.Id;
            var leaseHaulerId = driverToIgnore?.LeaseHaulerDriver?.LeaseHaulerId;
            if (emailAddress.IsNullOrEmpty())
            {
                return;
            }
            if (await (await _driverRepository.GetQueryAsync())
                .AnyAsync(d => d.Id != driverToIgnoreId
                        && d.IsExternal == driverToIgnore.IsExternal
                        && d.LeaseHaulerDriver.LeaseHaulerId == leaseHaulerId
                        && d.EmailAddress == emailAddress))
            {
                throw new UserFriendlyException("Another driver already uses this email");
            }
        }

        public async Task<List<DriverCompanyDto>> GetCompanyListForUserDrivers(GetCompanyListForUserDriversInput input)
        {
            var drivers = await (await _driverRepository.GetQueryAsync())
                .Where(x => input.UserId == x.UserId)
                .Select(x => new DriverCompanyDto
                {
                    DriverId = x.Id,
                    CompanyName = x.LeaseHaulerDriver == null ? "Internal driver" : "Lease Hauler: " + x.LeaseHaulerDriver.LeaseHauler.Name,
                    DateOfHire = x.DateOfHire,
                    TerminationDate = x.TerminationDate,
                    IsActive = !x.IsInactive,
                })
                .OrderBy(x => x.CompanyName)
                .ToListAsync();

            return drivers;
        }
    }
}
