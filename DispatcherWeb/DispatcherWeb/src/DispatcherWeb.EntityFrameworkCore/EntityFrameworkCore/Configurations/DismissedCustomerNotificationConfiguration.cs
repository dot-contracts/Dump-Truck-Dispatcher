using DispatcherWeb.CustomerNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class DismissedCustomerNotificationConfiguration : IEntityTypeConfiguration<DismissedCustomerNotification>
    {
        public void Configure(EntityTypeBuilder<DismissedCustomerNotification> builder)
        {
            builder
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
