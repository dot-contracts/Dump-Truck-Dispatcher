using DispatcherWeb.CustomerNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class CustomerNotificationTenantConfiguration : IEntityTypeConfiguration<CustomerNotificationTenant>
    {
        public void Configure(EntityTypeBuilder<CustomerNotificationTenant> builder)
        {
            builder
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
