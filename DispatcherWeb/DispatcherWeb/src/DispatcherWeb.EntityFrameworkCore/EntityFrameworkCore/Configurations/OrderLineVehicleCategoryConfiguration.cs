using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class OrderLineVehicleCategoryConfiguration : IEntityTypeConfiguration<OrderLineVehicleCategory>
    {
        public void Configure(EntityTypeBuilder<OrderLineVehicleCategory> builder)
        {
            builder
                .HasOne(e => e.OrderLine)
                .WithMany(e => e.OrderLineVehicleCategories)
                .HasForeignKey(e => e.OrderLineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.VehicleCategory)
                .WithMany()
                .HasForeignKey(e => e.VehicleCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
