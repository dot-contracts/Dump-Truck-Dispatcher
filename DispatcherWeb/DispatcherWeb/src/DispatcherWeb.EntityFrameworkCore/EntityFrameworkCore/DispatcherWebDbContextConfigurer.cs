using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore
{
    public static class DispatcherWebDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<DispatcherWebDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString, opt => opt.CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds));
        }

        public static void Configure(DbContextOptionsBuilder<DispatcherWebDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection, opt => opt.CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds));
        }
    }
}