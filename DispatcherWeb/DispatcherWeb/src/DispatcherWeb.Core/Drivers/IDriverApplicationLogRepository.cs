using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;

namespace DispatcherWeb.Drivers
{
    public interface IDriverApplicationLogRepository : IRepository<DriverApplicationLog>
    {
        Task<int> DeleteLogsEarlierThanAsync(DateTime date);
        Task<int> DeleteOldLogsAsync();
    }
}
