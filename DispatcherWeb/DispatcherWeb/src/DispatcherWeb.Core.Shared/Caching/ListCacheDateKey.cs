using System;

namespace DispatcherWeb.Caching
{
    public class ListCacheDateKey : ListCacheTenantKey
    {
        public ListCacheDateKey()
        {
        }

        public ListCacheDateKey(int tenantId, DateTime date, Shift? shift)
            : base(tenantId)
        {
            Date = date;
            Shift = shift;
        }

        public DateTime Date { get; set; }
        public Shift? Shift { get; set; }

        public override string ToStringKey()
        {
            return $"{base.ToStringKey()}-{Date:yyyy-MM-dd}-{(int?)Shift}";
        }
    }
}
