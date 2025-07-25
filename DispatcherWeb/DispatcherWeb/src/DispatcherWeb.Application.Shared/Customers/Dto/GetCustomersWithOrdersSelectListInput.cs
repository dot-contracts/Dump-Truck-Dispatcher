using System;
using DispatcherWeb.Dto;

namespace DispatcherWeb.Customers.Dto
{
    public class GetCustomersWithOrdersSelectListInput : GetSelectListInput
    {
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
    }
}
