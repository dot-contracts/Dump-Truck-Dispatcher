using DispatcherWeb.WebPush;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class FcmPushMessageConfiguration : IEntityTypeConfiguration<FcmPushMessage>
    {
        public void Configure(EntityTypeBuilder<FcmPushMessage> builder)
        {

            builder
                .HasOne(e => e.ReceiverUser)
                .WithMany()
                .HasForeignKey(e => e.ReceiverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.ReceiverDriver)
                .WithMany()
                .HasForeignKey(e => e.ReceiverDriverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
