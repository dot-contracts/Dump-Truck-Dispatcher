using System;

namespace DispatcherWeb.WorkOrders.Dto
{
    public class WorkOrderPictureEditDto
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public Guid FileId { get; set; }
        public string FileName { get; set; }

    }
}
