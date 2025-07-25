using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.EntityUpdaters
{
    public interface IEntityUpdater<TEntity> //IAsyncDisposable is not available in C# 7.3, we'll just use SaveChangesAsync instead
    {
        Task UpdateFieldAsync<TField>(Expression<Func<TEntity, TField>> entityField, TField newValue);
        Task<TEntity> GetEntityAsync();
        Task SaveChangesAsync();
    }
}
