using DispatcherWeb.CustomerNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class CustomerNotificationConfiguration : IEntityTypeConfiguration<CustomerNotification>
    {
        public void Configure(EntityTypeBuilder<CustomerNotification> builder)
        {
            builder
                .HasMany(e => e.Tenants)
                .WithOne(e => e.CustomerNotification)
                .HasForeignKey(e => e.CustomerNotificationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(e => e.Roles)
                .WithOne(e => e.CustomerNotification)
                .HasForeignKey(e => e.CustomerNotificationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(e => e.Editions)
                .WithOne(e => e.CustomerNotification)
                .HasForeignKey(e => e.CustomerNotificationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.CreatorUser)
                .WithMany()
                .HasForeignKey(e => e.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(e => e.Dismissions)
                .WithOne(e => e.CustomerNotification)
                .HasForeignKey(e => e.CustomerNotificationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
