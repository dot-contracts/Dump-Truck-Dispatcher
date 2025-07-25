using DispatcherWeb.HaulingCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class HaulingCategoryPriceConfiguration : IEntityTypeConfiguration<HaulingCategoryPrice>
    {
        public void Configure(EntityTypeBuilder<HaulingCategoryPrice> builder)
        {
            builder
                .Property(e => e.PricePerUnit)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .HasOne(e => e.HaulingCategory)
                .WithMany(e => e.HaulingCategoryPrices)
                .HasForeignKey(e => e.HaulingCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.PricingTier)
                .WithMany()
                .HasForeignKey(e => e.PricingTierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
