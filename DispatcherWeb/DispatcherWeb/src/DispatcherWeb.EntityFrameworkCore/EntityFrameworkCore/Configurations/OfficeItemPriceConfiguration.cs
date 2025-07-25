using DispatcherWeb.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class OfficeItemPriceConfiguration : IEntityTypeConfiguration<OfficeItemPrice>
    {
        public void Configure(EntityTypeBuilder<OfficeItemPrice> builder)
        {
            builder
                .Property(e => e.PricePerUnit)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
             .Property(e => e.FreightRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .HasOne(e => e.Office)
                .WithMany()
                .HasForeignKey(e => e.OfficeId)
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
                .HasOne(e => e.Item)
                .WithMany(e => e.OfficeItemPrices)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
