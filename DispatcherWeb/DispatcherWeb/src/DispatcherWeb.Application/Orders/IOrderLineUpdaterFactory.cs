namespace DispatcherWeb.Orders
{
    public interface IOrderLineUpdaterFactory
    {
        IOrderLineUpdater Create(int entityId, int[] alreadySyncedOrderLineIds = null);
    }
}
