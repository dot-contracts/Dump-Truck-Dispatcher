using DispatcherWeb.HaulZones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class HaulZoneConfiguration : IEntityTypeConfiguration<HaulZone>
    {
        public void Configure(EntityTypeBuilder<HaulZone> builder)
        {
            builder
                .HasOne(e => e.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(e => e.BillRatePerTon)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.MinPerLoad)
                .HasColumnType("decimal(18, 4)");

            builder
                .Property(e => e.PayRatePerTon)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
        }
    }
}
