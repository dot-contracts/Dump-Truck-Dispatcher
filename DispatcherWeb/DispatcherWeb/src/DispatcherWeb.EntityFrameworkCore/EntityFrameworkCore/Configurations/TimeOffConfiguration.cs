using DispatcherWeb.TimeOffs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TimeOffConfiguration : IEntityTypeConfiguration<TimeOff>
    {
        public void Configure(EntityTypeBuilder<TimeOff> builder)
        {
            builder
                .Property(e => e.RequestedHours)
                .HasColumnType("decimal(18, 1)");

            builder
                .HasOne(e => e.Driver)
                .WithMany(e => e.TimeOffs)
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
