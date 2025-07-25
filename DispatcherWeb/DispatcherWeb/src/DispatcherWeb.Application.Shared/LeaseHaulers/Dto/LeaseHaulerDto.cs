using System;
using System.Collections.Generic;

namespace DispatcherWeb.LeaseHaulers.Dto
{
    public class LeaseHaulerDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string CountryCode { get; set; }

        public string AccountNumber { get; set; }

        public string StreetAddress1 { get; set; }

        public string StreetAddress2 { get; set; }

        public string MailingAddress1 { get; set; }

        public string MailingAddress2 { get; set; }

        public string MailingCity { get; set; }

        public string MailingState { get; set; }

        public string MailingCountryCode { get; set; }

        public string MailingZipCode { get; set; }

        public string MotorCarrierNumber { get; set; }

        public string DeptOfTransportationNumber { get; set; }

        public string EinOrTin { get; set; }

        public bool IsActive { get; set; }

        public DateTime? HireDate { get; set; }

        public DateTime? TerminationDate { get; set; }

        public string PhoneNumber { get; set; }

        public ICollection<LeaseHaulerInsuranceDto> Insurances { get; set; }
    }
}
