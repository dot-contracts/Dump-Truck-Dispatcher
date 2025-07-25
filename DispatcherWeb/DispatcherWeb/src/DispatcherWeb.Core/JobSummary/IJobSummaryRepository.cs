using System.Collections.Generic;
using System.Threading.Tasks;

namespace DispatcherWeb.JobSummary
{
    public interface IJobSummaryRepository
    {
        Task<List<JobCycle>> GetOrderTrucksLoadJobTripCycles(int tenantId, int orderLineId);
    }
}
