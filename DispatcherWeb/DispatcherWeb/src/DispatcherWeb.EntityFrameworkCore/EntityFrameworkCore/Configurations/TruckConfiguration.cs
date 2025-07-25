using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TruckConfiguration : IEntityTypeConfiguration<Truck>
    {
        public void Configure(EntityTypeBuilder<Truck> builder)
        {
            builder
                .HasOne(e => e.DefaultDriver)
                .WithMany(e => e.DefaultTrucks)
                .HasForeignKey(e => e.DefaultDriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.VehicleCategory)
                .WithMany()
                .HasForeignKey(e => e.VehicleCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.CurrentTrailer)
                .WithMany(e => e.CurrentTractors)
                .HasForeignKey(e => e.CurrentTrailerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(e => e.CargoCapacity)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_2);

            builder
                .Property(e => e.CargoCapacityCyds)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_2);

            builder
                .Property(e => e.CurrentHours)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_2);
        }
    }
}
