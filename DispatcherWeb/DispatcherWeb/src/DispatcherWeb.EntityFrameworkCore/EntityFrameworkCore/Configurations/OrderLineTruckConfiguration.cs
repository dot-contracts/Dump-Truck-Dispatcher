using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class OrderLineTruckConfiguration : IEntityTypeConfiguration<OrderLineTruck>
    {
        public void Configure(EntityTypeBuilder<OrderLineTruck> builder)
        {
            builder
                .HasOne(e => e.Truck)
                .WithMany(e => e.OrderLineTrucksOfTruck)
                .HasForeignKey(e => e.TruckId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Trailer)
                .WithMany(e => e.OrderLineTrucksOfTrailer)
                .HasForeignKey(e => e.TrailerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(e => e.Utilization)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_2);
        }
    }
}
