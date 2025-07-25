using DispatcherWeb.ActiveReports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class ActiveReportCategoryConfiguration : IEntityTypeConfiguration<ActiveReportCategory>
    {
        public void Configure(EntityTypeBuilder<ActiveReportCategory> builder)
        {
            builder.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
        }
    }
}
