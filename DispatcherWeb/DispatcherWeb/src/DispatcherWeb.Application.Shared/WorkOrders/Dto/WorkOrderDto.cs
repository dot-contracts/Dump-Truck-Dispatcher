using System;

namespace DispatcherWeb.WorkOrders.Dto
{
    public class WorkOrderDto
    {
        public int Id { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; }
        public string Vehicle { get; set; }
        public string Note { get; set; }
        public decimal Odometer { get; set; }
        public string AssignedTo { get; set; }
        public bool CanEdit { get; set; }
    }
}
