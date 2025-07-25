using DispatcherWeb.CustomerNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class CustomerNotificationEditionConfiguration : IEntityTypeConfiguration<CustomerNotificationEdition>
    {
        public void Configure(EntityTypeBuilder<CustomerNotificationEdition> builder)
        {
            builder
                .HasOne(e => e.Edition)
                .WithMany()
                .HasForeignKey(e => e.EditionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
