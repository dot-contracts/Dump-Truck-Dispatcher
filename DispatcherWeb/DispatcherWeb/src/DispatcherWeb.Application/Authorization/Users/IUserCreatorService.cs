using System.Threading.Tasks;
using Abp.Application.Services;
using DispatcherWeb.Authorization.Users.Dto;

namespace DispatcherWeb.Authorization.Users
{
    //this interface is not in DriverWeb.Application.Shared layer because User type can't be accessed from that layer
    public interface IUserCreatorService : IApplicationService
    {
        Task<User> CreateUser(CreateOrUpdateUserInput input);
        Task<ActivationEmailTemplateDto> GetActivationEmailTemplate(string[] roleNames);
        Task<string> ResetUserPassword(User user);
    }
}
