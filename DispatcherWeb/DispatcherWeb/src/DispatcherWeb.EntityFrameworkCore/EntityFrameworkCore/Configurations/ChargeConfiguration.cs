using DispatcherWeb.Charges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class ChargeConfiguration : IEntityTypeConfiguration<Charge>
    {
        public void Configure(EntityTypeBuilder<Charge> builder)
        {
            builder
                .Property(e => e.Rate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.Quantity)
                .HasColumnType("decimal(18, 4)");

            builder
                .Property(e => e.ChargeAmount)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.OrderLine)
                .WithMany(e => e.Charges)
                .HasForeignKey(e => e.OrderLineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
