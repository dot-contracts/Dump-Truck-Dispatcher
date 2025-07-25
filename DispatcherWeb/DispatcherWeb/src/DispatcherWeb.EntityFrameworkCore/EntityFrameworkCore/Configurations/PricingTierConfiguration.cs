using DispatcherWeb.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class PricingTierConfiguration : IEntityTypeConfiguration<PricingTier>
    {
        public void Configure(EntityTypeBuilder<PricingTier> builder)
        {
            builder
                .HasMany(e => e.ProductLocationPrices)
                .WithOne(e => e.PricingTier)
                .HasForeignKey(e => e.PricingTierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(e => e.Customers)
                .WithOne(e => e.PricingTier)
                .HasForeignKey(e => e.PricingTierId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
