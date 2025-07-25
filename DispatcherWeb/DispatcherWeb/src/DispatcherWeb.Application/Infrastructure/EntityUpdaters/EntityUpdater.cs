using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abp.Localization;

namespace DispatcherWeb.Infrastructure.EntityUpdaters
{
    public abstract class EntityUpdater<TEntity>
    {
        protected readonly ILocalizationManager _localizationManager;

        public EntityUpdater(
            ILocalizationManager localizationManager
            )
        {
            _localizationManager = localizationManager;
        }

        protected string L(string name)
        {
            return _localizationManager.GetString(DispatcherWebConsts.LocalizationSourceName, name);
        }

        public async Task UpdateFieldAsync<TField>(Expression<Func<TEntity, TField>> entityField, TField newValue)
        {
            var newValueParam = Expression.Parameter(entityField.Body.Type, "value");
            var assign = Expression.Lambda<Action<TEntity, TField>>(
                Expression.Assign(entityField.Body, newValueParam),
                entityField.Parameters[0], newValueParam);

            var getter = entityField.Compile();
            var setter = assign.Compile();

            var entityFieldName = ((MemberExpression)entityField.Body).Member.Name;

            var entity = await GetEntityAsync();

            await UpdateFieldAsync(entity, entityFieldName, getter(entity), newValue, (val) => setter(entity, val));
        }

        public abstract Task<TEntity> GetEntityAsync();

        protected abstract Task UpdateFieldAsync<TField>(TEntity entity, string fieldName, TField oldValue, TField newValue, Action<TField> setValue);
    }
}
