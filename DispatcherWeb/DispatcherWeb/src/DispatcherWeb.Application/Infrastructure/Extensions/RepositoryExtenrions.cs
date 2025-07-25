using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Infrastructure.Extensions
{
    public static class RepositoryExtenrions
    {
        public static async Task DeleteInBatchesAsync<TEntity, TPrimaryKey>(this IRepository<TEntity, TPrimaryKey> repository, Expression<Func<TEntity, bool>> predicate, IActiveUnitOfWork currentUnitOfWork, int batchSize = 100) where TEntity : class, IEntity<TPrimaryKey>
        {
            List<TEntity> records;
            do
            {
                records = await (await repository.GetQueryAsync()).Where(predicate).Take(batchSize).ToListAsync();
                await repository.DeleteRangeAsync(records);
                await currentUnitOfWork.SaveChangesAsync();
            }
            while (records.Any());
        }

        public static async Task HardDeleteInBatchesAsync<TEntity, TPrimaryKey>(this IRepository<TEntity, TPrimaryKey> repository, Expression<Func<TEntity, bool>> predicate, IActiveUnitOfWork currentUnitOfWork, int batchSize = 100) where TEntity : class, IEntity<TPrimaryKey>, ISoftDelete
        {
            List<TEntity> records;
            do
            {
                records = await (await repository.GetQueryAsync()).Where(predicate).Take(batchSize).ToListAsync();
                foreach (var record in records)
                {
                    await repository.HardDeleteAsync(record);
                }
                await currentUnitOfWork.SaveChangesAsync();
            }
            while (records.Any());
        }
    }
}
