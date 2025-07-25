using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace DispatcherWeb.UnitTests.TestUtilities
{
    public static class ExtensionsMethods
    {
        public static DbSet<T> Initialize<T>(this DbSet<T> dbSet, IQueryable<T> data) where T : class
        {
            ((IQueryable<T>)dbSet).Provider.Returns(new TestDbAsyncQueryProvider<T>(data.Provider));

            ((IQueryable<T>)dbSet).Expression.Returns(data.Expression);
            ((IQueryable<T>)dbSet).ElementType.Returns(data.ElementType);
            ((IQueryable<T>)dbSet).GetEnumerator().Returns(data.GetEnumerator());

            return dbSet;
        }

        public static DbSet<T> CreateMockSet<T>(this IList<T> data) where T : class
        {
            return CreateMockSet(data.AsQueryable());
        }

        public static DbSet<T> CreateMockSet<T>(this IQueryable<T> data) where T : class
        {
            return Substitute.For<DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>>().Initialize(data);
        }
    }
}
