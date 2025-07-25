using System.Threading.Tasks;

namespace DispatcherWeb.Infrastructure.EntityReadonlyCheckers
{
    public interface IReadonlyChecker<T>
    {
        Task<string> GetReadOnlyReasonForFieldAsync(string fieldName);
        Task<bool> IsFieldReadonlyAsync(string fieldName);
        Task ThrowIfFieldIsReadonlyAsync(string fieldName);
        IReadonlyChecker<T> SetEntity(T entity);
    }
}
