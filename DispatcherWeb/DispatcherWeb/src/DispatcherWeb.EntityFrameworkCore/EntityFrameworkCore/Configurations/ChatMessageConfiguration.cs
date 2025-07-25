using DispatcherWeb.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder
                .HasOne(e => e.TargetDriver)
                .WithMany()
                .HasForeignKey(e => e.TargetDriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.TargetTruck)
                .WithMany()
                .HasForeignKey(e => e.TargetTruckId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.TargetTrailer)
                .WithMany()
                .HasForeignKey(e => e.TargetTrailerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
