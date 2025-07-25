using DispatcherWeb.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class QuoteLineConfiguration : IEntityTypeConfiguration<QuoteLine>
    {
        public void Configure(EntityTypeBuilder<QuoteLine> builder)
        {
            builder
                .Property(e => e.PricePerUnit)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.MaterialCostRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.FreightRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.FreightRateToPayDrivers)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.HourlyDriverPayRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.LeaseHaulerRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.FreightQuantity)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_4);

            builder
                .Property(e => e.MaterialQuantity)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_4);

            builder
                .HasOne(e => e.Quote)
                .WithMany(e => e.QuoteLines)
                .HasForeignKey(e => e.QuoteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.FreightItem)
                .WithMany(e => e.QuoteFreightItems)
                .HasForeignKey(e => e.FreightItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.MaterialItem)
                .WithMany(e => e.QuoteMaterialItems)
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
                .WithMany(e => e.LoadAtQuoteLines)
                .HasForeignKey(e => e.LoadAtId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.DeliverTo)
                .WithMany(e => e.DeliverToQuoteLines)
                .HasForeignKey(e => e.DeliverToId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.DriverPayTimeClassification)
                .WithMany(e => e.QuoteLines)
                .HasForeignKey(e => e.DriverPayTimeClassificationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
