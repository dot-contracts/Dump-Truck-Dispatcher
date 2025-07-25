using System;

namespace DispatcherWeb.Orders.Dto
{
    public class CopyOrderInput
    {
        public int OrderId { get; set; }
        public int? OrderLineId { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        public Shift[] Shifts { get; set; }
        public bool CopyTrucks { get; set; }
        public bool CarryUnfinishedPortionForward { get; set; }
    }
}
