namespace DispatcherWeb.Infrastructure.EntityReadonlyCheckers
{
    public interface IReadonlyCheckerFactory<T>
    {
        IReadonlyChecker<T> Create(int entityId);
    }
}
