using DispatcherWeb.LeaseHaulerStatements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class LeaseHaulerStatementConfiguration : IEntityTypeConfiguration<LeaseHaulerStatement>
    {
        public void Configure(EntityTypeBuilder<LeaseHaulerStatement> builder)
        {
            builder
                .HasMany(e => e.LeaseHaulerStatementTickets)
                .WithOne(e => e.LeaseHaulerStatement)
                .HasForeignKey(e => e.LeaseHaulerStatementId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
