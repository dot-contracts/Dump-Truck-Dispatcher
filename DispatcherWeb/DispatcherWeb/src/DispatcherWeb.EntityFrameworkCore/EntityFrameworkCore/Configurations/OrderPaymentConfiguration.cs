using DispatcherWeb.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class OrderPaymentConfiguration : IEntityTypeConfiguration<OrderPayment>
    {
        public void Configure(EntityTypeBuilder<OrderPayment> builder)
        {
            builder
                .HasOne(e => e.Payment)
                .WithMany(e => e.OrderPayments)
                .HasForeignKey(e => e.PaymentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Order)
                .WithMany(e => e.OrderPayments)
                .HasForeignKey(e => e.OrderId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
