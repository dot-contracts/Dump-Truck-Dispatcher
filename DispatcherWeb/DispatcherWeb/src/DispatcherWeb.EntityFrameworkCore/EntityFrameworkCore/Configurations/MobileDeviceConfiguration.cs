using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class MobileDeviceConfiguration : IEntityTypeConfiguration<MobileDevice>
    {
        public void Configure(EntityTypeBuilder<MobileDevice> builder)
        {
            builder
                .HasOne(e => e.Truck)
                .WithMany(e => e.MobileDevices)
                .HasForeignKey(e => e.TruckId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
