using DispatcherWeb.LeaseHaulers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class LeaseHaulerTruckConfiguration : IEntityTypeConfiguration<LeaseHaulerTruck>
    {
        public void Configure(EntityTypeBuilder<LeaseHaulerTruck> builder)
        {
            builder
                .HasOne(x => x.Truck)
                .WithOne(x => x.LeaseHaulerTruck)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.LeaseHauler)
                .WithMany(x => x.LeaseHaulerTrucks)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
