using System;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class LeaseHaulerInsuranceDto
    {
        public int Id { get; set; }

        public string InsuranceTypeName { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}
