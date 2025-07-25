using DispatcherWeb.LeaseHaulerStatements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class LeaseHaulerStatementTicketConfiguration : IEntityTypeConfiguration<LeaseHaulerStatementTicket>
    {
        public void Configure(EntityTypeBuilder<LeaseHaulerStatementTicket> builder)
        {
            builder
                .HasIndex(x => x.TicketId)
                .IsUnique();

            builder
                .HasOne(x => x.Ticket)
                .WithOne(x => x.LeaseHaulerStatementTicket)
                .HasForeignKey<LeaseHaulerStatementTicket>(x => x.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(e => e.BrokerFee)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.ExtendedAmount)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .Property(e => e.Quantity)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_4);

            builder
                .Property(e => e.Rate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
        }
    }
}
