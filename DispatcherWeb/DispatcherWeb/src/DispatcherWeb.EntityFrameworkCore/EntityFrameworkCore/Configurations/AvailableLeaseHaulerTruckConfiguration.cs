using DispatcherWeb.LeaseHaulerRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class AvailableLeaseHaulerTruckConfiguration : IEntityTypeConfiguration<AvailableLeaseHaulerTruck>
    {
        public void Configure(EntityTypeBuilder<AvailableLeaseHaulerTruck> builder)
        {
            builder
                .HasOne(x => x.LeaseHaulerRequest)
                .WithMany(x => x.AvailableLeaseHaulerTrucks)
                .HasForeignKey(x => x.LeaseHaulerRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.LeaseHauler)
                .WithMany(x => x.AvailableLeaseHaulerTrucks)
                .HasForeignKey(x => x.LeaseHaulerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Office)
                .WithMany()
                .HasForeignKey(x => x.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Truck)
                .WithMany(x => x.AvailableLeaseHaulerTrucks)
                .HasForeignKey(x => x.TruckId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Driver)
                .WithMany()
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
