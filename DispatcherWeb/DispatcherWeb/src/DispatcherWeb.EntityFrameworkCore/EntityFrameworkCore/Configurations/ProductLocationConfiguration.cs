using DispatcherWeb.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class ProductLocationConfiguration : IEntityTypeConfiguration<ProductLocation>
    {
        public void Configure(EntityTypeBuilder<ProductLocation> builder)
        {
            builder
                .Property(e => e.Cost)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .HasOne(e => e.Location)
                .WithMany(e => e.ProductLocations)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Item)
                .WithMany(e => e.ProductLocations)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
