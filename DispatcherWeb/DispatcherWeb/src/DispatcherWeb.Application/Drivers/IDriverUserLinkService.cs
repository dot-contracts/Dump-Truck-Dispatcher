using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Drivers.Dto;

namespace DispatcherWeb.Drivers
{
    public interface IDriverUserLinkService : IApplicationService
    {
        Task UpdateDriver(User user);
        Task<User> UpdateUser(Driver driver, bool sendEmail = true, bool inactivateUser = false);
        Task EnsureCanDeleteDriver(Driver driver);
        Task EnsureCanDeleteUser(User user);
        Task EnsureCanUnlinkAsync(Driver driver);
        Task<List<EmployeeTimeClassificationEditDto>> GetDefaultTimeClassifications();
        Task<List<DriverCompanyDto>> GetCompanyListForUserDrivers(GetCompanyListForUserDriversInput input);
    }
}