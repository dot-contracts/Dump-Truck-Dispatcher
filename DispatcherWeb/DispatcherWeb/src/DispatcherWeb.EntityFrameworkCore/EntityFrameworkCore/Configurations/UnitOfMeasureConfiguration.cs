using DispatcherWeb.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
    {
        public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
        {
            builder
                .HasOne(e => e.UnitOfMeasureBase)
                .WithMany()
                .HasForeignKey(e => e.UnitOfMeasureBaseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
