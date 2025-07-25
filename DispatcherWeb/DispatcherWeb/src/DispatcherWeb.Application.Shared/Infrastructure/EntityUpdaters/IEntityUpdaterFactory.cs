namespace DispatcherWeb.Infrastructure.EntityUpdaters
{
    public interface IEntityUpdaterFactory<TEntity>
    {
        IEntityUpdater<TEntity> Create(int entityId);
    }
}
