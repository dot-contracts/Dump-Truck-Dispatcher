using System;

namespace DispatcherWeb.Infrastructure
{
    public class OrderAttribute : Attribute
    {
        public OrderAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; }
    }
}
