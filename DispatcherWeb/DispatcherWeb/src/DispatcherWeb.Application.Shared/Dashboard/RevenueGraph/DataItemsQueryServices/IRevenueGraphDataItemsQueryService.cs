using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using DispatcherWeb.Dashboard.RevenueGraph.Dto;

namespace DispatcherWeb.Dashboard.RevenueGraph.DataItemsQueryServices
{
    public interface IRevenueGraphDataItemsQueryService : ITransientDependency
    {
        Task<IEnumerable<RevenueGraphDataItem>> GetRevenueGraphDataItemsAsync(PeriodInput input);
    }
}