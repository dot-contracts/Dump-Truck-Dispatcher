using System.Threading.Tasks;
using Abp.EntityFrameworkCore;
using DispatcherWeb.PayStatements;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore.Repositories
{
    public class PayStatementRepository : DispatcherWebRepositoryBase<PayStatement>, IPayStatementRepository
    {
        public PayStatementRepository(IDbContextProvider<DispatcherWebDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<int> DeleteWithAllChildRecordsAsync(int payStatementId, int tenantId)
        {
            var rowsAffected = 0;
            var context = await GetContextAsync();

            //await _payStatementDriverDateConflictRepository.DeleteAsync(x => x.PayStatementId == input.Id);
            rowsAffected += await context.Database.ExecuteSqlInterpolatedAsync($@"
                Delete from dbo.[PayStatementDriverDateConflict] where TenantId = {tenantId} and PayStatementId = {payStatementId}
            ");

            //await _employeeTimePayStatementTimeRepository.DeleteAsync(x => x.PayStatementTime.PayStatementDetail.PayStatementId == input.Id);
            rowsAffected += await context.Database.ExecuteSqlInterpolatedAsync($@"
                Delete from dbo.[EmployeeTimePayStatementTime] where TenantId = {tenantId} and PayStatementTimeId in (
                    select Id from dbo.[PayStatementTime] where TenantId = {tenantId} and PayStatementDetailId in (
                        select Id from dbo.[PayStatementDetail] where TenantId = {tenantId} and PayStatementId = {payStatementId}
                    )
                )
            ");

            //await _payStatementTimeRepository.DeleteAsync(x => x.PayStatementDetail.PayStatementId == input.Id);
            rowsAffected += await context.Database.ExecuteSqlInterpolatedAsync($@"
                Delete from dbo.[PayStatementTime] where TenantId = {tenantId} and PayStatementDetailId in (
                    select Id from dbo.[PayStatementDetail] where TenantId = {tenantId} and PayStatementId = {payStatementId}
                )
            ");

            //await _payStatementTicketRepository.DeleteAsync(x => x.PayStatementDetail.PayStatementId == input.Id);
            rowsAffected += await context.Database.ExecuteSqlInterpolatedAsync($@"
                Delete from dbo.[PayStatementTicket] where TenantId = {tenantId} and PayStatementDetailId in (
                    select Id from dbo.[PayStatementDetail] where TenantId = {tenantId} and PayStatementId = {payStatementId}
                )
            ");

            //await _payStatementDetailRepository.DeleteAsync(x => x.PayStatementId == input.Id);
            rowsAffected += await context.Database.ExecuteSqlInterpolatedAsync($@"
                Delete from dbo.[PayStatementDetail] where TenantId = {tenantId} and PayStatementId = {payStatementId}
            ");

            //await _payStatementRepository.DeleteAsync(input.Id);
            // We want to soft-delete the parent record. The above ones are not soft-delete entities.
            await DeleteAsync(payStatementId);

            return rowsAffected;
        }
    }
}
