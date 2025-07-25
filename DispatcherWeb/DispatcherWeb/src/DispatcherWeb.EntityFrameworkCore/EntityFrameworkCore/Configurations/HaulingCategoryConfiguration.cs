using DispatcherWeb.HaulingCategories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class HaulingCategoryConfiguration : IEntityTypeConfiguration<HaulingCategory>
    {
        public void Configure(EntityTypeBuilder<HaulingCategory> builder)
        {
            builder
                .Property(e => e.MinimumBillableUnits)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.LeaseHaulerRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .HasOne(e => e.Item)
                .WithMany(e => e.HaulingCategories)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.TruckCategory)
                .WithMany()
                .HasForeignKey(e => e.TruckCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(e => e.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
