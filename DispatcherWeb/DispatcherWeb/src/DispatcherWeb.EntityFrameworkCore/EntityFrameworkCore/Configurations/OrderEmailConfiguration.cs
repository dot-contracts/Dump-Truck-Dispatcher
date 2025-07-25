using DispatcherWeb.Emailing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class OrderEmailConfiguration : IEntityTypeConfiguration<OrderEmail>
    {
        public void Configure(EntityTypeBuilder<OrderEmail> builder)
        {
            builder
                .HasOne(e => e.Order)
                .WithMany(e => e.OrderEmails)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Email)
                .WithMany(e => e.OrderEmails)
                .HasForeignKey(e => e.EmailId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
