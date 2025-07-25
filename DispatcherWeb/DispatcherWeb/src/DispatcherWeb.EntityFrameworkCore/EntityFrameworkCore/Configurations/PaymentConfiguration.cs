using DispatcherWeb.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder
                .HasOne(x => x.PaymentHeartlandKey)
                .WithMany()
                .HasForeignKey(x => x.PaymentHeartlandKeyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.AuthorizationUser)
                .WithMany()
                .HasForeignKey(x => x.AuthorizationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.AuthorizationCaptureUser)
                .WithMany()
                .HasForeignKey(x => x.AuthorizationCaptureUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.CancelOrRefundUser)
                .WithMany()
                .HasForeignKey(x => x.CancelOrRefundUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.PaymentHeartlandKey)
                .WithMany()
                .HasForeignKey(x => x.PaymentHeartlandKeyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
