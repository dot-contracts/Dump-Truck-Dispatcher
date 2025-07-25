using System.Linq;
using System.Threading.Tasks;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.EntityFrameworkCore.Uow;
using Abp.Timing;
using DispatcherWeb.Auditing;
using DispatcherWeb.Auditing.Dto;
using DispatcherWeb.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Auditing
{
    public class AuditLogAppService_Tests : AppTestBase
    {
        private readonly IAuditLogAppService _auditLogAppService;

        public AuditLogAppService_Tests()
        {
            _auditLogAppService = Resolve<IAuditLogAppService>();
        }

        [Fact]
        public async Task Should_Get_Audit_Logs()
        {
            //Arrange
            UsingDbContext(
                context =>
                {
                    context.AuditLogs.Add(
                        new AuditLog
                        {
                            TenantId = Session.TenantId,
                            UserId = Session.UserId,
                            ServiceName = "ServiceName-Test-1",
                            MethodName = "MethodName-Test-1",
                            Parameters = "{}",
                            ExecutionTime = Clock.Now.AddMinutes(-1),
                            ExecutionDuration = 123,
                        });

                    context.AuditLogs.Add(
                        new AuditLog
                        {
                            TenantId = Session.TenantId,
                            ServiceName = "ServiceName-Test-2",
                            MethodName = "MethodName-Test-2",
                            Parameters = "{}",
                            ExecutionTime = Clock.Now,
                            ExecutionDuration = 456,
                        });
                });

            //Act
            var output = await _auditLogAppService.GetAuditLogs(new GetAuditLogsInput
            {
                StartDate = Clock.Now.AddMinutes(-10),
                EndDate = Clock.Now.AddMinutes(10),
            });

            output.TotalCount.ShouldBe(2);

            output.Items[0].ServiceName.ShouldBe("ServiceName-Test-2");
            output.Items[0].UserName.ShouldBe(null);

            output.Items[1].ServiceName.ShouldBe("ServiceName-Test-1");

            output.Items[1].UserName.ShouldBe(AbpUserBase.AdminUserName, StringCompareShould.IgnoreCase);
        }

        [Fact]
        public async Task Should_Get_Entity_Changes()
        {
            LoginAsHostAdmin();

            //Arrange
            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var context = await UnitOfWorkManager.Current.GetDbContextAsync<DispatcherWebDbContext>();
                var aTenant = await context.Tenants.FirstOrDefaultAsync();

                aTenant.Name = "changed name";

                var aUser = await context.Users.FirstOrDefaultAsync(u => u.TenantId == null);

                if (aUser != null)
                {
                    aUser.Name = "changed name";
                }

                await context.SaveChangesAsync();
            });

            //Act
            var entityChangeList = await _auditLogAppService.GetEntityChanges(new GetEntityChangeInput
            {
                StartDate = Clock.Now.AddMinutes(-10),
                EndDate = Clock.Now.AddMinutes(10),
            });

            entityChangeList.TotalCount.ShouldBe(2);
        }
    }
}
