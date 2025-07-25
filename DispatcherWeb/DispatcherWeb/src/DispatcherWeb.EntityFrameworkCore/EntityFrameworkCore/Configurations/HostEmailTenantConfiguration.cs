using DispatcherWeb.HostEmails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class HostEmailTenantConfiguration : IEntityTypeConfiguration<HostEmailTenant>
    {
        public void Configure(EntityTypeBuilder<HostEmailTenant> builder)
        {
            builder
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
