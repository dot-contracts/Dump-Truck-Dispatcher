using DispatcherWeb.Trux;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TruxEarningsConfiguration : IEntityTypeConfiguration<TruxEarnings>
    {
        public void Configure(EntityTypeBuilder<TruxEarnings> builder)
        {
            builder
                .HasOne(x => x.Batch)
                .WithMany(x => x.TruxEarnings)
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(p => p.Id)
                .ValueGeneratedNever();

            builder
                .Property(e => e.Hours)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_4);

            builder
                .Property(e => e.Rate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.Tons)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.Total)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
        }
    }
}
