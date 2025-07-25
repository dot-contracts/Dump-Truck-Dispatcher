using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Extensions;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Drivers;
using DispatcherWeb.Imports.RowReaders;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    public class ImportEmployeesAppService : ImportDataBaseAppService<EmployeeImportRow>, IImportEmployeesAppService
    {
        private readonly IRepository<Driver> _driverRepository;
        private readonly IDriverUserLinkService _driverUserLinkService;
        private readonly UserManager _userManager;
        private int? _officeId = null;

        public ImportEmployeesAppService(
            IRepository<Driver> driverRepository,
            IDriverUserLinkService driverUserLinkService,
            UserManager userManager
        )
        {
            _driverRepository = driverRepository;
            _driverUserLinkService = driverUserLinkService;
            _userManager = userManager;
        }

        protected override async Task<bool> CacheResourcesBeforeImportAsync(IImportReader reader)
        {
            _officeId = await OfficeResolver.GetOfficeIdAsync(_userId.ToString());
            if (_officeId == null)
            {
                _result.NotFoundOffices.Add(_userId.ToString());
                return false;
            }

            return await base.CacheResourcesBeforeImportAsync(reader);
        }

        protected override async Task<bool> ImportRowAsync(EmployeeImportRow row)
        {
            var (firstName, middle, lastName) = ParseName(row);

            var email = row.Email;
            if (email.IsNullOrEmpty())
            {
                email = null;
            }

            var hasUsersWithSameNameOrEmail = await (await _userManager.GetQueryAsync())
                .AnyAsync(x => (x.Name == firstName && x.Surname == lastName)
                    || email != null && x.EmailAddress == email);

            var hasDriversWithSameNameOrEmail = await (await _driverRepository.GetQueryAsync())
                .AnyAsync(x => (x.FirstName == firstName && x.LastName == lastName)
                    || email != null && x.EmailAddress == email);

            if (hasUsersWithSameNameOrEmail || hasDriversWithSameNameOrEmail)
            {
                row.AddParseErrorIfNotExist("Name", $"Driver or User already exists with the same name or email", typeof(string));
                return false;
            }

            var driver = new Driver
            {
                IsInactive = false,
                FirstName = firstName,
                LastName = lastName,
                EmailAddress = row.Email,
                OfficeId = _officeId,
                CellPhoneNumber = row.Phone,
                Address = row.Address,
                City = row.City,
                State = row.State,
                ZipCode = row.Zip,
                OrderNotifyPreferredFormat = row.NotifyPreferredFormat ?? OrderNotifyPreferredFormat.Neither,
                TenantId = _tenantId,
            };
            await _driverRepository.InsertAsync(driver);
            await CurrentUnitOfWork.SaveChangesAsync();

            if (email == null)
            {
                row.AddParseErrorIfNotExist("Email", $"Email is empty for user {row.Name}", typeof(string));
                return true;
            }

            try
            {
                var sendEmail = row.SendEmail;
                await _driverUserLinkService.UpdateUser(driver, sendEmail);
            }
            catch (Exception e)
            {
                row.AddParseErrorIfNotExist("-", e.Message, typeof(string));
                return false;
            }

            return true;
        }

        private static (string firstName, string middle, string lastName) ParseName(EmployeeImportRow row)
        {
            if (row.Name.Contains(","))
            {
                //assume LastName, FirstName MiddleInitial format
                var commaParts = row.Name.Split(",");
                var lastName = string.Join(",", commaParts.SkipLast(1));
                var parts = commaParts.Last().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Last().Length == 1)
                {
                    var firstName = string.Join(" ", parts.SkipLast(1));
                    var middleName = parts.Last();
                    return (firstName, middleName, lastName);
                }
                else
                {
                    var firstName = string.Join(" ", parts);
                    return (firstName, null, lastName);
                }
            }
            else
            {
                //assume FirstName MiddleInitial LastName format
                var parts = row.Name.Split(" ");
                if (parts.Length == 1)
                {
                    return (parts.First(), null, "-");
                }

                if (parts.Length == 2)
                {
                    return (parts.First(), null, parts.Last());
                }

                if (parts.Length == 3 && parts[1].EndsWith("."))
                {
                    return (parts.First(), parts[1], parts.Last());
                }

                return (string.Join(" ", parts.Take(parts.Length - 1)), null, parts.Last());
            }
        }

        protected override bool IsRowEmpty(EmployeeImportRow row)
        {
            return row.Name.IsNullOrEmpty();
        }
    }
}
