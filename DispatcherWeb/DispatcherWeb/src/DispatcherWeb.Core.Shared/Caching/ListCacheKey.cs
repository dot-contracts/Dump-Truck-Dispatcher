namespace DispatcherWeb.Caching
{
    public abstract class ListCacheKey
    {
        public abstract string ToStringKey();

        public override string ToString()
        {
            return ToStringKey();
        }
    }
}
