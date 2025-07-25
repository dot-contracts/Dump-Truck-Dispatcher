using DispatcherWeb.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class ProductLocationPriceConfiguration : IEntityTypeConfiguration<ProductLocationPrice>
    {
        public void Configure(EntityTypeBuilder<ProductLocationPrice> builder)
        {
            builder
                .Property(e => e.PricePerUnit)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .HasOne(e => e.ProductLocation)
                .WithMany(e => e.ProductLocationPrices)
                .HasForeignKey(e => e.ProductLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.PricingTier)
                .WithMany(e => e.ProductLocationPrices)
                .HasForeignKey(e => e.PricingTierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
