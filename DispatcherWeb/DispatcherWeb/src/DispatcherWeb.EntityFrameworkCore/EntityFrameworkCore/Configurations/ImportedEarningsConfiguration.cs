using DispatcherWeb.LuckStone;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class ImportedEarningsConfiguration : IEntityTypeConfiguration<ImportedEarnings>
    {
        public void Configure(EntityTypeBuilder<ImportedEarnings> builder)
        {
            builder
                .HasOne(x => x.Batch)
                .WithMany(x => x.ImportedEarnings)
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(e => e.FscAmount)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.HaulPayment)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.HaulPaymentRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.NetTons)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_4);
        }
    }
}
