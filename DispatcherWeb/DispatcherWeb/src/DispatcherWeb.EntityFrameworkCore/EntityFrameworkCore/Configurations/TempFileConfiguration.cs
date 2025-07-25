using DispatcherWeb.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class TempFileConfiguration : IEntityTypeConfiguration<TempFile>
    {
        public void Configure(EntityTypeBuilder<TempFile> builder)
        {
            builder.HasIndex(e => new { e.ExpirationDateTime });
        }
    }
}
