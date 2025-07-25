using System;

namespace DispatcherWeb.Payments.Dto
{
    public class UpdatePaymentsFromHeartlandInput
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AllOffices { get; set; }
    }
}
