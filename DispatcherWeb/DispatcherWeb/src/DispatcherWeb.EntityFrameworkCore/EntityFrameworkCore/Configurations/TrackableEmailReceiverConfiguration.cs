using DispatcherWeb.Emailing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TrackableEmailReceiverConfiguration : IEntityTypeConfiguration<TrackableEmailReceiver>
    {
        public void Configure(EntityTypeBuilder<TrackableEmailReceiver> builder)
        {
            builder
                .HasMany(e => e.Events)
                .WithOne(e => e.TrackableEmailReceiver)
                .HasForeignKey(e => e.TrackableEmailReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.TrackableEmail)
                .WithMany(e => e.Receivers)
                .HasForeignKey(e => e.TrackableEmailId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
