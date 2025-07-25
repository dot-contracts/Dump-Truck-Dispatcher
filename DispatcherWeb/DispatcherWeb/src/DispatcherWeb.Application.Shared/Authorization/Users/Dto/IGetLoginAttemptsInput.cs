using Abp.Application.Services.Dto;

namespace DispatcherWeb.Authorization.Users.Dto
{
    public interface IGetLoginAttemptsInput : ISortedResultRequest
    {
        string Filter { get; set; }
    }
}