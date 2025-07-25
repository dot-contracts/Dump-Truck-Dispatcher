namespace DispatcherWeb.SyncRequests.Entities
{
    public abstract class ChangedEntityAbstract
    {
        public virtual bool IsSame(ChangedEntityAbstract obj)
        {
            return true;
        }
    }

    public class ChangedEntityId<TKey> : ChangedEntityAbstract
    {
        public TKey Id { get; set; }

        public ChangedEntityId()
        {
        }

        public ChangedEntityId(TKey id)
        {
            Id = id;
        }

        public override bool IsSame(ChangedEntityAbstract obj)
        {
            return obj is ChangedEntityId<TKey> other
                && other.Id.Equals(Id)
                && base.IsSame(obj);
        }
    }


    public class ChangedEntityId : ChangedEntityId<int>
    {
        public ChangedEntityId()
        {
        }

        public ChangedEntityId(int id) : base(id)
        {
        }
    }
}
