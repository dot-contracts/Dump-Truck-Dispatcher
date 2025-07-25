using DispatcherWeb.LeaseHaulerRequests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class RequestedLeaseHaulerTruckConfiguration : IEntityTypeConfiguration<RequestedLeaseHaulerTruck>
    {
        public void Configure(EntityTypeBuilder<RequestedLeaseHaulerTruck> builder)
        {
            builder
                .HasOne(x => x.LeaseHaulerRequest)
                .WithMany(x => x.RequestedLeaseHaulerTrucks)
                .HasForeignKey(x => x.LeaseHaulerRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Truck)
                .WithMany(x => x.RequestedLeaseHaulerTrucks)
                .HasForeignKey(x => x.TruckId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Driver)
                .WithMany(x => x.RequestedLeaseHaulerTrucks)
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
