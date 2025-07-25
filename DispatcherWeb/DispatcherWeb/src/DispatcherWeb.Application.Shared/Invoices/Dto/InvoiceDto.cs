using System;
using System.Collections.Generic;
using System.Linq;

namespace DispatcherWeb.Invoices.Dto
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public InvoiceStatus Status { get; set; }
        public string StatusName => Status.GetDisplayName();
        public string CustomerName { get; set; }
        public bool CustomerHasMaterialCompany { get; set; }
        public string JobNumber => JobNumbers?.Any() == true ? string.Join("; ", JobNumbers) : null;
        public string JobNumberSort { get; set; }
        public List<string> JobNumbers { get; set; }
        public DateTime? IssueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? QuickbooksExportDateTime { get; set; }
        public List<EmailDeliveryStatus> EmailDeliveryStatuses { get; set; }
        public EmailDeliveryStatus? CalculatedEmailDeliveryStatus => EmailDeliveryStatuses?.GetLowestStatus();
    }
}
