using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.CustomerNotifications.Dto;

namespace DispatcherWeb.CustomerNotifications
{
    public interface ICustomerNotificationAppService : IApplicationService
    {
        Task<CustomerNotificationEditDto> GetCustomerNotificationForEdit(NullableIdDto input);
    }
}
