using DispatcherWeb.Emailing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TrackableEmailEventConfiguration : IEntityTypeConfiguration<TrackableEmailEvent>
    {
        public void Configure(EntityTypeBuilder<TrackableEmailEvent> builder)
        {
            builder
                .HasOne(e => e.TrackableEmailReceiver)
                .WithMany(e => e.Events)
                .HasForeignKey(e => e.TrackableEmailReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.TrackableEmail)
                .WithMany(e => e.Events)
                .HasForeignKey(e => e.TrackableEmailId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
