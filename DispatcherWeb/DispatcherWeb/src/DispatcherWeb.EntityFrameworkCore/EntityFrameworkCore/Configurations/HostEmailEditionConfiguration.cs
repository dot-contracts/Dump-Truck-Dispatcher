using DispatcherWeb.HostEmails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class HostEmailEditionConfiguration : IEntityTypeConfiguration<HostEmailEdition>
    {
        public void Configure(EntityTypeBuilder<HostEmailEdition> builder)
        {
            builder
                .HasOne(e => e.Edition)
                .WithMany()
                .HasForeignKey(e => e.EditionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
