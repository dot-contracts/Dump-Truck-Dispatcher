using DispatcherWeb.Emailing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class QuoteEmailConfiguration : IEntityTypeConfiguration<QuoteEmail>
    {
        public void Configure(EntityTypeBuilder<QuoteEmail> builder)
        {
            builder
                .HasOne(e => e.Quote)
                .WithMany(e => e.QuoteEmails)
                .HasForeignKey(e => e.QuoteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Email)
                .WithMany(e => e.QuoteEmails)
                .HasForeignKey(e => e.EmailId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
