using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using DispatcherWeb.Customers.Dto;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Customers
{
    public interface ICustomerAppService : IApplicationService
    {
        Task<PagedResultDto<CustomerDto>> GetCustomers(GetCustomersInput input);

        Task<PagedResultDto<SelectListDto>> GetActiveCustomersSelectList(GetActiveCustomersSelectListInput input);

        Task<ListResultDto<SelectListDto>> GetCustomersByIdsSelectList(GetItemsByIdsInput input);

        Task<CustomerEditDto> GetCustomerForEdit(NullableIdNameDto input);

        Task<CustomerEditDto> EditCustomer(CustomerEditDto model);

        Task<bool> CanDeleteCustomer(EntityDto input);

        Task DeleteCustomer(EntityDto input);

        Task<PagedResultDto<CustomerContactDto>> GetCustomerContacts(GetCustomerContactsInput input);

        Task<ListResultDto<SelectListDto>> GetCustomerContactsByIdsSelectList(GetItemsByIdsInput input);

        Task<ListResultDto<SelectListDto>> GetContactsForCustomer(NullableIdDto input);

        Task<CustomerContactEditDto> GetCustomerContactForEdit(GetCustomerContactForEditInput input);

        Task<int> EditCustomerContact(CustomerContactEditDto model);

        Task<bool> CanDeleteCustomerContact(EntityDto input);

        Task DeleteCustomerContact(EntityDto input);

        Task MergeCustomers(DataMergeInput input);

        Task MergeCustomerContacts(DataMergeInput input);

        Task<CustomerEditDto> GetCustomerIfExistsOrNull(GetCustomerIdIfExistsOrNullInput input);
    }
}
