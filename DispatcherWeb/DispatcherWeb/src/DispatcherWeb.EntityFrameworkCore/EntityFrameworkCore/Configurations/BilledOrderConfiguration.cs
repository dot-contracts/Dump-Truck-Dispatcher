using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class BilledOrderConfiguration : IEntityTypeConfiguration<BilledOrder>
    {
        public void Configure(EntityTypeBuilder<BilledOrder> builder)
        {
            builder
                .HasOne(e => e.Office)
                .WithMany()
                .HasForeignKey(e => e.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Order)
                .WithMany(e => e.BilledOrders)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
