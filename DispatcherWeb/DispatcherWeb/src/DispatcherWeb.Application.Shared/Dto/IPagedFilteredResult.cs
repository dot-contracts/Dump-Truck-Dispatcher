using Abp.Application.Services.Dto;

namespace DispatcherWeb.Dto
{
    public interface IPagedFilteredResult<T> : IPagedResult<T>, IHasFilteredCount
    {
    }
}
