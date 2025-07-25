using DispatcherWeb.PayStatements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class PayStatementDriverDateConflictConfiguration : IEntityTypeConfiguration<PayStatementDriverDateConflict>
    {
        public void Configure(EntityTypeBuilder<PayStatementDriverDateConflict> builder)
        {
            builder
                .HasOne(e => e.Driver)
                .WithMany()
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.PayStatement)
                .WithMany(e => e.PayStatementDriverDateConflicts)
                .HasForeignKey(e => e.PayStatementId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
