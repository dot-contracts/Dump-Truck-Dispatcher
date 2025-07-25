using System;
using System.Threading.Tasks;

namespace DispatcherWeb.DailyHistory
{
    public interface IDailyHistoryAppService
    {
        Task FillDailyHistoriesAsync();
        Task FillTenantDailyHistoryAsync(DateTime todayUtc);
        Task FillUserDailyHistoryAsync(DateTime todayUtc);
        Task FillTransactionDailyHistoryAsync(DateTime todayUtc);
    }
}
