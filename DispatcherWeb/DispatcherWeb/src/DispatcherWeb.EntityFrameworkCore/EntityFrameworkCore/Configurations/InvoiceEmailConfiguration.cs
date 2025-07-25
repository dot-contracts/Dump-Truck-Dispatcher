using DispatcherWeb.Emailing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class InvoiceEmailConfiguration : IEntityTypeConfiguration<InvoiceEmail>
    {
        public void Configure(EntityTypeBuilder<InvoiceEmail> builder)
        {
            builder
                .HasOne(e => e.Invoice)
                .WithMany(e => e.InvoiceEmails)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Email)
                .WithMany(e => e.InvoiceEmails)
                .HasForeignKey(e => e.EmailId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
