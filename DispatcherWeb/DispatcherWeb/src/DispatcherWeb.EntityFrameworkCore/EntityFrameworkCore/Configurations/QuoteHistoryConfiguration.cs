using DispatcherWeb.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class QuoteHistoryConfiguration : IEntityTypeConfiguration<QuoteHistoryRecord>
    {
        public void Configure(EntityTypeBuilder<QuoteHistoryRecord> builder)
        {
            builder
                .HasOne(e => e.Quote)
                .WithMany()
                .HasForeignKey(e => e.QuoteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(e => e.FieldDiffs)
                .WithOne(e => e.QuoteHistoryRecord)
                .HasForeignKey(e => e.QuoteHistoryRecordId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.CreatorUser)
                .WithMany()
                .HasForeignKey(e => e.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
