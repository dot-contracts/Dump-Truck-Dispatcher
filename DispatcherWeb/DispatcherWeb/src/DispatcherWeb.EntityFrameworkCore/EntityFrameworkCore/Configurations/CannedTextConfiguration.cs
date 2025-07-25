using DispatcherWeb.CannedTexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class CannedTextConfiguration : IEntityTypeConfiguration<CannedText>
    {
        public void Configure(EntityTypeBuilder<CannedText> builder)
        {
            builder
                .HasOne(e => e.Office)
                .WithMany()
                .HasForeignKey(e => e.OfficeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
