using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
    {
        public void Configure(EntityTypeBuilder<OrderLine> builder)
        {
            builder
                .Property(e => e.MaterialQuantity)
                .HasColumnType("decimal(18, 4)");

            builder
                .Property(e => e.FreightQuantity)
                .HasColumnType("decimal(18, 4)");

            builder
                .Property(e => e.MaterialPricePerUnit)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.MaterialCostRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.FreightPricePerUnit)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.FreightRateToPayDrivers)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.HourlyDriverPayRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.MaterialPrice)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.FreightPrice)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.LeaseHaulerRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.EstimatedAmount)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .HasOne(e => e.FreightItem)
                .WithMany(e => e.OrderLineFreightItems)
                .HasForeignKey(e => e.FreightItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.MaterialItem)
                .WithMany(e => e.OrderLineMaterialItems)
                .HasForeignKey(e => e.MaterialItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.MaterialUom)
                .WithMany()
                .HasForeignKey(e => e.MaterialUomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.FreightUom)
                .WithMany()
                .HasForeignKey(e => e.FreightUomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.LoadAt)
                .WithMany()
                .HasForeignKey(e => e.LoadAtId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.DeliverTo)
                .WithMany()
                .HasForeignKey(e => e.DeliverToId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.QuoteLine)
                .WithMany(e => e.OrderLines)
                .HasForeignKey(e => e.QuoteLineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(e => e.LeaseHaulerRequests)
                .WithOne(e => e.OrderLine)
                .HasForeignKey(e => e.OrderLineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.DriverPayTimeClassification)
                .WithMany(e => e.OrderLines)
                .HasForeignKey(e => e.DriverPayTimeClassificationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
