using DispatcherWeb.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class DriverPushSubscriptionConfiguration : IEntityTypeConfiguration<DriverPushSubscription>
    {
        public void Configure(EntityTypeBuilder<DriverPushSubscription> builder)
        {
            builder
                .HasOne(e => e.Driver)
                .WithMany(e => e.DriverPushSubscriptions)
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.PushSubscription)
                .WithMany(e => e.DriverPushSubscriptions)
                .HasForeignKey(e => e.PushSubscriptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
