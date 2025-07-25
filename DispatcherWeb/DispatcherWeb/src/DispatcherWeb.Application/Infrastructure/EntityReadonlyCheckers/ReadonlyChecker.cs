using System.Threading.Tasks;
using Abp.Localization;
using Abp.UI;

namespace DispatcherWeb.Infrastructure.EntityReadonlyCheckers
{
    public abstract class ReadonlyChecker<TEntity> : IReadonlyChecker<TEntity> where TEntity : class
    {
        protected readonly ILocalizationManager _localizationManager;

        public ReadonlyChecker(
            ILocalizationManager localizationManager
            )
        {
            _localizationManager = localizationManager;
        }

        public async Task<bool> IsFieldReadonlyAsync(string fieldName)
        {
            var reason = await GetReadOnlyReasonForFieldAsync(fieldName);
            return !string.IsNullOrEmpty(reason);
        }

        public async Task ThrowIfFieldIsReadonlyAsync(string fieldName)
        {
            var reason = await GetReadOnlyReasonForFieldAsync(fieldName);
            if (!string.IsNullOrEmpty(reason))
            {
                throw new UserFriendlyException(reason);
            }
        }

        public abstract Task<string> GetReadOnlyReasonForFieldAsync(string fieldName);

        protected string L(string name)
        {
            return _localizationManager.GetString(DispatcherWebConsts.LocalizationSourceName, name);
        }


        protected TEntity _entity = null;
        protected abstract Task<TEntity> GetEntityAsync();

        public IReadonlyChecker<TEntity> SetEntity(TEntity entity)
        {
            _entity = entity;
            return this;
        }
    }
}
