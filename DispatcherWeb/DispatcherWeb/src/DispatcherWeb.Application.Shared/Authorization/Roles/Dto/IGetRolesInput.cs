using Abp.Application.Services.Dto;

namespace DispatcherWeb.Authorization.Roles.Dto
{
    public interface IGetRolesInput : ISortedResultRequest
    {
        string Permission { get; set; }
    }
}
