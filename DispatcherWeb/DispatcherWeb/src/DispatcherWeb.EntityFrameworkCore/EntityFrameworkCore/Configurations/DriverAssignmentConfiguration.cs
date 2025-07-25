using DispatcherWeb.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class DriverAssignmentConfiguration : IEntityTypeConfiguration<DriverAssignment>
    {
        public void Configure(EntityTypeBuilder<DriverAssignment> builder)
        {
            builder
                .HasOne(e => e.Truck)
                .WithMany(e => e.DriverAssignments)
                .HasForeignKey(e => e.TruckId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Office)
                .WithMany()
                .HasForeignKey(e => e.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Driver)
                .WithMany(e => e.DriverAssignments)
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
