using DispatcherWeb.LeaseHaulers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class LeaseHaulerDriverConfiguration : IEntityTypeConfiguration<LeaseHaulerDriver>
    {
        public void Configure(EntityTypeBuilder<LeaseHaulerDriver> builder)
        {
            builder
                .HasOne(x => x.Driver)
                .WithOne(x => x.LeaseHaulerDriver)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.LeaseHauler)
                .WithMany(x => x.LeaseHaulerDrivers)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
