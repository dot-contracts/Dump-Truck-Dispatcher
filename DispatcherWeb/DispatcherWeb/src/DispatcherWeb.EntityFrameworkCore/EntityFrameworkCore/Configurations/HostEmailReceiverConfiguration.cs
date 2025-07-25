using DispatcherWeb.HostEmails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class HostEmailReceiverConfiguration : IEntityTypeConfiguration<HostEmailReceiver>
    {
        public void Configure(EntityTypeBuilder<HostEmailReceiver> builder)
        {
            builder
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
