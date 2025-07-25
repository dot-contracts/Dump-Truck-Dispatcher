using DispatcherWeb.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class QuoteLineVehicleCategoryConfiguration : IEntityTypeConfiguration<QuoteLineVehicleCategory>
    {
        public void Configure(EntityTypeBuilder<QuoteLineVehicleCategory> builder)
        {
            builder
                .HasOne(e => e.QuoteLine)
                .WithMany(e => e.QuoteLineVehicleCategories)
                .HasForeignKey(e => e.QuoteLineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.VehicleCategory)
                .WithMany()
                .HasForeignKey(e => e.VehicleCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
