using System;

namespace DispatcherWeb.Trucks.Dto
{
    public class InsuranceDto
    {
        public int Id { get; set; }
        public int LeaseHaulerId { get; set; }
        public bool IsActive { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
