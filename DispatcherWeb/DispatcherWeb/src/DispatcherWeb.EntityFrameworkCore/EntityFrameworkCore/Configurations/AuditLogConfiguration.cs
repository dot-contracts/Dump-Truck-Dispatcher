using Abp.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasIndex(e => new { e.ExecutionTime });

            builder.HasIndex(e => new { e.ImpersonatorTenantId, e.TenantId, e.UserId, e.ExecutionTime });
        }
    }
}
